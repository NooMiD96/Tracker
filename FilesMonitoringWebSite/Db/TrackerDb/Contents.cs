using System;
using System.Collections.Generic;

namespace FilesMonitoringWebSite.Db.TrackerDb
{
    public partial class Contents
    {
        public int ContentId { get; set; }
        public string FilePath { get; set; }
        public byte[] Payload { get; set; }

        public Changes Changes { get; set; }
    }
}
