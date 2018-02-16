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

namespace FilesMonitoring {
    public class Program {
        static void Main(string[] args) {
            new TrackerProgram().Start(args);
        }
    }
}
