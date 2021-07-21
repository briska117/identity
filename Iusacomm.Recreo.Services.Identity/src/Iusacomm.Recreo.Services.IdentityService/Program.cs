using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iusacomm.Recreo.Services.IdentityService.Data;
using Iusacomm.Recreo.Services.IdentityService.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Iusacomm.Recreo.Services.IdentityService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                try
                {
                    ApplicationDbContextInitializer.Initialize(services);

                    var applicationDbContext = services.GetRequiredService<ApplicationDbContext>();
                    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                    if (!IdentityInitializationService.IsInitialized(applicationDbContext))
                    {
                        IdentityInitializationService.Initialize(userManager, roleManager)
                            .GetAwaiter()
                            .GetResult();
                    }
                }
                catch (Exception exception)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(exception, "Failed to seed database");
                }
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
