using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FilesMonitoringWebSite.Controllers {
    [Route("api/[controller]")]
    public class AuthorizationController :Controller {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthorizationController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, RoleManager<IdentityRole> roleManager) {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        [HttpPost("[action]")]
        public async Task<string> SignIn(string un, string pw) {
            var result = await _signInManager.PasswordSignInAsync(un, pw, false, false);

            return result.Succeeded ? "true" : "false";
        }
        [HttpGet("[action]")]
        public string GetUserInfo() {
            if(!User.Identity.IsAuthenticated) {
                return "";
            }
            var userType = User.IsInRole("Admin") ? "Admin" : "User";
            var item = new {  userType, userName = User.Identity.Name, trackerId = 1 };
            var result = JsonConvert.SerializeObject(item);
            return result;
        }
        [HttpPut("[action]")]
        public async Task SignOut() {
            await _signInManager.SignOutAsync();
        }

    }
}