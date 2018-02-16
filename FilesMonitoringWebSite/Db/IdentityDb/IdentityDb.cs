using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FilesMonitoringWebSite.Db {
    public class IdentityDb :IdentityDbContext<IdentityUser> {
        public IdentityDb(DbContextOptions<IdentityDb> options)
            : base(options) {
        }
    }
}
