using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace IS4
{
    public class Program
    {
        public static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                        .MinimumLevel.Debug()
                        .Enrich.FromLogContext()
                        .CreateLogger();
            var seed = args.Contains("/seed");
            var host = CreateHostBuilder(args).Build();

            if(seed){
                Log.Information("seed comming...");
                var config = host.Services.GetRequiredService<IConfiguration>();
                var cn = config.GetConnectionString("DefaultConnection");

                SeedData.EnsureSeedData(cn);
                Log.Information("Seed completed");

                return 0;
            }

            Log.Information("starting hosts..");
            host.Run();
            return 0;

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
