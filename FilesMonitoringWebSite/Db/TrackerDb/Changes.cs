using System;
using System.Collections.Generic;

namespace FilesMonitoringWebSite.Db.TrackerDb
{
    public partial class Changes
    {
        public long Id { get; set; }
        public DateTime DateTime { get; set; }
        public int EventName { get; set; }
        public int FileId { get; set; }
        public string OldFullName { get; set; }
        public string OldName { get; set; }
        public int UserId { get; set; }
        public int? ContentId { get; set; }

        public Contents Content { get; set; }
        public Files File { get; set; }
    }
}
