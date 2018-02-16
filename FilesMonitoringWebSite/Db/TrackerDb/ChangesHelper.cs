using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace FilesMonitoringWebSite.Db.TrackerDb {
    public partial class Changes {
        [NotMapped]
        public string UserName { get; set; }
    }
    public static class ChangesHelper {
        static public Changes AddUserName(this Changes changes, string userName) {
            changes.UserName = userName;
            return changes;
        }
    }
}
