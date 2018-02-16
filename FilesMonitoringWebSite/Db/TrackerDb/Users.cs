using System;
using System.Collections.Generic;

namespace FilesMonitoringWebSite.Db.TrackerDb
{
    public partial class Users
    {
        public int UserId { get; set; }
        public int TrackerId { get; set; }
        public string UserName { get; set; }
        public bool IsCanAuthohorization { get; set; }

        public Trackers Tracker { get; set; }
    }
}
