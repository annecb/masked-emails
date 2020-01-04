﻿using Data;
using Data.Interop;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Utils;
using Utils.Interop;
using WebApi.Configuration;
using WebApi.Owin;
using WebApi.Services;
using WebApi.Services.Interop;

namespace WebApi
{
    public class Startup
    {
        IWebHostEnvironment Environment { get; }
        IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddMvcCore(options => { options.EnableEndpointRouting = false; })
                .AddAuthorization()
                ;

            ConfigureApplication(services);
            ConfigureDependencies(services);
        }

        public void Configure(IApplicationBuilder app)
        {
            if (!Environment.IsDevelopment())
            {
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            app.HandleExceptions();
            app.UseAuthentication();
            app.UseStaticFiles();

            app.UseMvc();
            app.UseSpaStaticFiles();
            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";
                if (Environment.IsDevelopment())
                    spa.UseAngularCliServer(npmScript: "start");
            });
        }

        private void ConfigureApplication(IServiceCollection services)
        {
            var appSettings = new AppSettings();
            var appSettingsSection = Configuration.GetSection("AppSettings");
            appSettingsSection.Bind(appSettings);

            services.AddOptions();
            services.Configure<AppSettings>(appSettingsSection);

            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.Authority = appSettings.Authority;
                    options.RequireHttpsMetadata = appSettings.RequireHttpsMetadata;
                    options.Audience = appSettings.Audience;
                });

            services.AddDbContext<MaskedEmailsDbContext>(
                options =>
                    options.UseSqlite(Configuration.GetConnectionString("DefaultConnection"))
            );

            if (!Environment.IsDevelopment())
            {
                services.AddHttpsRedirection(options =>
                {
                    options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
                    options.HttpsPort = appSettings.HttpsPort;
                });
            }

            services.AddSpaStaticFiles(options =>
            {
                options.RootPath = "ClientApp/dist";
            });
        }

        private void ConfigureDependencies(IServiceCollection services)
        {
            services.AddTransient<IMaskedEmailsDbContext, MaskedEmailsDbContext>();
            services.AddTransient<IMaskedEmailService, MaskedEmailService>();
            services.AddTransient<IProfilesService, ProfilesService>();
            services.AddTransient<IUniqueIdGenerator, UniqueIdGenerator>();

            services.AddTransient<ISequence, StorageTableSequence>(
                provider => new StorageTableSequence(Configuration));

            services.AddSingleton<IMaskedEmailCommandService, MaskedEmailCommandQueueService>(
                provider => new MaskedEmailCommandQueueService(Configuration));
        }
    }
}
