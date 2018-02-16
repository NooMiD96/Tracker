using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using System.Linq;
using System.IO.Compression;
using System.Collections.Concurrent;
using System.Xml;
using System.Xml.Linq;
using System.Configuration;
using System.Collections.Specialized;

using Microsoft.Extensions.Configuration;

/// <summary>
/// TODO:
///     compressing
///     start setting 
///     replace file
///     scaning when first start
///     delete file.zip after send
///     When delete 2 equal folder analizer return MoveEvent !!
/// </summary>
namespace FilesMonitoring {
    public class TrackerProgram {
        static private FileSystemWatcher[] _watchers;
        static private bool _isEventsRecord = false;
        static private Timer _timer = new Timer();
        static private int _timerInterval = 500;

        static private System.Threading.EventWaitHandle ewh = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.ManualReset);

        static ConcurrentQueue<TrackerEvent> eventList = new ConcurrentQueue<TrackerEvent>();
        static private Analizer analizer = new Analizer(eventList);

        static private int _trackerId;

        static private string _ip;
        static private int _port;

        static private Socket socket;

        static private object lockObjAnalizeEventList = new object();
        static private object lockObjSendEventList = new object();

        static private string dbName;
        static private string connectionString;
        static private SQLiteDb _context;

        static private string[] _path;
        static private string[] _extensions;
        static private string[] _ignores;

        public TrackerProgram(string[] args) {
            ParseConfig();

            Analizer.ignores = _ignores;
            Analizer.extensions = _extensions;

            TryConnectSQLite();
            TryCreateWatcher(_path);

            Task.Factory.StartNew(DbListener);

            SetEventRecordTimer();

            Console.WriteLine("Input text for exit");
            Console.ReadLine();
        }
        private void ParseConfig() {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddXmlFile("app.config");

            var configuration = builder.Build();

            bool parsed = false;
            //db
            connectionString = configuration.GetConnectionString("connectionString");
            dbName = connectionString.Split('=')[1];
            //server
            _ip = configuration["ServerIP:value"];
            parsed = Int32.TryParse(configuration["ServerPort:value"], out _port);
            if(!parsed) {
                _port = 8000;
            }
            //directories
            _path = configuration["Dirs:value"].Split(';');
            //extensions and ignores
            var extensions = configuration["Extensions:value"];
            if(string.IsNullOrEmpty(extensions)) {
                _ignores = configuration["Ignores:value"].Split(';');
            } else {
                _extensions = extensions.Split(';');
            }

            //trackerId(if exist)
            var trackerId = configuration["TrackerId:value"];
            if(string.IsNullOrEmpty(trackerId)) {
                _trackerId = -1;
            } else {
                parsed = Int32.TryParse(trackerId, out _trackerId);
                if(!parsed) {
                    _trackerId = -1;
                }
            }
        }
        private void SaveInXmlTrackerId() {
            XmlDocument Config = new XmlDocument();
            Config.Load("app.config");
            XmlElement xRoot = Config.DocumentElement;
            var add = xRoot.GetElementsByTagName("TrackerId").Item(0);
            add.Attributes["value"].Value = _trackerId.ToString();
            Config.Save("app.config");
        }

        private void TryCreateWatcher(string[] paths) {
            _watchers = new FileSystemWatcher[paths.Length];
            for(int i = 0; i < paths.Length; i++) {
                var watcher = new FileSystemWatcher();
                watcher.Path = paths[i];
                watcher.Filter = "*.*";

                watcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.DirectoryName |
                    NotifyFilters.FileName | NotifyFilters.LastAccess | NotifyFilters.LastWrite;

                watcher.IncludeSubdirectories = true;

                watcher.Changed += new FileSystemEventHandler(OnChanged);
                watcher.Created += new FileSystemEventHandler(OnChanged);
                watcher.Deleted += new FileSystemEventHandler(OnChanged);
                watcher.Renamed += new RenamedEventHandler(OnRenamed);
                watcher.Error += new ErrorEventHandler(OnError);

                watcher.EnableRaisingEvents = true;

                _watchers[i] = watcher;
            }
        }
        private void TryConnectSQLite() {
            SQLiteDb.ConnectingString = connectionString;
            _context = new SQLiteDb();
            _context.Database.Migrate();
            //_context.RemoveAll();
        }
        private bool TryConnectSocket() {
            try {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                var ipPoint = new IPEndPoint(IPAddress.Parse(_ip), _port);
                socket.Connect(ipPoint);

                //connected work
                byte[] buffer = new byte[128];
                StringBuilder sb = new StringBuilder();
                string str;

                str = SocketHelper.GetMessageFromServer(buffer, sb, socket);

                if(str == "GET_INFO") {
                    var item = _trackerId;

                    SocketHelper.SendItemToServer(item, socket);

                    str = SocketHelper.GetMessageFromServer(buffer, sb, socket);
                    if(str.Contains("trackerId=")) {
                        Int32.TryParse(str.Replace("trackerId=", ""), out _trackerId);
                        SaveInXmlTrackerId();
                    }
                }
                //end first connect
                return true;
            } catch {
                return false;
            }
        }

        private void DbListener() {
            bool isConnected = TryConnectSocket();
            bool isNeedSendEventsAgain = false;
            bool isNeedSendExceptionsAgain = false;

            while(true) {
                ewh.WaitOne();
                System.Threading.Thread.Sleep(1000);
                if(_isEventsRecord) {
                    ewh.Reset();
                    continue;
                }
                if(isConnected) {
                    isNeedSendEventsAgain = _context.SendTrackerEvents(socket, ref isConnected);
                    isNeedSendExceptionsAgain = _context.SendExceptions(socket, ref isConnected);
                    if(!isNeedSendEventsAgain && !isNeedSendExceptionsAgain) {
                        ewh.Reset();
                    }
                } else {
                    isConnected = TryConnectSocket();
                    if(!isConnected) {
                        System.Threading.Thread.Sleep(1000);
                    }
                }
            }
        }

        private void SetEventRecordTimer() {
            _timer.Elapsed += new ElapsedEventHandler(SendChangeToServer);
            _timer.Interval = 1000;
        }
        private void TimerReset() {
            _timer.Stop();
            _timer.Interval = _timerInterval;
            _isEventsRecord = false;
        }
        private void StartOrContinueRecord() {
            if(!_isEventsRecord) {
                _isEventsRecord = true;
                _timer.Start();
            }
            //} else {
            //_timer.Interval += 10;
            //}
        }

        private void OnError(object sender, ErrorEventArgs e) {
            _context.AddException(e.GetException());
        }
        private void OnRenamed(object sender, RenamedEventArgs e) {
            Console.WriteLine($"Was renamed file {e.OldName} to {e.Name}");

            eventList.Enqueue(new TrackerEvent(e.Name, e.OldName, e.FullPath, e.OldFullPath, TrackerEvents.Renamed));

            StartOrContinueRecord();
        }
        private void OnChanged(object sender, FileSystemEventArgs e) {
            switch(e.ChangeType.ToString()) {
                case nameof(TrackerEvents.Created):
                    Console.WriteLine($"Was create file with name - {e.FullPath}");

                    eventList.Enqueue(new TrackerEvent(e.Name, e.FullPath, TrackerEvents.Created));

                    StartOrContinueRecord();

                    break;

                case nameof(TrackerEvents.Changed):
                    Console.WriteLine($"Was changed file with name - {e.FullPath}");

                    eventList.Enqueue(new TrackerEvent(e.Name, e.FullPath, TrackerEvents.Changed));

                    StartOrContinueRecord();
                    break;

                case nameof(TrackerEvents.Deleted):
                    Console.WriteLine($"Was deleted file with name - {e.FullPath}");

                    eventList.Enqueue(new TrackerEvent(e.Name, e.FullPath, TrackerEvents.Deleted));

                    StartOrContinueRecord();
                    break;

                default:
                    throw new Exception($"{e.ChangeType} not supported");
            }
        }

        private void SendChangeToServer(object sender, ElapsedEventArgs e) {
            TimerReset();

            _context.AddEvents(analizer.Analize());
            //say can send to server
            ewh.Set();
        }
    }
}
