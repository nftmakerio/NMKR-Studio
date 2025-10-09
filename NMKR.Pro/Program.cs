using System;
using System.IO;
using System.Threading;
using NMKR.Shared.Classes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace NMKR.Pro
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ThreadPool.SetMinThreads(256, 256);

            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "settings.yaml"))
            {
                GeneralConfigurationClass.CreateYamlFile(AppDomain.CurrentDomain.BaseDirectory + "settings.yaml");
            }

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, builder) =>
                {
                    string env = Environment.GetEnvironmentVariable("CARDANO_NETWORK");
                    builder.AddYamlFile(AppDomain.CurrentDomain.BaseDirectory + "settings.yaml", optional: false, reloadOnChange: true);
                    builder.AddYamlFile(AppDomain.CurrentDomain.BaseDirectory + $"settings.{env}.yaml", optional: true, reloadOnChange: true);
                    builder.AddEnvironmentVariables();
                })
                .ConfigureWebHost(webHostBuilder =>
                {
                    webHostBuilder.ConfigureKestrel(kestrelBuilder =>
                    {
                        kestrelBuilder.ListenAnyIP(5000);
                        kestrelBuilder.AddServerHeader = false;
                    });
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
