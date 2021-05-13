using System;
using System.IO;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using space.linuxct.malninstall.Configuration.Core.Application.Contracts.Services;
using space.linuxct.malninstall.Configuration.Core.Application.Services;

namespace space.linuxct.malninstall.Configuration
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            HostEnvironment = environment;
            CreateILoggerConfiguration();
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment HostEnvironment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("FrontEndPolicy",
                    builder =>
                    {
                        builder.WithOrigins("https://*.linuxct.space", "http://*.linuxct.space")
                            .SetIsOriginAllowedToAllowWildcardSubdomains()
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
            });
            
            services.AddControllers()
                .AddJsonOptions(opts =>
                {
                    opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Malninstall Configuration Service",
                    Version = "v1",
                    Contact = new OpenApiContact
                    {
                        Name = "linuxct",
                        Email = "malninstall@linuxct.space",
                        Url = new Uri("https://malninstall.linuxct.space")
                    },
                    Description = "WebApi created in order to supply configuration to the Mobile and Web clients",
                    License = new OpenApiLicense
                    {
                        Name = "Open Source //ToDo",
                        Url = new Uri("https://github.com/linuxct/malninstall-configuration")
                    }
                });
            });

            services.AddDistributedRedisCache(options =>
            {
                options.Configuration = "redis-malninstall-backend:6379";
                options.InstanceName = "";
            });

            services.AddScoped<IPackageGenerationService, PackageGenerationService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();

            app.UseSwaggerUI(c =>
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "space.linuxct.malninstall.Configuration.Api v1"));

            app.UseHttpsRedirection();

            app.UseRouting();
            
            app.UseCors();

            app.UseAuthorization();

            app.UseSerilogRequestLogging();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }

        private void CreateILoggerConfiguration()
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.Logger(lc => lc.Filter
                    .ByIncludingOnly(e => e.Level == LogEventLevel.Information || e.Level == LogEventLevel.Debug)
                    .WriteTo.File(
                        new RenderedCompactJsonFormatter(),
                        Path.Combine(HostEnvironment.ContentRootPath, "logs/applog.ndjson"),
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 7,
                        shared: true))
                .WriteTo.Logger(lc => lc.Filter
                    .ByIncludingOnly(e => e.Level == LogEventLevel.Error)
                    .WriteTo.File(new RenderedCompactJsonFormatter(),
                        Path.Combine(HostEnvironment.ContentRootPath, "logs/errorlog.ndjson"),
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 7,
                        shared: true))
                .WriteTo.Seq("http://seq-malninstall-backend")
                .CreateLogger();
        }
    }
}