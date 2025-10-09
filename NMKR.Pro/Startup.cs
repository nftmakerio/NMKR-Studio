using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Blazor.Analytics;
using BlazorDownloadFile;
using Blazored.LocalStorage;
using NMKR.Pro.Classes;
using NMKR.Pro.LocalizationResources;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MudBlazor.Services;
using NMKR.RazorSharedClassLibrary.Helpers;
using StackExchange.Redis;
using XLocalizer;
using XLocalizer.Translate;
using XLocalizer.Translate.MyMemoryTranslate;
using XLocalizer.Xml;


namespace NMKR.Pro
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            GeneralConfigurationClass.ReadConfiguration(Configuration);

            if (!Directory.Exists(GeneralConfigurationClass.TempFilePath))
                Directory.CreateDirectory(GeneralConfigurationClass.TempFilePath);

            string connectionString = GeneralConfigurationClass.ConnectionString;
            string connectionStringDbSync = GeneralConfigurationClass.ConnectionStringDbSync;
            var serverVersion = new MySqlServerVersion(ServerVersion.AutoDetect(connectionString));

            GlobalFunctions.optionsBuilder.UseMySql(
                connectionString,
                serverVersion, options => options.EnableRetryOnFailure(
                    maxRetryCount: 10,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: new List<int> { 4060 }));

            GlobalFunctions.optionsBuilderDbSync.UseNpgsql(connectionStringDbSync);


          

            services.AddMvc(options => options.EnableEndpointRouting = false).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            services.AddHttpClient<ITranslator, MyMemoryTranslateService>();

            services.Configure<RequestLocalizationOptions>(ops =>
            {
                var cultures = new CultureInfo[] { new CultureInfo("en") }; //, new CultureInfo("de"), new CultureInfo("ja") };
                ops.SupportedCultures = cultures;
                ops.SupportedUICultures = cultures;
                ops.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("en");
            });
            services.AddSingleton<IXResourceProvider, XmlResourceProvider>();


            services.AddRazorPages()
                .AddXLocalizer<LocSource, MyMemoryTranslateService>(ops =>
                {
                    ops.ResourcesPath = "LocalizationResources";
                    ops.AutoAddKeys = true;
                    ops.AutoTranslate = true;
                });

            services.AddServerSideBlazor(options =>
            {
                options.DetailedErrors = true;
            });
            services.AddSingleton<TimerServices>();
            services.AddScoped<AppSettings>();
            services.AddControllers();



            // Redis
            var options = ConfigurationOptions.Parse(GeneralConfigurationClass.Redis.Server); // host1:port1, host2:port2, ...
            options.Password = GeneralConfigurationClass.Redis.Password;
            options.User = GeneralConfigurationClass.Redis.Username;
            options.Ssl = true;
            var redis = ConnectionMultiplexer.Connect(options);
            services.AddSingleton<IConnectionMultiplexer>(provider => redis);

            
       /*     services.AddDataProtection()
                .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys");
       */
            string redisconnectionstring =
                $"{GeneralConfigurationClass.Redis.Server},password={GeneralConfigurationClass.Redis.Password},abortConnect=false,ssl=true";
            services.AddSignalR(hubOptions =>
            {
                hubOptions.MaximumReceiveMessageSize = 2 * 32 * 1024;
                hubOptions.EnableDetailedErrors = true;
            
            });//.AddStackExchangeRedis(redisconnectionstring);

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            services.AddMudServices();
            services.AddBlazoredLocalStorage(config => config.JsonSerializerOptions.WriteIndented = true);
            services.AddScoped<ClipboardService>();
            services.AddBlazorDownloadFile(); //(ServiceLifetime lifetime = ServiceLifetime.Scoped);
            services.AddGoogleAnalytics("G-YLMNPZ466D");

            services.AddResponseCompression(options =>
            {
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
                options.EnableForHttps = true;
            });
            services.Configure<BrotliCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Fastest;
            });
            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Fastest;
            });
            // Install the new node
          //  GlobalFunctions.InstallNewCardanoCli();
            GlobalFunctions.CheckMainPassword();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseMvcWithDefaultRoute();

            // Localization
            app.UseRequestLocalization();

            // Delete older files in tmp directory
            Directory.GetFiles(GeneralConfigurationClass.TempFilePath)
                .Select(f => new FileInfo(f))
                .Where(f => f.LastWriteTime < DateTime.Now.AddDays(-1))
                .ToList()
                .ForEach(f => f.Delete());


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
