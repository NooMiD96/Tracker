using System;
using System.Collections.Generic;

namespace FilesMonitoringWebSite.Db.TrackerDb
{
    public partial class Files
    {
        public Files()
        {
            Changes = new HashSet<Changes>();
        }

        public int FileId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FullName { get; set; }
        public bool IsNeedDelete { get; set; }
        public bool IsWasDeletedChange { get; set; }
        public DateTime? RemoveFromDbTime { get; set; }
        public int TrackerId { get; set; }

        public Trackers Tracker { get; set; }
        public ICollection<Changes> Changes { get; set; }
    }
}
