using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrystalOcean.Data.Models;
using CrystalOcean.Data.Models.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace CrystalOcean.ImageWebApi
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
            ConfigureRepositories(services);

            JsonOutputFormatter jsonFormatter = new JsonOutputFormatter(
                new JsonSerializerSettings {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }, 
                ArrayPool<Char>.Create()
            );
            services.AddMvc(
                opts => {
                    opts.OutputFormatters.Clear();
                    opts.OutputFormatters.Insert(0, jsonFormatter);
                }
            );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            
            if (env.IsDevelopment()) 
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }

        private void ConfigureRepositories(IServiceCollection services)
        {
            var connectionString = this.Configuration["DbContextSettings:ConnectionString"];
            services.AddDbContext<UserRepository>(
                opts => opts.UseNpgsql(connectionString));
            services.AddDbContext<BinaryRepository>(
                opts => opts.UseNpgsql(connectionString));
        }
    }
}
