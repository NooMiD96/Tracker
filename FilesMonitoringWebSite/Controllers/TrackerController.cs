using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FilesMonitoringWebSite.Db.TrackerDb;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Query;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using System.IO;

namespace FilesMonitoringWebSite.Controllers {
    [Route("api/[controller]")]
    public class TrackerController :Controller {
        private readonly TrackerDb _context;

        static private JsonSerializerSettings JsonSettings = new JsonSerializerSettings {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
        };

        public TrackerController([FromServices] TrackerDb context) {
            _context = context;
        }

        [HttpGet("[action]")]
        public string GetTrackerList(bool needUsersName, int? count, int? page) {
            if(!User.Identity.IsAuthenticated || !User.IsInRole("Admin")) {
                return "null";
            }
            int cnt, pg;
            if(!count.HasValue || !page.HasValue) {
                cnt = 10;
                pg = 1;
            } else {
                cnt = count.Value;
                pg = page.Value;
            }
            List<Users> result;

            result = _context
                .Users
                .OrderBy(us => us.TrackerId)
                .Skip(cnt * (pg - 1))
                .Take(cnt)
                .ToList();

            if(result.Count == 0) {
                return "null";
            } else {
                return JsonConvert.SerializeObject(result, JsonSettings);
            }
        }
        [HttpGet("[action]")]
        public string GetFileList(int? trackerId, string userName, string filter, int? count, int? page) {
            if(!User.Identity.IsAuthenticated) {
                return "null";
            }

            int cnt, pg;
            if(!count.HasValue || !page.HasValue) {
                cnt = 10;
                pg = 1;
            } else {
                cnt = count.Value;
                pg = page.Value;
            }

            if(!String.IsNullOrEmpty(filter)) {
                return GetSearchedFileList(filter, userName, cnt, pg);
            }

            List<Files> result;

            if(User.IsInRole("Admin")) {
                if(!trackerId.HasValue) {
                    return "null";
                }
                if(!String.IsNullOrEmpty(userName)) {
                    result = (
                            from c in _context.Changes
                            join u in _context.Users on c.UserId equals u.UserId
                            join f in _context.Files on c.FileId equals f.FileId
                            where u.TrackerId.Equals(trackerId.Value) && u.UserName.Equals(userName)
                            orderby f.FileName
                            select f
                        )?.Distinct()
                        .Skip(cnt * (pg - 1))
                        .Take(cnt)
                        .ToList();
                } else {
                    result = (
                            from f in _context.Files
                            where f.TrackerId.Equals(trackerId.Value)
                            orderby f.FileName
                            select f
                        ).Skip(cnt * (pg - 1))
                        .Take(cnt)
                        .ToList();
                }
            } else {
                result = (
                        from c in _context.Changes
                        join u in _context.Users on c.UserId equals u.UserId
                        join f in _context.Files on c.FileId equals f.FileId
                        where u.UserName.Equals(User.Identity.Name)
                        orderby f.FileName
                        select f
                    ).Distinct()
                    .Skip(cnt * (pg - 1))
                    .Take(cnt)
                    .ToList();
            }

            if(result.Count == 0) {
                return "null";
            } else {
                result.ForEach(res => res.Changes = null);
                result.ForEach(res => res.Tracker = null);

                return JsonConvert.SerializeObject(result, JsonSettings);
            }
        }
        [HttpGet("[action]")]
        public string GetExceptionList(int trackerId, string userName, int? count, int? page) {
            if(!User.Identity.IsAuthenticated && !User.IsInRole("Admin")) {
                return "null";
            }

            int cnt, pg;
            if(!count.HasValue || !page.HasValue) {
                cnt = 10;
                pg = 1;
            } else {
                cnt = count.Value;
                pg = page.Value;
            }

            List<ClientExceptions> result;

            if(!String.IsNullOrEmpty(userName)) {
                result = (
                        from t in _context.Trackers
                        join ex in _context.ClientExceptions on t.TrackerId equals ex.TrackerId
                        join u in _context.Users on ex.UserId equals u.UserId
                        where u.UserName.Equals(userName)
                        orderby ex.DateTime
                        select ex
                    )?.Skip(cnt * (pg - 1))
                    .Take(cnt)
                    .ToList();
            } else {
                result = (
                        from t in _context.Trackers
                        join ex in _context.ClientExceptions on t.TrackerId equals ex.TrackerId
                        orderby ex.DateTime
                        select ex
                    )?.Skip(cnt * (pg - 1))
                    .Take(cnt)
                    .ToList();
            }

            if(result.Count == 0) {
                return "null";
            } else {
                result.ForEach(res => res.Tracker = null);

                return JsonConvert.SerializeObject(result, JsonSettings);
            }
        }
        [HttpGet("[action]")]
        public string GetChangeList(int fileId, int? count, int? page) {
            if(!User.Identity.IsAuthenticated) {
                return "null";
            }

            List<Changes> result;

            int cnt, pg;
            if(!count.HasValue || !page.HasValue) {
                cnt = 10;
                pg = 1;
            } else {
                cnt = count.Value;
                pg = page.Value;
            }

            if(User.IsInRole("Admin")) {
                result = (
                        from c in _context.Changes
                        join u in _context.Users on c.UserId equals u.UserId
                        join f in _context.Files on c.FileId equals f.FileId
                        where f.FileId.Equals(fileId)
                        orderby c.DateTime
                        select c.AddUserName(u.UserName)
                    ).Skip(cnt * (pg - 1))
                    .Take(cnt)
                    .ToList();
            } else {
                result = (
                        from c in _context.Changes
                        join u in _context.Users on c.UserId equals u.UserId
                        join f in _context.Files on c.FileId equals f.FileId
                        where u.UserName.Equals(User.Identity.Name) && f.FileId.Equals(fileId)
                        orderby c.DateTime
                        select c.AddUserName(u.UserName)
                    ).Skip(cnt * (pg - 1))
                    .Take(cnt)
                    .ToList();
            }

            if(result.Count == 0) {
                return "null";
            } else {
                result.ForEach(res => res.File = null);

                return JsonConvert.SerializeObject(result, JsonSettings);
            }
        }
        [HttpGet("[action]")]
        public string GetSearchedFileList(string findString, string userName, int? count, int? page) {
            if(!User.Identity.IsAuthenticated) {
                return "null";
            }

            List<Files> result;

            int cnt, pg;
            if(!count.HasValue || !page.HasValue) {
                cnt = 10;
                pg = 1;
            } else {
                cnt = count.Value;
                pg = page.Value;
            }

            if(User.IsInRole("Admin")) {
                if(String.IsNullOrEmpty(userName)) {
                    if(findString.Contains('/')) {
                        result = _context
                            .Files
                            .Where(fl => fl.FullName.Contains(findString))
                            .OrderBy(fl => fl.FileName)
                            .Skip(cnt * (pg - 1))
                            .Take(cnt)
                            .ToList();
                    } else {
                        result = _context
                            .Files
                            .Where(fl => fl.FileName.Contains(findString))
                            .OrderBy(fl => fl.FileName)
                            .Skip(cnt * (pg - 1))
                            .Take(cnt)
                            .ToList();
                    }
                } else {
                    result = GetSearchedListFilesByNameAndFilter(findString, userName, cnt, pg);
                }
            } else {
                result = GetSearchedListFilesByNameAndFilter(findString, User.Identity.Name, cnt, pg);
            }

            if(result.Count == 0) {
                return "null";
            } else {
                result.ForEach(res => res.Changes = null);
                result.ForEach(res => res.Tracker = null);

                return JsonConvert.SerializeObject(result, JsonSettings);
            }
        }
        [HttpGet("[Action]")]
        public IActionResult GetFile(long changeId) {
            var toSend = (
                    from c in _context.Changes
                    join f in _context.Files on c.FileId equals f.FileId
                    join cntnt in _context.Contents on c.ContentId equals cntnt.ContentId
                    where c.Id.Equals(changeId)
                    select new {
                        fileName = f.FileName,
                        content = cntnt,
                    }
                ).FirstOrDefault();

            if(toSend.content.Payload != null) {
                return File(toSend.content.Payload, "application/octet-stream", toSend.fileName);
            } else {
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                var ipPoint = new IPEndPoint(IPAddress.Parse("10.10.0.167"), 8000);
                socket.Connect(ipPoint);
                NetworkStream stream = new NetworkStream(socket, true);

                byte[] buffer = new byte[50];
                stream.Read(buffer, 0, 50);

                stream.Write(Encoding.ASCII.GetBytes("GET_FILEEND"), 0, "GET_FILEEND".Length);
                stream.Read(buffer, 0, 50);

                stream.Write(Encoding.ASCII.GetBytes(toSend.content.FilePath), 0, toSend.content.FilePath.Length);

                return File(stream, "application/octet-stream", toSend.fileName + ".zip");
            }
        }
        private List<Files> GetSearchedListFilesByNameAndFilter(string findString, string userName, int cnt, int pg) {
            if(findString.Contains('/')) {
                return (
                        from c in _context.Changes
                        join u in _context.Users on c.UserId equals u.UserId
                        join f in _context.Files on c.FileId equals f.FileId
                        where u.UserName.Equals(userName) && f.FullName.Contains(findString)
                        orderby f.FileName
                        select f
                    ).Distinct()
                    .Skip(cnt * (pg - 1))
                    .Take(cnt)
                    .ToList();
            } else {
                return (
                        from c in _context.Changes
                        join u in _context.Users on c.UserId equals u.UserId
                        join f in _context.Files on c.FileId equals f.FileId
                        where u.UserName.Equals(userName) && f.FileName.Contains(findString)
                        orderby f.FileName
                        select f
                    ).Distinct()
                    .Skip(cnt * (pg - 1))
                    .Take(cnt)
                    .ToList();
            }
        }
    }
}