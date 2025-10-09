using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AspNetCoreRateLimit;
using AspNetCoreRateLimit.Redis;
using NMKR.Shared.Classes;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NMKR.Shared.Functions;
using Microsoft.AspNetCore.HttpOverrides;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json.Serialization;
using Asp.Versioning;
using MassTransit;
using NMKR.Api.Apikey;


namespace NMKR.Api
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


            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            // needed to load configuration from appsettings.json
            services.AddOptions();

            // needed to store rate limit counters and ip rules
            services.AddMemoryCache();

            //load general configuration from appsettings.json
            services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));

            //load ip rules from appsettings.json
            services.Configure<IpRateLimitPolicies>(Configuration.GetSection("IpRateLimitPolicies"));


            var options = ConfigurationOptions.Parse(GeneralConfigurationClass.Redis.Server); // host1:port1, host2:port2, ...
            options.Password = GeneralConfigurationClass.Redis.Password;
            options.User= GeneralConfigurationClass.Redis.Username;
            options.Ssl = true;
            services.AddSingleton<IConnectionMultiplexer>(provider => ConnectionMultiplexer.Connect(options));

            services.AddRedisRateLimiting();


            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            // Check the Apikey
            services.AddSingleton<ApiKeyAuthorizationFilter>();
            services.AddSingleton<IApiKeyValidator, ApiKeyValidator>();


            // Add framework services.
            services.AddMvc();
            services.AddControllers()
                .AddJsonOptions(options =>
               options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));


            // OpenTelemetry
            // Telemetry
         /*   var openTelemetryConfiguration = new OpenTelemetryConfiguration();
            Configuration.GetSection("OpenTelemetry").Bind(openTelemetryConfiguration);

            if (openTelemetryConfiguration.Enabled)
            {
                services.AddOpenTelemetryTracing((builder) => builder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(openTelemetryConfiguration.ServiceName))
                    .AddAspNetCoreInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddMassTransitInstrumentation()
                    .AddRedisInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddJaegerExporter(options =>
                    {
                        options.Protocol = OpenTelemetry.Exporter.JaegerExportProtocol.HttpBinaryThrift;
                        options.Endpoint = new Uri(openTelemetryConfiguration.Endpoint);
                    })
                );
            }*/

            try
            {

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
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while connecting to RabbitMQ: "+ GeneralConfigurationClass.RabbitMqConnectionString +" - " + e.Message);
            }
            
            services.AddSwaggerGen(c =>
            {

                string description = "Documentation of the NMKR Studio Api Functions. All API Functions must called from "+GeneralConfigurationClass.ApiUrl;

                string name = $"NMKR Studio Api ({GeneralConfigurationClass.EnvironmentName})";

                var contact = new OpenApiContact
                {
                    Name = "Support NMKR Studio",
                    Url = new("https://nmkr.io")
                };

                c.OperationFilter<OpenApiHeaderIgnoreFilter>();
                c.SwaggerDoc("v2", new()
                {
                    Title = name,
                    Description = description,
                    Contact = contact,
                    Version = "v2"
                });
                c.EnableAnnotations();
                c.SwaggerDoc("v1", new()
                    {
                    Title = name + " - Deprecated",
                    Description = description,
                    Contact = contact,
                    Version = "v1"
                }
                );
                c.DocInclusionPredicate((version, desc) =>
                {
                    if (!desc.TryGetMethodInfo(out MethodInfo methodInfo)) return false;
                    var versions = methodInfo.DeclaringType.GetCustomAttributes(true).OfType<ApiVersionAttribute>().SelectMany(attr => attr.Versions);
                    var maps = methodInfo.GetCustomAttributes(true).OfType<MapToApiVersionAttribute>().SelectMany(attr => attr.Versions).ToArray();
                    version = version.Replace("v", "");
                    return versions.Any(v => v.ToString() == version && maps.Any(v => v.ToString() == version));
                });
                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Enter the token via the following template: Bearer JWT\nExample:\nBearer ApikeyOrAccessToken",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Reference = new()
                    {
                        Id = "bearer",
                        Type = ReferenceType.SecurityScheme,
                    }
                };
                c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
                c.UseAllOfToExtendReferenceSchemas();
                c.AddSecurityRequirement(new()
                {
                        { securityScheme, Array.Empty<string>() }
                    });
                //  }

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
                c.AddServer(new()
                {
                    Url =  GeneralConfigurationClass.ApiUrl
                });
            });
        // Install the new node
     //   GlobalFunctions.InstallNewCardanoCli();
        GlobalFunctions.CheckMainPassword();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {

            }
            app.UseForwardedHeaders();

            app.UseIpRateLimiting();


            app.UseDeveloperExceptionPage();
            app.UseSwagger(options => options.RouteTemplate = "swagger/{documentName}/swagger.json");


            app.UseSwaggerUI(c =>
                {
                   
                    c.InjectStylesheet("/swagger-ui/custom.css");
                    //if (!GlobalFunctions.IsMainnet())
                    {
                        c.SwaggerEndpoint("/swagger/v2/swagger.json", "NMKR Studio Api v2");
                    }

                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "NMKR Studio Api v1");
                }
            );

            //  app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
