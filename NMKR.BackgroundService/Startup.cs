using System;
using System.Collections.Generic;
using System.Reflection;
using NMKR.BackgroundService.HostedServices;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace NMKR.BackgroundService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            GeneralConfigurationClass.ReadConfiguration(Configuration);
            
            string connectionString = GeneralConfigurationClass.ConnectionString;

            var serverVersion = new MySqlServerVersion(ServerVersion.AutoDetect(connectionString));

            GlobalFunctions.optionsBuilder.UseMySql(
                connectionString,
                serverVersion, options => options.EnableRetryOnFailure(
                    maxRetryCount: 10,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: new List<int> { 4060 }));

            var options = ConfigurationOptions.Parse(GeneralConfigurationClass.Redis.Server); // host1:port1, host2:port2, ...
            options.Password = GeneralConfigurationClass.Redis.Password;
            options.User = GeneralConfigurationClass.Redis.Username;
            options.Ssl = true;
            services.AddSingleton<IConnectionMultiplexer>(provider => ConnectionMultiplexer.Connect(options));
            
            services.AddRazorPages();
            services.AddHostedService<CheckPaymentAddressesService>();
            services.AddHostedService<LifesignLogger>();
            services.AddControllers();

            // RabbitMQ
            services.AddMassTransit(x =>
                {
                    x.SetKebabCaseEndpointNameFormatter();

                    // By default, sagas are in-memory, but should be changed to a durable
                    // saga repository.
                    x.SetInMemorySagaRepositoryProvider();

                    var entryAssembly = Assembly.GetEntryAssembly();

                    x.AddConsumers(entryAssembly);
                    x.AddSagaStateMachines(entryAssembly);
                    x.AddSagas(entryAssembly);
                    x.AddActivities(entryAssembly);

                    x.UsingRabbitMq((context, cfg) =>
                    {
                        cfg.Host(
                            GeneralConfigurationClass.RabbitMqConnectionString,
                            h =>
                            {
                                h.Username(GeneralConfigurationClass.RabbitMqUsername);
                                h.Password(GeneralConfigurationClass.RabbitMqPassword);

                            });
                        cfg.UseMessageRetry(r => r.Interval(5, new TimeSpan(0, 0, 0, 10)));
                        cfg.ConfigureEndpoints(context);
                    });


                });
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
             //   app.UseHsts();
            }

           // app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapGet("/api/check", () => "OK");
                endpoints.MapControllers();

            });
        }
    }
}
