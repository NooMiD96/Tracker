using System;
using System.Collections.Generic;

namespace FilesMonitoringWebSite.Db.TrackerDb
{
    public partial class Trackers
    {
        public Trackers()
        {
            ClientExceptions = new HashSet<ClientExceptions>();
            Files = new HashSet<Files>();
            Users = new HashSet<Users>();
        }

        public int TrackerId { get; set; }

        public ICollection<ClientExceptions> ClientExceptions { get; set; }
        public ICollection<Files> Files { get; set; }
        public ICollection<Users> Users { get; set; }
    }
}
