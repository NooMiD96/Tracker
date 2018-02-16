using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FilesMonitoringWebSite.Db.TrackerDb;
using FilesMonitoringWebSite.Db;

namespace FilesMonitoringWebSite
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        //add in identity database admins info
        public async void CreateAdminAndRole(IServiceProvider serviceProvider) {
            var RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var UserManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            try {
                string roleName = "Admin";
                IdentityResult roleResult;

                var roleExist = await RoleManager.RoleExistsAsync(roleName);
                // ensure that the role does not exist
                if(!roleExist) {
                    //create the roles and seed them to the database: 
                    roleResult = await RoleManager.CreateAsync(new IdentityRole(roleName));
                }

                // find the user with the admin email 
                var admins = Configuration.GetSection("Admins").GetChildren();
                foreach(var admin in admins) {
                    var userName = admin["UserName"];
                    var password = admin["Password"];

                    var _user = await UserManager.FindByNameAsync(userName);
                    // check if the user exists
                    if(_user == null) {
                        //Here you could create the super admin who will maintain the web app
                        var poweruser = new IdentityUser {
                            UserName = userName,
                        };

                        var createPowerUser = await UserManager.CreateAsync(poweruser, password);
                        if(createPowerUser.Succeeded) {
                            //here we tie the new user to the role
                            await UserManager.AddToRoleAsync(poweruser, "Admin");
                        }
                        //UserManager.AddToRoleAsync(poweruser, "Admin")
                    }
                }
            } catch(Exception ex) {
                Console.WriteLine($"info: Trouble with first connection to identity database:\n{ex.Message}");
            } finally {
                if(RoleManager != null) {
                    RoleManager.Dispose();
                }
                if(UserManager != null) {
                    UserManager.Dispose();
                }
            }
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddSi
            services.AddDbContext<TrackerDb>(options => {
                options.UseSqlServer(Configuration.GetConnectionString("TrackerDb"));
            });
            services.AddDbContext<IdentityDb>(options => {
                options.UseSqlServer(Configuration.GetConnectionString("IdentityDb"));
            });

            services.AddIdentity<IdentityUser, IdentityRole>(options => {
                options.Password.RequiredLength = 1;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                })
                .AddEntityFrameworkStores<IdentityDb>();

            var serviceProvider = services.BuildServiceProvider();
            CreateAdminAndRole(serviceProvider);

            services.AddMvc();
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true,
                    ReactHotModuleReplacement = true
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }
    }
}
