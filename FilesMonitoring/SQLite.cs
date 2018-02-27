using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using FilesMonitoring;
using System.IO.Compression;
using Newtonsoft.Json;

namespace FilesMonitoring {
    public class SQLiteDb :DbContext {
        public DbSet<TrackerEvent> TrackerEvent { get; set; }
        public DbSet<TrackerEventInfo> TrackerEventInfo { get; set; }
        public DbSet<ClientException> ClientExceptions { get; set; }

        private object objLock = new object();

        private long sizeLimit = 10000000;

        static public string ConnectingString = "Data Source=temp.db";

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            optionsBuilder.UseSqlite(ConnectingString);
            SQLitePCL.Batteries.Init();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<TrackerEvent>()
                .HasKey("id");
        }

        public void AddEvents(List<TrackerEvent> items) {
            if(items == null || items.Count == 0) {
                return;
            }

            FileInfo fi;
            var teInfos = new List<TrackerEventInfo>();

            foreach(var item in items) {
                var teInfo = new TrackerEventInfo {
                    TrackerEvent = item
                };

                switch(item.EventName)
                {
                    case TrackerEvents.Changed:
                    case TrackerEvents.Created:
                        fi = new FileInfo(item.FullName);
                        if(fi.Length < sizeLimit)
                        {
                            //TODO: catch exaption
                            try
                            {
                                GetContentFromFile(fi, item);

                                teInfo.IsContentInTrackerEvent = true;

                            } catch
                            {
                                continue;
                            }
                        } else
                        {
                            var path = ArchivetedFile(item.Name, item.FullName);
                            if(!String.IsNullOrEmpty(path))
                            {
                                teInfo.IsContentInTrackerEvent = false;
                                teInfo.PathToContent = path;
                            }
                        }
                        break;

                    default:
                        teInfo.IsContentInTrackerEvent = true;

                        break;
                }

                teInfos.Add(teInfo);
            }
            lock(objLock) {
                TrackerEventInfo.AddRange(teInfos);
                SaveChanges();
            }
        }

        public bool SendTrackerEvents(Socket socket, ref bool isConnected) {
            List<TrackerEventInfo> teInfos;
            bool isNeedSendAgain = true;
            lock(objLock) {
                teInfos = TrackerEventInfo
                    .Include(te => te.TrackerEvent)
                    .Take(100)
                    .ToList();
            }

            if(teInfos == null || teInfos.Count == 0) {
                return false;
            }
            if(teInfos.Count < 100) {
                isNeedSendAgain = false;
            }

            var listToDelete = new List<TrackerEventInfo>();

            foreach(var teInfo in teInfos) {
                try {
                    SocketHelper.SendItemToServer(teInfo.TrackerEvent, socket);

                    if(!teInfo.IsContentInTrackerEvent){
                        SocketHelper.SendZipArchive(socket, teInfo.PathToContent);
                        File.Delete(teInfo.PathToContent);
                    }

                    listToDelete.Add(teInfo);
                } catch {
                    isConnected = false;
                    isNeedSendAgain = true;
                    break;
                }
            }

            RemoveList(listToDelete);
            return isNeedSendAgain;
        }
        public bool SendExceptions(Socket socket, ref bool isConnected) {
            List<ClientException> clientExceptionList;
            bool isNeedSendAgain = false;
            lock(objLock) {
                clientExceptionList = ClientExceptions
                    .Take(100)
                    .ToList();
            }

            if(clientExceptionList == null || clientExceptionList.Count == 0) {
                return false;
            }
            if(clientExceptionList.Count < 100) {
                isNeedSendAgain = false;
            }

            var listToDelete = new List<ClientException>();

            foreach(var clientException in clientExceptionList) {
                try {
                    SocketHelper.SendExaptionToServer(clientException.UserName, clientException.DateTime, clientException.ExceptionInner, socket);

                    listToDelete.Add(clientException);
                } catch {
                    isConnected = false;
                    isNeedSendAgain = true;
                    break;
                }
            }

            RemoveExceptionList(listToDelete);
            return isNeedSendAgain;
        }

        private void RemoveExceptionList(List<ClientException> list) {
            RemoveRange(list);
            SaveChanges();
        }
        private void RemoveList(List<TrackerEventInfo> teInfos) {
            if(teInfos.Count == 0) {
                return;
            }
            lock(objLock) {
                foreach(var teInfo in teInfos) {
                    Remove(teInfo.TrackerEvent);
                }
                RemoveRange(teInfos);

                SaveChanges();
            }
        }
        public void RemoveAll() {
            lock(objLock) {
                RemoveRange(TrackerEvent.ToList());
                RemoveRange(TrackerEventInfo.ToList());

                SaveChanges();
            }
        }

        private void GetContentFromFile(FileInfo fi, TrackerEvent item) {
            using(var fsRead = fi.OpenRead())
            {
                var buffer = new byte[fsRead.Length];
                int readed;

                readed = fsRead.Read(buffer, 0, buffer.Length);
                item.Content = buffer;
            }
        }
        private static string ArchivetedFile(string name, string fullName) {
            if(!File.Exists(fullName)) return null;

            string path = fullName.Split('\\')[0] + @"\tmpFolder\";
            if(!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }

            path += Guid.NewGuid().ToString() + ".zip";

            string extension = Path.GetExtension(fullName);
            if(extension.Contains(".zip")) {
                File.Copy(fullName, path);
            } else {
                using(FileStream zipToCreate = new FileStream($"{path}", FileMode.Create)) {
                    using(ZipArchive zip = new ZipArchive(zipToCreate, ZipArchiveMode.Create)) {
                        zip.CreateEntryFromFile(fullName, name, CompressionLevel.NoCompression);
                    }
                }
            }
            return path;
        }

        internal void AddException(Exception exception) {
            lock(objLock) {
                ClientExceptions
                    .Add(new ClientException() {
                        DateTime = DateTime.Now,
                        UserName = Environment.UserName,
                        ExceptionInner = exception.Message + '\n' + exception.InnerException,
                    });
                SaveChanges();
            }
        }

        public void AddDirEvent(TrackerEvent trackerEvent)
        {
            TrackerEventInfo dirEvent = new TrackerEventInfo()
            {
                IsContentInTrackerEvent = true,
                TrackerEvent = trackerEvent,
            };
            lock(objLock)
            {
                TrackerEventInfo.Add(dirEvent);
                SaveChanges();
            }
        }
    }

    public class TrackerEventInfo {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), ForeignKey(nameof(TrackerEvent))]
        public int TrackerEventInfoId { get; set; }
        [Required]
        public bool IsContentInTrackerEvent { get; set; }
        public string PathToContent { get; set; }

        //children
        public TrackerEvent TrackerEvent { get; set; }
    }
    public class ClientException {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ExceptionId { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public DateTime DateTime { get; set; }
        [Required]
        public string ExceptionInner { get; set; }
    }
}
