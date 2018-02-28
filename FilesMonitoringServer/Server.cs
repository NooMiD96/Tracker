using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using FilesMonitoring;
using System.Collections.Generic;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Xml;
using System.Threading;
using System.Linq;

namespace FilesMonitoringServer
{
    class Server
    {
        static private int _port;
        static private string _ip;

        static private int sizeLimit = 10000000;

        static private TrackerDb _context;

        public Server(string[] args)
        {
            ParseXml();

            _context = new TrackerDb();
            _context.Database.Migrate();

            Task.Factory.StartNew(DbCleaner);

            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var ipPoint = new IPEndPoint(IPAddress.Parse(_ip), _port);
            try
            {
                socket.Bind(ipPoint);
                socket.Listen(100);
                Console.WriteLine("Server is started");
                while(true)
                {
                    var newSocket = socket.Accept();

                    var tsc = new Task(ListenConnection, newSocket);
                    tsc.Start();
                }
            } catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            } 
        }

        private void ParseXml()
        {
            XmlDocument Config = new XmlDocument();
            Config.Load("Settings.xml");
            XmlElement xRoot = Config.DocumentElement;
            //db
            TrackerDb.ConnectingString = xRoot["ConnectionString"].InnerXml;
            //server
            var xServer = xRoot["Server"];
            _ip = xServer["ServerIP"].InnerText;
            _port = Convert.ToInt32(xServer["ServerPort"].InnerText);
        }

        private void DbCleaner()
        {
            while(true)
            {
                try
                {
                    var files = _context.GetFilesToDelete(DateTime.Now);

                    var contents = _context.GetContentsToDelete(files);

                    contents.ForEach(content =>
                    {
                        if(System.IO.File.Exists(Directory.GetCurrentDirectory() + content.FilePath))
                        {
                            System.IO.File.Delete(Directory.GetCurrentDirectory() + content.FilePath);
                        }
                    });

                    _context.RemoveFiles(files.AsEnumerable());
                } catch { }

                Thread.Sleep(86400000);
            }
        }

        public static void ListenConnection(Object obj)
        {
            if(obj is Socket socket)
            {
                var trackerId = Connected(socket);

                if(trackerId == -1)
                {
                    SendFileToWebClient(socket);
                    return;
                }

                Console.WriteLine($"Add new connection");
                byte[] buffer = new byte[sizeLimit];
                StringBuilder sb = new StringBuilder();
                TrackerEvent evnt;
                string path, str;
                ValueTuple<string, DateTime, string> clientException;
                try
                {
                    while(true)
                    {
                        path = null;

                        str = GetMessageFromClient(buffer, sb, socket);
                        if(str.Substring(str.Length - 5) == "ERROR")
                        {
                            str = str.Replace("ERROR", "");
                            clientException = JsonConvert.DeserializeObject<ValueTuple<string, DateTime, string>>(str);
                            _context.AddException(trackerId, clientException.Item1, clientException.Item2, clientException.Item3);
                            continue;
                        }
                        evnt = JsonConvert.DeserializeObject<TrackerEvent>(str);
                        sb.Clear();

                        switch(evnt.EventName)
                        {
                            case TrackerEvents.Moved:
                                _context.MovedEvent(evnt, trackerId);

                                Console.WriteLine($"Move {evnt.Name}\nfrom {evnt.OldFullName}\ninto {evnt.FullName}");
                                break;

                            case TrackerEvents.Renamed:
                                _context.RenamedEvent(evnt, trackerId);

                                Console.WriteLine($"Rename {evnt.OldName} into {evnt.Name}");
                                break;

                            case TrackerEvents.Created:
                                if(evnt.Content == null)
                                {
                                    Console.WriteLine($"Content null");
                                    path = GetContent(evnt, socket, trackerId);
                                }

                                _context.CreatedEvent(evnt, path, trackerId);

                                Console.WriteLine($"Create file {evnt.FullName}");
                                break;

                            case TrackerEvents.Deleted:
                                _context.DeletedEvent(evnt, trackerId);

                                Console.WriteLine($"Delete file {evnt.FullName}");
                                break;

                            case TrackerEvents.Changed:
                                if(evnt.Content == null)
                                {
                                    Console.WriteLine($"Content null");
                                    path = GetContent(evnt, socket, trackerId);
                                }

                                _context.ChangedEvent(evnt, path, trackerId);

                                Console.WriteLine($"Change file {evnt.FullName}");
                                break;

                            case TrackerEvents.DeletedDir:
                                Console.WriteLine($"Delete dir {evnt.FullName}");

                                _context.DeleteDir(evnt, trackerId);

                                break;

                            case TrackerEvents.CreatedDir:
                                Console.WriteLine($"Create dir {evnt.FullName}");

                                //_context.CreateDir(evnt, trackerId);

                                break;

                            case TrackerEvents.RenamedDir:
                                Console.WriteLine($"Rename dir {evnt.OldName} into {evnt.Name}");

                                _context.RenameDir(evnt, trackerId);

                                break;

                            case TrackerEvents.MovedDir:
                                Console.WriteLine($"Move dir {evnt.Name}\nfrom {evnt.OldFullName}\ninto {evnt.FullName}");

                                _context.MoveDir(evnt, trackerId);

                                break;

                            default:
                                break;
                        }
                    }
                } catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                } finally
                {
                    if(socket != null)
                    {
                        socket.Dispose();
                    }
                }
            }

        }

        private static int Connected(Socket socket)
        {
            SendMessage(socket, "GET_INFO");

            byte[] buffer = new byte[256];
            StringBuilder sb = new StringBuilder();
            string str;

            str = GetMessageFromClient(buffer, sb, socket);

            if(str.Equals("GET_FILE"))
            {
                return -1;
            }

            var trackerId = JsonConvert.DeserializeObject<int>(str);
            if(trackerId == -1)
            {
                trackerId = _context.AddTracker();
                SendMessage(socket, "trackerId=" + trackerId);
            } else
            {
                SendMessage(socket, "OK");
            }
            if(!_context.Contains(trackerId))
            {
                _context.AddTracker(trackerId);
            }
            return trackerId;
        }

        private static string GetContent(TrackerEvent evnt, Socket socket, int trackerId)
        {
            int size = 0;
            int bufSize = 16378 + 6;
            long pos = 0;
            byte[] buffer = new byte[bufSize];
            bool getting = true;
            string start, end;
            byte[] OK = Encoding.ASCII.GetBytes("OKEND");
            string path = null;
            using(MemoryStream ms = new MemoryStream())
            {
                //Checking to ready client send file
                pos = ms.Position;
                do
                {
                    ms.Position = pos;
                    do
                    {
                        size = socket.Receive(buffer);
                        ms.Write(buffer, 0, size);
                    } while(socket.Available > 0);
                    pos = ms.Position;
                    ms.Position = 0L;

                    size = ms.Read(buffer, 0, (int)ms.Length);

                    start = Encoding.ASCII.GetString(buffer, 0, size);
                } while(start != "SENDING");
                socket.Send(OK);
                //start getting data
                ms.Position = 0L;

                path = $"\\Files\\{trackerId}";
                if(!Directory.Exists(Directory.GetCurrentDirectory() + path))
                {
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + path);
                }
                path += $"\\{evnt.Name}_{Guid.NewGuid().ToString()}.zip";

                using(BinaryWriter writer = new BinaryWriter(System.IO.File.Open($"{Directory.GetCurrentDirectory() + path}", FileMode.Create)))
                {
                    while(getting)
                    {
                        do
                        {
                            size = socket.Receive(buffer);
                            ms.Write(buffer, 0, size);
                        } while(socket.Available > 0);

                        pos = ms.Position;
                        ms.Position = 0L;

                        size = ms.Read(buffer, 0, (int)ms.Length);

                        start = Encoding.ASCII.GetString(buffer, 0, 2);
                        end = Encoding.ASCII.GetString(buffer, size - 3, 3);

                        if(start == "OK" && end == "END")
                        {
                            socket.Send(OK);
                            writer.Write(buffer, 3, size - 6);

                            ms.Position = 0;
                        } else
                        {

                            start = Encoding.ASCII.GetString(buffer, 0, 3);

                            if(start == "END" && end == "END")
                            {
                                socket.Send(OK);
                                writer.Write(buffer, 3, size - 6);
                                getting = false;

                            } else
                            {
                                ms.Position = pos;
                            }
                        }
                    }
                } //!using writer
            } //!using ms

            return path;
        }
        private static string GetMessageFromClient(byte[] buffer, StringBuilder sb, Socket socket)
        {
            int size;
            string str;

            do
            {
                do
                {
                    size = socket.Receive(buffer);
                    sb.Append(Encoding.ASCII.GetString(buffer, 0, size));
                } while(socket.Available > 0);
                str = sb.ToString();

            } while(str.Substring(str.Length - 3) != "END");
            str = str.Substring(0, str.Length - 3);
            sb.Clear();

            SendMessage(socket, "OK");

            return str;
        }

        private static void SendMessage(Socket socket, string message) => socket.Send(Encoding.ASCII.GetBytes(message + "END"));

        private static void SendFileToWebClient(Socket socket)
        {
            NetworkStream stream = new NetworkStream(socket, true);
            int packSize = 25000000;
            byte[] buffer = new byte[packSize];
            stream.Read(buffer, 0, packSize);
            string path = Encoding.ASCII.GetString(buffer);
            path = Directory.GetCurrentDirectory() + path.Replace("\0", "");
            FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            int size;
            while(fileStream.Length > fileStream.Position)
            {
                size = fileStream.Read(buffer, 0, packSize);
                stream.Write(buffer, 0, size);
            }
            stream.Close();
        }
    }
}
