using System;
using System.IO;
using System.Text.Json.Serialization;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using space.linuxct.malninstall.Configuration.Core.Application.Contracts.Services.ControllerLogic;
using space.linuxct.malninstall.Configuration.Core.Application.Contracts.Services.PackageGenerationLogic;
using space.linuxct.malninstall.Configuration.Core.Application.Services.ControllerLogic;
using space.linuxct.malninstall.Configuration.Core.Application.Services.PackageGenerationLogic;

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

            services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));
            
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
                        Name = "GNU Affero General Public License v3.0",
                        Url = new Uri("https://github.com/linuxct/malninstall-configuration")
                    }
                });
            });

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
            {
                services.AddDistributedRedisCache(options =>
                {
                    options.Configuration = "redis-malninstall-backend:6379";
                    options.InstanceName = "";
                });
                services.AddSingleton<IIpPolicyStore, DistributedCacheIpPolicyStore>();
                services.AddSingleton<IRateLimitCounterStore,DistributedCacheRateLimitCounterStore>(); 
            }
            else
            {
                services.AddMemoryCache();
                services.AddDistributedMemoryCache();
                services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
                services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            }
            
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IPackageGenerationService, PackageGenerationService>();
            services.AddScoped<IConfigurationLogicService, ConfigurationLogicService>();
            services.AddScoped<IDownloadLogicService, DownloadLogicService>();
            services.AddScoped<IPackageCreatorLogicService, PackageCreatorLogicService>();
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

            app.UseIpRateLimiting();

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