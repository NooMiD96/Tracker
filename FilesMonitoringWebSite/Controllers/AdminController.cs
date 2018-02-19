using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FilesMonitoringWebSite.Db.TrackerDb;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FilesMonitoringWebSite.Controllers {
    [Route("api/[controller]")]
    public class AdminController :Controller {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly TrackerDb _context;

        static private JsonSerializerSettings JsonSettings = new JsonSerializerSettings {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
        };

        public AdminController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, [FromServices] TrackerDb context) {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="trackerId"></param>
        /// <param name="un"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [HttpPut("[action]")]
        public async Task<bool> EditUser() {
            JObject result = await GetJsonFromBodyRequest();

            string un = result["userName"]?.Value<string>();
            if(String.IsNullOrEmpty(un)) {
                return false;
            }
            string password = result["password"]?.Value<string>();

            var userIdentity = await _userManager.FindByNameAsync(un);
            var user = _context.GetUserOrNull(un);
                

            if(user == null) {
                return false;
            }
            if(userIdentity != null) {
                await _userManager.DeleteAsync(userIdentity);
            }

            if(String.IsNullOrEmpty(password)) {
                user.IsCanAuthohorization = false;
            } else {
                user.IsCanAuthohorization = true;
                userIdentity = new IdentityUser(un);
                await _userManager.CreateAsync(userIdentity, password);
            }
            _context.SaveChanges();

            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="date"></param>
        /// <param name="isNeedDelete"></param>
        /// <returns></returns>
        [HttpPut("[Action]")]
        public async Task<bool> EditDeleteTime() {
            JObject result = await GetJsonFromBodyRequest();

            int fileId;
            bool isNeedDelete;
            DateTime date;

            try {
                fileId = result["fileId"].Value<int>();
                date = result["dateTime"].Value<DateTime>();
                isNeedDelete = result["isNeedDelete"].Value<bool>();
            } catch {
                return false;
            }

            _context.EditDeleteTime(fileId, date, isNeedDelete);

            return true;
        }

        private async Task<JObject> GetJsonFromBodyRequest() {
            var bodyStream = Request.Body;
            string content;

            using(var reader = new StreamReader(bodyStream)) {
                content = await reader.ReadToEndAsync();
            }

            return JObject.Parse(content);
        }
    }
}