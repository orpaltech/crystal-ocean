using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrystalOcean.Data.Models;
using CrystalOcean.Data.Repository;
using CrystalOcean.Web.Configuration;
using CrystalOcean.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CrystalOcean.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {            
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureRepositores(services);
            ConfigureAuthentication(services);

            services.AddTransient<IEmailSender, AuthMessageSender>();

            services.Configure<AuthMessageSenderSettings>(
                this.Configuration.GetSection("AuthMessageSenderSettings"));
            
            services.Configure<ImageWebApiSettings>(
                this.Configuration.GetSection("ImageWebApiSettings"));
        
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
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
            });
        }

        private void ConfigureRepositores(IServiceCollection services)
        {
            var connectionString = this.Configuration["DbContextSettings:ConnectionString"];
            services.AddDbContext<UserRepository>(o => { o.UseNpgsql(connectionString); });
            services.AddDbContext<UserIdentityRepository>(o => { o.UseNpgsql(connectionString); });
        }

        private void ConfigureAuthentication(IServiceCollection services)
        {
            services.AddIdentity<User, Role>(o =>
                {
                    o.SignIn.RequireConfirmedEmail = false;
                    o.SignIn.RequireConfirmedPhoneNumber = false;
                })
                .AddEntityFrameworkStores<UserIdentityRepository>()
                .AddDefaultTokenProviders();

            services.AddAuthentication()
                .AddGoogle(o =>
                {
                    // TODO: dotnet user-secrets set Name Value
                    o.ClientId = this.Configuration["AuthSettings:Google:ClientId"];
                    o.ClientSecret = this.Configuration["AuthSettings:Google:ClientSecret"];
                })
                .AddFacebook(o => 
                {
                    o.AppId = this.Configuration["AuthSettings:Facebook:AppId"];
                    o.AppSecret = this.Configuration["AuthSettings:Facebook:AppSecret"];
                });
            
            services.ConfigureApplicationCookie(o =>
            {
                // Cookie settings
                o.Cookie.HttpOnly = true;
                o.Cookie.Expiration = TimeSpan.FromDays(7);
            });

            services.Configure<IdentityOptions>(o =>
            {
                // Password settings
                o.Password.RequiredLength = 8;
                o.Password.RequiredUniqueChars = 5;

                // Lockout settings
                o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                o.Lockout.MaxFailedAccessAttempts = 10;
                o.Lockout.AllowedForNewUsers = true;

                // User settings
                o.User.RequireUniqueEmail = true;

            });
        }

    }
}
