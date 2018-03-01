using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using FilesMonitoring;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq;
using System.Threading.Tasks;

namespace FilesMonitoringServer {
    class TrackerDb :DbContext {
        static private object lockObj = new object();

        public DbSet<Tracker> Trackers { get; set; }
        public DbSet<File> Files { get; set; }
        public DbSet<Change> Changes { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Content> Contents { get; set; }
        public DbSet<ClientException> ClientExceptions { get; set; }

        static public string ConnectingString = @"Data Source=(localdb)\mssqllocaldb;Initial Catalog=Tracker;Integrated Security=True;MultipleActiveResultSets=True";

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            optionsBuilder.UseSqlServer(ConnectingString);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Tracker>()
                .HasMany(tr => tr.UserList)
                .WithOne(us => us.Tracker)
                .HasForeignKey(us => us.TrackerId);
            modelBuilder.Entity<Tracker>()
                .HasMany(tr => tr.FileList)
                .WithOne(fl => fl.Tracker)
                .HasForeignKey(fl => fl.TrackerId);
            modelBuilder.Entity<Change>()
                .Property(ch => ch.EventName)
                .HasColumnType("TrackerEvents");
            //modelBuilder.Entity<File>()
            //    .Property(f => f.IsWasDeletedChange)
            //    .HasDefaultValue(false);
            //modelBuilder.Entity<File>()
            //    .Property(f => f.IsNeedDelete)
            //    .HasDefaultValue(true);
        }

        public bool Contains(int trackerId) {
            var db = this;
            var tracker = db.Trackers
                .FirstOrDefault(tr => tr.TrackerId.Equals(trackerId));

            if(tracker != null) return true;
            else return false;
        }
        public int AddTracker(){
            var index = Trackers
                .OrderBy(tr => tr.TrackerId)
                .Count() + 1;

            var tracker = new Tracker() {
                TrackerId = index,
            };

            lock(lockObj) {
                Trackers.Add(tracker);

                SaveChanges();
            }
            return tracker.TrackerId;
        }

        internal void RemoveFiles(IEnumerable<File> files) {
            lock(lockObj) {
                Files.RemoveRange(files);

                SaveChanges();
            }
        }

        public int AddTracker(int trackerId) {
            var tracker = new Tracker() {
                TrackerId = trackerId
            };

            lock(lockObj) {
                Trackers.Add(tracker);

                SaveChanges();
            }
            return tracker.TrackerId;
        }
        private void AddChange(TrackerEvent evnt, Change change, string path, int trackerId) {
            var db = this;
            var tracker = db.Trackers
                .Include(tr => tr.FileList)
                .FirstOrDefault(tr => tr.TrackerId == trackerId);

            if(tracker == null) return;

            File file;
            bool newFile = false;

            if(evnt.EventName == TrackerEvents.Moved || evnt.EventName == TrackerEvents.Renamed) {
                file = tracker.FileList
                    .FirstOrDefault(fl => fl.FullName.Equals(evnt.OldFullName));

                if(file == null) {
                    file = new File() {
                        FullName = evnt.FullName,
                        FileName = evnt.Name,
                    };
                    newFile = true;
                }

                if(evnt.EventName == TrackerEvents.Moved) {
                    file.FullName = evnt.FullName;
                } else {
                    file.FileName = evnt.Name;
                    file.FullName = evnt.FullName;
                }
            } else { 
                file = tracker.FileList
                    .FirstOrDefault(fl => fl.FullName.Equals(evnt.FullName));

                if(file == null) {
                    file = new File() {
                        FullName = evnt.FullName,
                        FileName = evnt.Name,
                    };
                    newFile = true;
                }
                if(evnt.EventName == TrackerEvents.Created)
                {
                    file.IsWasDeletedChange = false;
                    file.IsNeedDelete = false;
                    file.RemoveFromDbTime = null;
                }
                if(evnt.EventName == TrackerEvents.Deleted) {
                    file.IsWasDeletedChange = true;
                    file.RemoveFromDbTime = DateTime.UtcNow.AddDays(14);
                } 
            }

            if(!String.IsNullOrEmpty(path)) {
                file.FilePath = path;
            }

            lock(lockObj) {
                file.ChangeList.Add(change);
                if(newFile) {
                    tracker.FileList.Add(file);
                }
                db.SaveChanges();
            }
        }
        public void AddException(int trackerId, string userName, DateTime date, string exception) {
            var trackerAndUserId = (
                    from t in Trackers
                    join u in Users on t.TrackerId equals u.TrackerId
                    where t.TrackerId.Equals(trackerId)
                    select new {
                        userId = u.UserId,
                        tracker = t,
                    }
                ).FirstOrDefault();

            lock(lockObj) {
                trackerAndUserId
                    .tracker
                    .ClientExceptionList
                    .Add(new ClientException() {
                        DateTime = date,
                        UserId = trackerAndUserId.userId,
                        ExceptionInner = exception,
                    });
                SaveChanges();
            }
        }

        public void ChangedEvent(TrackerEvent evnt, string path, int trackerId) {
            var userId = GetUserId(evnt, trackerId);

            Content content = new Content() {
                Payload = evnt.Content,
                FilePath = path,
            };

            var change = new Change() {
                UserId = userId,
                EventName = evnt.EventName,
                DateTime = evnt.DateTime,
                Content = content,
            };

            AddChange(evnt, change, path, trackerId);
        }

        public void CreatedEvent(TrackerEvent evnt, string path, int trackerId) {
            var userId = GetUserId(evnt, trackerId);

            Content content = new Content() {
                Payload = evnt.Content,
                FilePath = path,
            };

            var change = new Change() {
                UserId = userId,
                EventName = evnt.EventName,
                DateTime = evnt.DateTime,
                Content = content,
            };

            AddChange(evnt, change, path, trackerId);
        }
        public void RenamedEvent(TrackerEvent evnt, int trackerId) {
            var userId = GetUserId(evnt, trackerId);
            var change = new Change() {
                EventName = evnt.EventName,
                DateTime = evnt.DateTime,
                OldName = evnt.OldName,
                OldFullName = evnt.OldFullName,
                UserId = userId,
            };

            AddChange(evnt, change, null, trackerId);
        }
        public void MovedEvent(TrackerEvent evnt, int trackerId) {
            var userId = GetUserId(evnt, trackerId);
            var change = new Change() {
                EventName = evnt.EventName,
                DateTime = evnt.DateTime,
                OldName = evnt.OldName,
                OldFullName = evnt.OldFullName,
                UserId = userId,
            };

            AddChange(evnt, change, null, trackerId);
        }
        public void DeletedEvent(TrackerEvent evnt, int trackerId) {
            var userId = GetUserId(evnt, trackerId);
            var change = new Change() {
                EventName = evnt.EventName,
                DateTime = evnt.DateTime,
                UserId = userId,
            };

            AddChange(evnt, change, null, trackerId);
        }

        public void DeleteDir(TrackerEvent evnt, int trackerId)
        {
            var files = (
                    from t in Trackers
                    join f in Files on t.TrackerId equals f.TrackerId
                    where t.TrackerId.Equals(trackerId) && f.FullName.IndexOf(evnt.FullName) == 0
                    select f
                ).ToList();

            lock(lockObj)
            {
                files.ForEach(file => {
                    file.ChangeList.Add(new Change()
                    {
                        UserId = GetUserId(evnt, trackerId),
                        EventName = TrackerEvents.Deleted,
                        DateTime = evnt.DateTime,
                    });
                    file.IsWasDeletedChange = true;
                    file.IsNeedDelete = true;
                    file.RemoveFromDbTime = DateTime.UtcNow.AddDays(14);
                });
                SaveChanges();
            }
        }
        public void RenameDir(TrackerEvent evnt, int trackerId)
        {
            var files = (
                    from t in Trackers
                    join f in Files on t.TrackerId equals f.TrackerId
                    where t.TrackerId.Equals(trackerId) && f.FullName.IndexOf(evnt.OldFullName) == 0
                    select f
                ).ToList();

            lock(lockObj)
            {
                files.ForEach(file => {
                    file.ChangeList.Add(new Change()
                    {
                        UserId = GetUserId(evnt, trackerId),
                        EventName = TrackerEvents.Renamed,
                        DateTime = evnt.DateTime,
                        OldName = file.FileName,
                        OldFullName = file.FullName
                    });
                    file.FullName = file.FullName.Replace(evnt.OldFullName, evnt.FullName);
                });
                SaveChanges();
            }
        }
        public void MoveDir(TrackerEvent evnt, int trackerId)
        {
            var files = (
                    from t in Trackers
                    join f in Files on t.TrackerId equals f.TrackerId
                    where t.TrackerId.Equals(trackerId) && f.FullName.Equals(evnt.OldFullName)
                    select f
                ).ToList();

            lock(lockObj)
            {
                files.ForEach(file => {
                    file.ChangeList.Add(new Change()
                    {
                        UserId = GetUserId(evnt, trackerId),
                        EventName = TrackerEvents.Moved,
                        DateTime = evnt.DateTime,
                    });
                    file.FullName = evnt.FullName;
                });
                SaveChanges();
            }
        }
        public void CreateDir(TrackerEvent evnt, int trackerId)
        {
            var files = (
                    from t in Trackers
                    join f in Files on t.TrackerId equals f.TrackerId
                    join c in Changes on f.FileId equals c.FileId
                    where t.TrackerId.Equals(trackerId) && f.FullName.Equals(evnt.FullName)
                    select f
                ).ToList();

            lock(lockObj)
            {
                files.ForEach(file => {
                    file.ChangeList.Add(new Change()
                    {
                        UserId = GetUserId(evnt, trackerId),
                        EventName = TrackerEvents.Created,
                        DateTime = evnt.DateTime,
                    });
                    file.IsWasDeletedChange = false;
                    file.IsNeedDelete = false;
                    file.RemoveFromDbTime = null;
                });
                SaveChanges();
            }
        }

        public IQueryable<File> GetFilesToDelete(DateTime now) => (
            from f in Files
            where f.IsNeedDelete.Equals(true) && f.RemoveFromDbTime.HasValue && DateTime.Compare(f.RemoveFromDbTime.Value, now) < 0
            select f);

        public List<Content> GetContentsToDelete(IQueryable<File> files) => (
                from f in files
                join ch in Changes on f.FileId equals ch.FileId
                join co in Contents on ch.ContentId equals co.ContentId
                where !String.IsNullOrEmpty(co.FilePath)
                select co
            ).ToList();

        private int GetUserId(TrackerEvent evnt, int trackerId) {
            var user = (
                from u in Users
                where u.UserName.Equals(evnt.UserName) && u.TrackerId.Equals(trackerId)
                select u
                ).FirstOrDefault();

            if(user == null) {
                user = new User() {
                    TrackerId = trackerId,
                    UserName = evnt.UserName,
                };
                Users.Add(user);
                lock(lockObj) {
                    SaveChanges();
                }
            }

            return user.UserId;
        }
    }

    class Tracker {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None), ForeignKey(nameof(File))]
        public int TrackerId { get; set; }

        //children
        public Tracker() { FileList = new List<File>(); UserList = new List<User>(); ClientExceptionList = new List<ClientException>(); }
        public ICollection<File> FileList { get; set; }
        public ICollection<User> UserList { get; set; }
        public ICollection<ClientException> ClientExceptionList { get; set; }
    }
    class User {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public int TrackerId { get; set; }
        [Required]
        public bool IsCanAuthohorization { get; set; }

        //parent
        public Tracker Tracker { get; set; }
    }
    class File {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), ForeignKey(nameof(Change))]
        public int FileId { get; set; }
        [Required]
        public int TrackerId { get; set; }
        [Required]
        public string FileName { get; set; }
        [Required]
        public string FullName { get; set; }
        public string FilePath { get; set; }
        public bool IsWasDeletedChange { get; set; } 
        public DateTime? RemoveFromDbTime { get; set; }
        public bool IsNeedDelete { get; set; } 
        //parent
        public Tracker Tracker { get; set; }
        //children
        public File() { ChangeList = new List<Change>(); }
        public ICollection<Change> ChangeList { get; set; }
    }
    class Change {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [Required]
        public int FileId { get; set; }
        [Required]
        public TrackerEvents EventName { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public DateTime DateTime { get; set; }
        [ForeignKey(nameof(Content))]
        public int? ContentId { get; set; }
        public string OldName { get; set; }
        public string OldFullName { get; set; }

        //parent
        public File File { get; set; }
        //children
        public Content Content { get; set; }
    }
    class Content {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ContentId { get; set; }
        //[Required]
        public byte[] Payload { get; set; }
        public string FilePath { get; set; }

        //parent
        public Change Change { get; set; }
    }
    class ClientException {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ExceptionId { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public int TrackerId { get; set; }
        [Required]
        public DateTime DateTime { get; set; }
        [Required]
        public string ExceptionInner { get; set; }
    }
}
