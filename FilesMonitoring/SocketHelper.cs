using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace FilesMonitoring {
    public class SocketHelper {
        private static object lockObj = new object();

        //private static string EndOfTransitision = "☻♥♦♣♦☺";
        public static string GetMessageFromServer(byte[] buffer, StringBuilder sb, Socket socket) {
            lock(lockObj) {
                int size;
                string str;

                do {
                    do {
                        size = socket.Receive(buffer);
                        sb.Append(Encoding.ASCII.GetString(buffer, 0, size));
                    } while(socket.Available > 0);
                    str = sb.ToString();

                } while(str.Substring(str.Length - 3) != "END");
                str = str.Substring(0, str.Length - 3);
                sb.Clear();

                return str;
            }
        }
        public static void SendItemToServer<T>(T item, Socket socket) {
            lock(lockObj) {
                byte[] sendBuffer = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(item) + "END");
                socket.Send(sendBuffer);

                GetMessageFromServer(sendBuffer, new StringBuilder(), socket);
            }
        }
        public static void SendBufferToServer(byte[] sendBuffer, Socket socket) {
            lock(lockObj) {
                socket.Send(sendBuffer);

                GetMessageFromServer(sendBuffer, new StringBuilder(), socket);
            }
        }
        public static void SendExaptionToServer(string userName, DateTime date, string ex, Socket socket) {
            lock(lockObj) {
                var item = (userName, date, ex);
                byte[] sendBuffer = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(item) + "ERROR" + "END");
                socket.Send(sendBuffer);

                GetMessageFromServer(sendBuffer, new StringBuilder(), socket);
            }
        }
        public static void SendZipArchive(Socket socket, string filePath) {
            lock(lockObj) {
                byte[] archive = new byte[1024];
                long length = 0;
                long size = 16378;
                byte[] tmpBuffer = new byte[size + 6];

                SendBufferToServer(Encoding.ASCII.GetBytes("SENDING"), socket);

                using(BinaryReader toWrite = new BinaryReader(File.Open(filePath, FileMode.Open))) {
                    length = toWrite.BaseStream.Length;
                    byte[] OK = Encoding.ASCII.GetBytes("OK");
                    byte[] END = Encoding.ASCII.GetBytes("END");
                    while(length > size) {
                        archive = toWrite.ReadBytes((int)size);

                        tmpBuffer[0] = OK[0];
                        tmpBuffer[1] = OK[1];
                        tmpBuffer[2] = 0;

                        Array.Copy(archive, 0, tmpBuffer, 3, archive.Length);

                        tmpBuffer[size + 3] = END[0];
                        tmpBuffer[size + 4] = END[1];
                        tmpBuffer[size + 5] = END[2];

                        SendBufferToServer(tmpBuffer, socket);

                        length -= size;
                    }
                    archive = toWrite.ReadBytes((int)length);

                    tmpBuffer = new byte[length + 6];

                    tmpBuffer[0] = END[0];
                    tmpBuffer[1] = END[1];
                    tmpBuffer[2] = END[2];

                    Array.Copy(archive, 0, tmpBuffer, 3, archive.Length);

                    tmpBuffer[length + 3] = END[0];
                    tmpBuffer[length + 4] = END[1];
                    tmpBuffer[length + 5] = END[2];

                    SendBufferToServer(tmpBuffer, socket);
                }
            }
        }
    }
}
