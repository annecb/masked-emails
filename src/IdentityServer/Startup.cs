using System;
using System.Data.Common;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IdentityServer
{
    public class Startup
    {
        private IConfiguration Configuration { get; }

        private ILogger<Startup> Logger { get; }
        private IWebHostEnvironment Environment { get; }

        public Startup(ILoggerFactory loggerFactory, IWebHostEnvironment environment, IConfiguration configuration)
        {
            Logger = loggerFactory.CreateLogger<Startup>();
            Environment = environment;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration["TableStorage:IdentityServerStore"];

            services
                .AddMvc(options => { options.EnableEndpointRouting = false; })
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                ;

            services.AddAspNetIdentity(connectionString, Configuration);

            var cb = new DbConnectionStringBuilder { ConnectionString = connectionString, ["AccountKey"] = "***REDACTED***" };
            Logger.LogDebug($"Configuring persistent storage for clients, resources, tokens and consents.\nConnectionString: \"{cb.ConnectionString}\".");

            var builder = services.AddIdentityServer(connectionString);

            if (Environment.IsDevelopment())
            {
                builder.AddDeveloperSigningCredential();
            }
            else
            {
                Logger.LogDebug("Loading signing certificate...");

                try
                {

                    var password = Configuration["SigningCertificate:Password"];
                    var certificatePath = Path.Combine(Environment.ContentRootPath, @"App_Data\signing.pfx");
                    if (File.Exists(certificatePath))
                    {
                        const X509KeyStorageFlags storageFlags = X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.EphemeralKeySet;
                        var rawBytes = File.ReadAllBytes(certificatePath);
                        var certificate = new X509Certificate2(rawBytes, password, storageFlags);
                        builder.AddSigningCredential(certificate);

                        Logger.LogDebug($"File '{certificatePath}' successfully loaded.");
                    }
                    else
                    {
                        Logger.LogCritical($"Unable to load the required signing certificate. FileNotFound.");
                        Logger.LogError($"File '{certificatePath}' not found.");
                    }
                }
                catch (Exception e)
                {
                    Logger.LogCritical($"Unable to load the required signing certificate. FileNotFound.");
                    Logger.LogError(e.Message);
                }
            }
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseIdentityServer();
            app.UseMvcWithDefaultRoute();
        }
    }
}
