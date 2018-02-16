using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FilesMonitoringWebSite.Db.TrackerDb {
    public partial class TrackerDb :DbContext {
        public TrackerDb(DbContextOptions<TrackerDb> options) : base(options) { }

        static private object lockObj = new object();

        public void EditDeleteTime(int fileId, DateTime date, bool isNeedDelete) {
            var file = Files
                .Where(fl => fl.FileId.Equals(fileId))
                .FirstOrDefault();

            if(file == null) {
                return;
            }

            lock(lockObj) {
                if(!isNeedDelete) {
                    file.IsNeedDelete = false;
                } else {
                    file.RemoveFromDbTime = date;
                }

                SaveChanges();
            }
        }
    }
}
