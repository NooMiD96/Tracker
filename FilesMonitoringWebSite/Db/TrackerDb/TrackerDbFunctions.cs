using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FilesMonitoringWebSite.Db.TrackerDb
{
    public partial class TrackerDb: DbContext
    {
        public TrackerDb(DbContextOptions<TrackerDb> options) : base(options) { }

        static private object lockObj = new object();

        public void EditDeleteTime(int fileId, DateTime date, bool isNeedDelete)
        {
            var file = Files
                .Where(fl => fl.FileId.Equals(fileId))
                .FirstOrDefault();

            if(file == null)
            {
                return;
            }

            lock(lockObj)
            {
                if(!isNeedDelete)
                {
                    file.IsNeedDelete = false;
                } else
                {
                    file.RemoveFromDbTime = date;
                }

                SaveChanges();
            }
        }

        public List<Users> GetTrackerList(int count, int page) => Users
                .OrderBy(us => us.TrackerId)
                .Skip(count * (page - 1))
                .Take(count)
                .ToList();

        public List<Files> GetAdminFileList(int trackerId, string userName, int count, int page) => (
                from c in Changes
                join u in Users on c.UserId equals u.UserId
                join f in Files on c.FileId equals f.FileId
                where u.TrackerId.Equals(trackerId) && u.UserName.Equals(userName)
                orderby f.FileName
                select f
            )?.Distinct()
            .Skip(count * (page - 1))
            .Take(count)
            .ToList();
        public List<Files> GetAdminFileList(int trackerId, int count, int page) => (
                from f in Files
                where f.TrackerId.Equals(trackerId)
                orderby f.FileName
                select f
            ).Skip(count * (page - 1))
            .Take(count)
            .ToList();

        public List<Files> GetUserFileList(string userName, int count, int page) => (
                from c in Changes
                join u in Users on c.UserId equals u.UserId
                join f in Files on c.FileId equals f.FileId
                where u.UserName.Equals(userName)
                orderby f.FileName
                select f
            ).Distinct()
            .Skip(count * (page - 1))
            .Take(count)
            .ToList();

        public List<Files> GetFilterUserFileList(string filter, int count, int page)
        {
            if(filter.Contains('/'))
            {
                return Files
                    .Where(fl => fl.FullName.Contains(filter))
                    .OrderBy(fl => fl.FileName)
                    .Skip(count * (page - 1))
                    .Take(count)
                    .ToList();
            } else
            {
                return Files
                    .Where(fl => fl.FileName.Contains(filter))
                    .OrderBy(fl => fl.FileName)
                    .Skip(count * (page - 1))
                    .Take(count)
                    .ToList();
            }

        }
        public List<Files> GetFilterUserFileList(string filter, string userName, int count, int page)
        {
            if(filter.Contains('/'))
            {
                return (
                        from c in Changes
                        join u in Users on c.UserId equals u.UserId
                        join f in Files on c.FileId equals f.FileId
                        where u.UserName.Equals(userName) && f.FullName.Contains(filter)
                        orderby f.FileName
                        select f
                    ).Distinct()
                    .Skip(count * (page - 1))
                    .Take(count)
                    .ToList();
            } else
            {
                return (
                        from c in Changes
                        join u in Users on c.UserId equals u.UserId
                        join f in Files on c.FileId equals f.FileId
                        where u.UserName.Equals(userName) && f.FileName.Contains(filter)
                        orderby f.FileName
                        select f
                    ).Distinct()
                    .Skip(count * (page - 1))
                    .Take(count)
                    .ToList();
            }
        }

        public List<ClientExceptions> GetAdminExceptionList(int trackerId, string userName, int count, int page) => (
            from t in Trackers
            join ex in ClientExceptions on t.TrackerId equals ex.TrackerId
            join u in Users on ex.UserId equals u.UserId
            where u.UserName.Equals(userName)
            orderby ex.DateTime
            select ex
        )?.Skip(count * (page - 1))
        .Take(count)
        .ToList();

        public List<ClientExceptions> GetExceptionList(int trackerId, int count, int page) => (
            from t in Trackers
            join ex in ClientExceptions on t.TrackerId equals ex.TrackerId
            orderby ex.DateTime
            select ex
        )?.Skip(count * (page - 1))
        .Take(count)
        .ToList();

        public List<Changes> GetAdminChangeList(int fileId, int count, int page) => (
            from c in Changes
            join u in Users on c.UserId equals u.UserId
            join f in Files on c.FileId equals f.FileId
            where f.FileId.Equals(fileId)
            orderby c.DateTime
            select c.AddUserName(u.UserName)
        ).Skip(count * (page - 1))
        .Take(count)
        .ToList();

        public List<Changes> GetUserChangeList(int fileId, string userName, int count, int page) => (
            from c in Changes
            join u in Users on c.UserId equals u.UserId
            join f in Files on c.FileId equals f.FileId
            where u.UserName.Equals(userName) && f.FileId.Equals(fileId)
            orderby c.DateTime
            select c.AddUserName(u.UserName)
        ).Skip(count * (page - 1))
        .Take(count)
        .ToList();

    }
}
