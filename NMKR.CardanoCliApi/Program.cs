using NMKR.Shared.Classes;
using NMKR.Shared.Classes.Cli;
using NMKR.Shared.Functions;

internal class Program
{
    public static IConfiguration Configuration;

    static void Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "CardanoCliApi",
                Version = "v1"
            });
        });


        string? env = Environment.GetEnvironmentVariable("CARDANO_NETWORK");
        
        if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "settings.yaml"))
        {
            GeneralConfigurationClass.CreateYamlFile(AppDomain.CurrentDomain.BaseDirectory + "settings.yaml");
        }

        var builder1 = new ConfigurationBuilder()
            .AddYamlFile("settings.yaml", optional: false, reloadOnChange: true)
            .AddYamlFile($"settings.{env}.yaml", optional: true, reloadOnChange: true);

        builder1.AddEnvironmentVariables();

        Configuration = builder1.Build();
        GeneralConfigurationClass.ReadConfiguration(Configuration);


        var app = builder.Build();

        // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "CardanoCliApi v1");
            });


      //  app.UseHttpsRedirection();
        
      //  GlobalFunctions.InstallNewCardanoCli();

        app.MapPost("/RemoteCallCardanoCli/", (CliCommand command) =>
            {
                string log = command.Command + Environment.NewLine;
                foreach (var file in command.InFiles.OrEmptyIfNull())
                {
                    log+= "Writing file /tmp/" + file.FileName + Environment.NewLine;
                    System.IO.File.WriteAllText("/tmp/"+file.FileName, file.Content);
                }

                var res = ConsoleCommand.CallCardanoCli(command.Command, out var errormessage);
                log += "Res: "+res + Environment.NewLine;
                foreach (var outfile in command.OutFiles.OrEmptyIfNull())
                {
                    if (File.Exists("/tmp/" + outfile.FileName))
                    {
                        log += "Reading file /tmp/" + outfile.FileName + Environment.NewLine;
                        outfile.Content = System.IO.File.ReadAllText("/tmp/" + outfile.FileName);
                    }
                }

                // Delete all files
                foreach (var file in command.InFiles.OrEmptyIfNull())
                {
                   GlobalFunctions.DeleteFile("/tmp/" + file.FileName);
                }
                foreach (var outfile in command.OutFiles.OrEmptyIfNull())
                {
                    GlobalFunctions.DeleteFile("/tmp/" + outfile.FileName);
                }

                return new RemoteCallCardanoCliResultClass() { ErrorMessage = errormessage, Result = res, OutFiles = command.OutFiles, Log=log};
            })
            .WithName("RemoteCallCardanoCli")
            .WithOpenApi();


        app.MapGet("/check", () => new {
            Status = "OK",
            SocketPath = Environment.GetEnvironmentVariable("CARDANO_NODE_SOCKET_PATH"),
            CliPath = Environment.GetEnvironmentVariable("CardanoCli"),
            NetworkId = Environment.GetEnvironmentVariable("CARDANO_NODE_NETWORK_ID"),
            UseTestnet = Environment.GetEnvironmentVariable("UseTestnet")
        }).WithName("Check")
        .WithOpenApi(); ;

        app.Run();
    }

}


