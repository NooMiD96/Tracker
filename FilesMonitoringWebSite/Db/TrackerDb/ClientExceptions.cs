using System;
using System.Collections.Generic;

namespace FilesMonitoringWebSite.Db.TrackerDb
{
    public partial class ClientExceptions
    {
        public int ExceptionId { get; set; }
        public DateTime DateTime { get; set; }
        public string ExceptionInner { get; set; }
        public int TrackerId { get; set; }
        public int UserId { get; set; }

        public Trackers Tracker { get; set; }
    }
}
