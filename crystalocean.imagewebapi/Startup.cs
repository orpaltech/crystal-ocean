using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CrystalOcean.Data.Models;
using CrystalOcean.Data.Repository;
using CrystalOcean.ImageWebApi.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace CrystalOcean.ImageWebApi
{
    public class Startup
    {
        private CrossOriginSettings CrossOriginSettings { get; }

        private JwtSecuritySettings JwtSecuritySettings { get; }

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
            
            this.CrossOriginSettings = configuration.GetSection("CrossOriginSettings")
                    .Get<CrossOriginSettings>();

            this.JwtSecuritySettings = configuration.GetSection("JwtSecuritySettings")
                    .Get<JwtSecuritySettings>();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureRepositories(services);
            ConfigureAuthentication(services);

            services.Configure<JwtSecuritySettings>(
                this.Configuration.GetSection("JwtSecuritySettings"));

            JsonOutputFormatter jsonFormatter = new JsonOutputFormatter(
                new JsonSerializerSettings 
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }, 
                ArrayPool<Char>.Create()
            );

            ConfigureCrossOrigin(services);

            services.AddMvc(options => 
            {
                options.OutputFormatters.Clear();
                options.OutputFormatters.Insert(0, jsonFormatter);
            });

            ConfigureFilters(services);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment()) 
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();

            if (this.CrossOriginSettings.Enabled)
            {
                app.UseCors(this.CrossOriginSettings.PolicyName);
            }

            app.UseMvc();
        }

        private void ConfigureRepositories(IServiceCollection services)
        {
            var connectionString = this.Configuration["DbContextSettings:ConnectionString"];
            services.AddDbContext<UserRepository>(o => { o.UseNpgsql(connectionString);});
            services.AddDbContext<UserTokenRepository>(o => { o.UseNpgsql(connectionString); });
            services.AddDbContext<BinaryRepository>(o => { o.UseNpgsql(connectionString); });
            services.AddDbContext<UserIdentityRepository>(o => { o.UseNpgsql(connectionString); });

            services.AddIdentity<User, Role>()
                .AddEntityFrameworkStores<UserIdentityRepository>()
                .AddDefaultTokenProviders();
            
            /*services.ConfigureApplicationCookie(opts => 
                opts.Events = new CookieAuthenticationEvents 
                {
                    OnRedirectToLogin = ctx => 
                    {
                        if (ctx.Request.Path.StartsWithSegments("/api"))
                        {
                            ctx.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                        }
                        return Task.FromResult(0);
                    }
                });*/
        }

        private void ConfigureAuthentication(IServiceCollection services)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(o => 
                {
                    var signingKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(this.JwtSecuritySettings.Key));

                    o.TokenValidationParameters = new TokenValidationParameters 
                    {
                        ValidIssuer = this.JwtSecuritySettings.Issuer,
                        ValidAudience = this.JwtSecuritySettings.Audience,
                        IssuerSigningKey = signingKey,
                        ValidateLifetime = true,
                        // If you want to allow a certain amount of clock drift, set that here:
                        ClockSkew = TimeSpan.Zero
                    };
                    o.Authority = this.JwtSecuritySettings.Authority;
                    o.Audience = this.JwtSecuritySettings.Audience;
                    o.RequireHttpsMetadata = false;
                    o.SaveToken = true;
                });
        }

        private void ConfigureFilters(IServiceCollection services)
        {
            if (this.CrossOriginSettings.Enabled)
            {
                services.Configure<MvcOptions>(options =>
                {
                    options.Filters.Add(new CorsAuthorizationFilterFactory(
                        this.CrossOriginSettings.PolicyName));
                });
            }
        }

        private void ConfigureCrossOrigin(IServiceCollection services)
        {
            if (!this.CrossOriginSettings.Enabled)
            {
                return;
            }
            
            services.AddCors(options =>
            {
                options.AddPolicy(this.CrossOriginSettings.PolicyName,
                    builder => 
                    { 
                        if (this.CrossOriginSettings.AllowAnyOrigin) 
                        {
                            builder.AllowAnyOrigin();
                        }
                        else
                        {
                            builder.WithOrigins(this.CrossOriginSettings.AllowedOrigins.ToArray());
                        }

                        if (this.CrossOriginSettings.AllowAnyHeader)
                        {
                            builder.AllowAnyHeader();
                        }
                        else
                        {
                            builder.WithHeaders(this.CrossOriginSettings.AllowedHeaders.ToArray());
                        }

                        if (this.CrossOriginSettings.AllowAnyMethod)
                        {
                            builder.AllowAnyMethod();
                        }
                        else
                        {
                            builder.WithMethods(this.CrossOriginSettings.AllowedMethods.ToArray());
                        }
                    });
            });
        }

    }
}
