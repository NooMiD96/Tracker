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

namespace FilesMonitoringWebSite.Controllers
{
    [Route("api/[controller]")]
    public class TrackerController: Controller
    {
        private readonly TrackerDb _context;

        static private JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
        };

        public TrackerController([FromServices] TrackerDb context)
        {
            _context = context;
        }

        [HttpGet("[action]")]
        public string GetTrackerList(int? count, int? page)
        {
            if(!User.Identity.IsAuthenticated || !User.IsInRole("Admin"))
                return "null";

            int cnt, pg;
            if(!count.HasValue || !page.HasValue)
            {
                cnt = 10;
                pg = 1;
            } else
            {
                cnt = count.Value;
                pg = page.Value;
            }

            var result = _context.GetTrackerList(cnt, pg);

            if(result.Count == 0)
                return "null";
            else
                return JsonConvert.SerializeObject(result, JsonSettings);
            
        }
        [HttpGet("[action]")]
        public string GetFileList(int? trackerId, string userName, string filter, int? count, int? page)
        {
            if(!User.Identity.IsAuthenticated)
                return "null";

            int cnt, pg;
            if(!count.HasValue || !page.HasValue)
            {
                cnt = 10;
                pg = 1;
            } else
            {
                cnt = count.Value;
                pg = page.Value;
            }

            if(!String.IsNullOrEmpty(filter))
                return GetFilterFileList(filter, userName, cnt, pg);

            List<Files> result;

            if(User.IsInRole("Admin"))
            {
                if(!trackerId.HasValue)
                    return "";
                if(!String.IsNullOrEmpty(userName))
                    result = _context.GetAdminFileList(trackerId.Value, userName, cnt, pg);
                else
                    result = _context.GetAdminFileList(trackerId.Value, cnt, pg);
            } else
                result = _context.GetUserFileList(User.Identity.Name, cnt, pg);

            if(result.Count == 0)
                return "null";
            else
            {
                result.ForEach(res => res.Changes = null);
                result.ForEach(res => res.Tracker = null);

                return JsonConvert.SerializeObject(result, JsonSettings);
            }
        }
        private string GetFilterFileList(string filter, string userName, int count, int page)
        {
            List<Files> result;

            if(User.IsInRole("Admin"))
            {
                if(String.IsNullOrEmpty(userName))
                    result = _context.GetFilterUserFileList(filter, count, page);
                else
                    result = _context.GetFilterUserFileList(filter, userName, count, page);
            } else
                result = _context.GetFilterUserFileList(filter, User.Identity.Name, count, page);

            if(result.Count == 0)
                return "null";
            else
            {
                result.ForEach(res => res.Changes = null);
                result.ForEach(res => res.Tracker = null);

                return JsonConvert.SerializeObject(result, JsonSettings);
            }
        }
        [HttpGet("[action]")]
        public string GetExceptionList(int trackerId, string userName, int? count, int? page)
        {
            if(!User.Identity.IsAuthenticated && !User.IsInRole("Admin"))
                return "null";

            int cnt, pg;
            if(!count.HasValue || !page.HasValue)
            {
                cnt = 10;
                pg = 1;
            } else
            {
                cnt = count.Value;
                pg = page.Value;
            }

            List<ClientExceptions> result;

            if(!String.IsNullOrEmpty(userName))
                result = _context.GetAdminExceptionList(trackerId, userName, cnt, pg);
            else
                result = _context.GetExceptionList(trackerId, cnt, pg);

            if(result.Count == 0)
                return "null";
            else
            {
                result.ForEach(res => res.Tracker = null);

                return JsonConvert.SerializeObject(result, JsonSettings);
            }
        }
        [HttpGet("[action]")]
        public string GetChangeList(int fileId, int? count, int? page)
        {
            if(!User.Identity.IsAuthenticated)
                return "null";

            int cnt, pg;
            if(!count.HasValue || !page.HasValue)
            {
                cnt = 10;
                pg = 1;
            } else
            {
                cnt = count.Value;
                pg = page.Value;
            }

            List<Changes> result;

            if(User.IsInRole("Admin"))
                result = _context.GetAdminChangeList(fileId, cnt, pg);
            else
                result = _context.GetUserChangeList(fileId, User.Identity.Name, cnt, pg);

            if(result.Count == 0)
                return "null";
            else
            {
                result.ForEach(res => res.File = null);

                return JsonConvert.SerializeObject(result, JsonSettings);
            }
        }

        [HttpGet("[Action]")]
        public IActionResult GetFile(long changeId)
        {
            var toSend = (
                    from c in _context.Changes
                    join f in _context.Files on c.FileId equals f.FileId
                    join cntnt in _context.Contents on c.ContentId equals cntnt.ContentId
                    where c.Id.Equals(changeId)
                    select new
                    {
                        fileName = f.FileName,
                        content = cntnt,
                    }
                ).FirstOrDefault();

            if(toSend.content.Payload != null)
            {
                return File(toSend.content.Payload, "application/octet-stream", toSend.fileName);
            } else
            {
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
    }
}