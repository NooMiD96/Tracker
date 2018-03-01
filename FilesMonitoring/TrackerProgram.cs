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
using System.Text.RegularExpressions;

/// <summary>
/// TODO:
///     for client program:
///         /compressing
///         /replace file (01.03.2018 need test)
///         /scaning when first start ?? (dont needed)
///         /When delete 2 equal folder analizer return MoveEvent !! (01.03.2018 this can was fixed becouse remade analizer)
///     
///     for server:
///         /
/// 
///     for site:
///         /
/// </summary>
namespace FilesMonitoring {
    public class TrackerProgram {
        static private FileSystemWatcher[,] _watchers;
        static private bool _isEventsRecord = false;
        static private bool _isEventsDirRecord = false;
        static private Timer _timer = new Timer();
        static private Timer _timerDir = new Timer();
        static private int _timerInterval = 500;
        static private int _timerDirInterval = 1500;

        static private System.Threading.EventWaitHandle ewh = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.ManualReset);

        static ConcurrentQueue<TrackerEvent> eventList = new ConcurrentQueue<TrackerEvent>();
        static ConcurrentQueue<TrackerEvent> dirEventList = new ConcurrentQueue<TrackerEvent>();
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

        static private Regex _regexExtensions;
        static private Regex _regexIgnores;

        public TrackerProgram(string[] args) {
            ParseConfig();

            Analizer.ignores = _ignores;

            TryConnectSQLite();
            TryCreateWatcher(_path, _extensions);

            Task.Factory.StartNew(DbListener);

            SetEventRecordTimer();
            SetEventDirRecordTimer();

            InitRegexs();

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
            //extensions 
            var extensions = configuration["Extensions:value"];
            if(!string.IsNullOrEmpty(extensions))
                _extensions = extensions.Split(';');
            else
                _extensions = new string[] { "*.*" };
            //ignores
            var ignores = configuration["Ignores:value"];
            if(!string.IsNullOrEmpty(ignores))
                _ignores = ignores.Split(';');
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

        private void TryCreateWatcher(string[] paths, string[] extensions) {
            _watchers = new FileSystemWatcher[paths.Length, extensions.Length + 1];

            var notify = NotifyFilters.FileName | NotifyFilters.LastWrite;
            var onChange = new FileSystemEventHandler(OnChanged);
            var onRename = new RenamedEventHandler(OnRenamed);
            var onError = new ErrorEventHandler(OnError);

            var onChangeDir = new FileSystemEventHandler(OnChangedDir);
            var onRenameDir = new RenamedEventHandler(OnRenamedDir);
            var onErrorDir = new ErrorEventHandler(OnErrorDir);

            FileSystemWatcher watcher;

            for(int i = 0; i < paths.Length; i++) {
                for(int j = 0; j < _extensions.Length; j++)
                {
                    watcher = new FileSystemWatcher
                    {
                        Path = paths[i],
                        Filter = _extensions[j],
                        NotifyFilter = notify,
                        IncludeSubdirectories = true,
                        EnableRaisingEvents = true,
                    };

                    watcher.Changed += onChange;
                    watcher.Created += onChange;
                    watcher.Deleted += onChange;
                    watcher.Renamed += onRename;
                    watcher.Error += onError;

                    _watchers[i, j] = watcher;
                }
                watcher = new FileSystemWatcher
                {
                    Path = paths[i],
                    Filter = "*.*",
                    NotifyFilter = NotifyFilters.DirectoryName,
                    IncludeSubdirectories = true,
                    EnableRaisingEvents = true,
                };

                watcher.Changed += onChangeDir;
                watcher.Created += onChangeDir;
                watcher.Deleted += onChangeDir;
                watcher.Renamed += onRenameDir;
                watcher.Error += onErrorDir;

                _watchers[i, extensions.Length] = watcher;
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

        private void InitRegexs()
        {
            if(_extensions != null || _extensions.Length == 0)
            {
                string extensions = "";
                foreach(var item in _extensions)
                {
                    extensions += $"({item.Replace(".", "\\.").Replace("*", "(\\w)*")})+|";
                }
                extensions = extensions.Remove(extensions.Length - 1);
                _regexExtensions = new Regex(extensions);
            }
            if(_ignores != null || _ignores.Length == 0)
            {
                string ignores = "";
                foreach(var item in _ignores)
                {
                    ignores += $"({item.Replace("*", "\\.").Replace("*", "(\\w)*")})+|";
                }
                ignores = ignores.Remove(ignores.Length - 1);
                _regexIgnores = new Regex(ignores);
            }
        }

        private void DbListener() {
            bool isConnected = TryConnectSocket();
            bool isNeedSendEventsAgain = false;
            bool isNeedSendExceptionsAgain = false;

            while(true) {
                ewh.WaitOne();
                System.Threading.Thread.Sleep(1500);
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
        private void SetEventDirRecordTimer()
        {
            _timerDir.Elapsed += new ElapsedEventHandler(SendDirChangeToServer);
            _timerDir.Interval = 1000;
        }
        private void TimerReset() {
            _timer.Stop();
            _timer.Interval = _timerInterval;
            _isEventsRecord = false;
        }
        private void TimerDirReset()
        {
            _timerDir.Stop();
            _timerDir.Interval = _timerDirInterval;
            _isEventsDirRecord = false;
        }
        private void StartOrContinueRecord() {
            if(!_isEventsRecord) {
                _isEventsRecord = true;
                _timer.Start();
            }
        }
        private void StartOrContinueRecordDir()
        {
            if(!_isEventsDirRecord)
            {
                _isEventsDirRecord = true;
                _timerDir.Start();
            }
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

        private void OnErrorDir(object sender, ErrorEventArgs e)
        {
            _context.AddException(e.GetException());
        }
        private void OnRenamedDir(object sender, RenamedEventArgs e)
        {
            Console.WriteLine($"Was renamed dir {e.OldName} to {e.Name}");

            dirEventList.Enqueue(new TrackerEvent(e.Name, e.OldName, e.FullPath, e.OldFullPath, TrackerEvents.RenamedDir));

            StartOrContinueRecordDir();
        }
        private void OnChangedDir(object sender, FileSystemEventArgs e)
        {
            switch(e.ChangeType.ToString())
            {
                case nameof(TrackerEvents.Created):
                    Console.WriteLine($"Was create dir with name - {e.FullPath}");

                    dirEventList.Enqueue(new TrackerEvent(e.Name, e.FullPath, TrackerEvents.CreatedDir));

                    StartOrContinueRecordDir();
                    break;

                case nameof(TrackerEvents.Changed):
                    Console.WriteLine($"Was changed dir with name - {e.FullPath}");
                    break;

                case nameof(TrackerEvents.Deleted):
                    Console.WriteLine($"Was deleted dir with name - {e.FullPath}");

                    dirEventList.Enqueue(new TrackerEvent(e.Name, e.FullPath, TrackerEvents.DeletedDir));

                    StartOrContinueRecordDir();
                    break;

                default:
                    throw new Exception($"{e.ChangeType} not supported");
            }
        }

        private void AddAllFilesInDir(string path)
        {
            var list = new List<TrackerEvent>();

            foreach(var item in Directory.GetFiles(path))
                if(_regexExtensions.IsMatch(item) && !_regexIgnores.IsMatch(item))
                    list.Add(new TrackerEvent()
                    {
                        FullName = item,
                        DateTime = DateTime.UtcNow,
                        Name = item.Substring(item.LastIndexOf('\\') + 1),
                        EventName = TrackerEvents.Created
                    });

            _context.AddEvents(list);

            foreach(var item in Directory.GetDirectories(path))
                AddAllFilesInDir(item);
        }
        private void AnalizeDir()
        {
            var list = new List<TrackerEvent>();
            while(dirEventList.Count != 0)
            {
                dirEventList.TryDequeue(out TrackerEvent trackerEvent);
                list.Add(trackerEvent);
            }
            var result = analizer.AnalizeDir(list);
            if(result == null)
                return;

            foreach(var item in result)
                if(item.EventName == TrackerEvents.CreatedDir)
                    AddAllFilesInDir(item.FullName);
                else
                    _context.AddDirEvent(item);
        }

        private void SendDirChangeToServer(object sender, ElapsedEventArgs e)
        {
            TimerDirReset();

            while(eventList.Count != 0)
                return;

            AnalizeDir();

            //say can send to server
            ewh.Set();
        }
        private void SendChangeToServer(object sender, ElapsedEventArgs e) {
            TimerReset();

            _context.AddEvents(analizer.Analize());
            //say can send to server
            ewh.Set();
        }
    }
}
