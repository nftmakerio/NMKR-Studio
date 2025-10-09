using System;
using System.Collections.Generic;
using System.Text;
using NMKR.Shared.Classes.Crossmint;
using Microsoft.Extensions.Configuration;

namespace NMKR.Shared.Classes
{
    public class KafkaConfiguration
    {
        /// <summary>
        /// List of Kafka endpoints, e.g. localhost:9092
        /// </summary>
        public List<string> Servers { get; set; } = new List<string>();

        /// <summary>
        /// Topic name to work with
        /// </summary>
        public string Topic { get; set; }

        /// <summary>
        /// Id of the consumer group (in case of consuming)
        /// </summary>
        public string ConsumerGroup { get; set; }
    }
    public class ApiKeyUrlConfiguration
    {
        public string ApiUrl { get; set; }
        public string ApiKey { get; set; }
    }
    public class UsernamePasswordConfiguration
    {
        public string Server { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
    public static class GeneralConfigurationClass
    {
        /// <summary>
        /// Where is the Cardano Cli installed.
        /// </summary>
        public static string CardanoCli { get; internal set; }

        /// <summary>
        /// Where are the temporary files located.
        /// </summary>
        public static string TempFilePath { get; internal set; } = @"/tmp/";

        /// <summary>
        /// Whether the Cardano testnet should be used instead of the mainnet
        /// </summary>
        public static bool UseTestnet { get; internal set; } = false;

      //  public static string RedisServer { get; internal set; } 
//        public static string RedisPassword { get; internal set; }


        public static UsernamePasswordConfiguration Redis { get; internal set; } = new UsernamePasswordConfiguration();

        // Email
        public static string AWSEmailServer { get; internal set; } 
        public static string AWSEmailPassword { get; internal set; } 
        public static string AWSEmailUsername { get; internal set; } 

        // Google
        public static string AuthenticatorSecret { get; internal set; }
        public static string RecaptchaWebsite { get; internal set; }

        // IPFS
        public static string IPFSApi { get; internal set; }
        public static string IPFSGateway { get; internal set; }

        // Yota
        public static string YotaSdkId { get; internal set; }

        // Messagebird
        public static string MessageBirdAccessKey { get; internal set; }

        // Rebex
        public static string RebexLicencekey { get; internal set; }

        public static string CliLogPath { get; set; }

        public static int? SFTPServerPort { get; internal set; }

        public static string ConnectionString { get; set; }
        public static string ConnectionStringDbSync { get; set; }

        private static string Koios;
        public static string KoiosApi
        {
            get
            {
                if (!string.IsNullOrEmpty(Koios) && !Koios.Contains("/api/"))
                {
                    return Koios + "/api/v1";
                }
                else
                {
                   return Koios;
                }
            }
        }

        public static string BlockfrostUrl;
        public static string BlockfrostApikey { get; set; }


        // Mailerlite
        public static string MailerliteUrl { get; set; }
        public static string MailerliteKey { get; set; }
        public static string MailerliteGroupId { get; set; }


        // NMKR
        public static string ApiUrl { get; internal set; }
        public static string CardanoCliApiUrl { get; internal set; }
        public static string StudioUrlMainnet { get; internal set; }
        public static string StudioUrlTestnet { get; internal set; }

        public static string CslBuildCbor { get; internal set; }


        public static string Paywindowlink { get; internal set; } 

        public static string SFTP_Server { get; internal set; }
        public static string EnvironmentName { get; internal set; } = "";

        public static string Era { get; internal set; } 
        public static string TestnetMagicId { get; internal set; } 

        public static string StoreApiUrl { get; internal set; }
        public static bool EnableInternalWallets { get; internal set; } = false;


        // RabbitMq
        public static string RabbitMqConnectionString { get; internal set; }
        public static string RabbitMqUsername { get; internal set; }
        public static string RabbitMqPassword { get; internal set; }


        public static ApiKeyUrlConfiguration MaestroConfiguration { get; internal set; } = new ApiKeyUrlConfiguration();
        public static ApiKeyUrlConfiguration MaestroBitcoinConfiguration { get; internal set; } = new ApiKeyUrlConfiguration();
        public static ApiKeyUrlConfiguration IagonConfiguration { get; internal set; } = new ApiKeyUrlConfiguration();
        public static ApiKeyUrlConfiguration HeliosConfiguration { get; internal set; } = new ApiKeyUrlConfiguration();

        public static CrossmintSettings CrossmintSettings { get; internal set; }= new CrossmintSettings();

        public static string CslService { get; internal set; } = "https://studio-cslservice.preprod.nmkr.io/api/v1/";
        public static string ProjectNameShort { get; internal set; }
        public static string ProjectNameAuthenticator { get; internal set; }
        public static string ApiTokenPassword { get; internal set; }
        public static string SolanaApiUrl { get; internal set; }
        public static string AptosApiUrl { get; internal set; }
        public static string AptosNodeApiKey { get; set; }
        public static string MidnightApiUrl { get; internal set; }

        public static string Masterpassword { get; internal set; }

        public static void ReadConfiguration(IConfiguration configuration)
        {
            if (!string.IsNullOrEmpty(configuration.GetSection("UseTestnet").Value))
                UseTestnet = configuration.GetSection("UseTestnet").Value == "true";

            if (!string.IsNullOrEmpty(configuration.GetSection("EnableInternalWallets").Value))
                EnableInternalWallets = configuration.GetSection("EnableInternalWallets").Value == "true";
            if (!string.IsNullOrEmpty(configuration.GetSection("ProjectNameShort").Value))
                ProjectNameShort = configuration.GetSection("ProjectNameShort").Value;

            if (!string.IsNullOrEmpty(configuration.GetSection("ApiTokenPassword").Value))
                ApiTokenPassword = configuration.GetSection("ApiTokenPassword").Value;

            if (!string.IsNullOrEmpty(configuration.GetSection("CardanoCli").Value))
                CardanoCli = configuration.GetSection("CardanoCli").Value;
            if (!string.IsNullOrEmpty(configuration.GetSection("TempFilePath").Value))
                TempFilePath = configuration.GetSection("TempFilePath").Value;
         /*   if (!string.IsNullOrEmpty(configuration.GetSection("RedisServer").Value))
                RedisServer = configuration.GetSection("RedisServer").Value;
            if (!string.IsNullOrEmpty(configuration.GetSection("RedisPassword").Value))
                RedisPassword = configuration.GetSection("RedisPassword").Value;*/
            if (!string.IsNullOrEmpty(configuration.GetSection("CliLogPath").Value))
                CliLogPath = configuration.GetSection("CliLogPath").Value;

            if (!string.IsNullOrEmpty(configuration.GetSection("ConnectionString").Value)) 
                ConnectionString = configuration.GetSection("ConnectionString").Value;
            if (!string.IsNullOrEmpty(configuration.GetSection("ConnectionStringDbSync").Value))
                ConnectionStringDbSync = configuration.GetSection("ConnectionStringDbSync").Value;

            if (!string.IsNullOrEmpty(configuration.GetSection("SFTPServerPort").Value))
                SFTPServerPort = Convert.ToInt32(configuration.GetSection("SFTPServerPort").Value);

            // Koios
            if (!string.IsNullOrEmpty(configuration.GetSection("Koios").Value))
                Koios = configuration.GetSection("Koios").Value;

           

            // Blockfrost
            if (!string.IsNullOrEmpty(configuration.GetSection("BlockfrostUrl").Value))
                BlockfrostUrl = configuration.GetSection("BlockfrostUrl").Value;
            if (!string.IsNullOrEmpty(configuration.GetSection("BlockfrostApikey").Value))
                BlockfrostApikey = configuration.GetSection("BlockfrostApikey").Value;


            // Cardano
            if (!string.IsNullOrEmpty(configuration.GetSection("Era").Value))
                Era = configuration.GetSection("Era").Value;
            if (!string.IsNullOrEmpty(configuration.GetSection("TestnetMagicId").Value))
                TestnetMagicId = configuration.GetSection("TestnetMagicId").Value;


            // IPFS
            if (!string.IsNullOrEmpty(configuration.GetSection("IPFSGateway").Value))
                IPFSGateway = configuration.GetSection("IPFSGateway").Value;
            if (!string.IsNullOrEmpty(configuration.GetSection("IPFSApi").Value))
                IPFSApi = configuration.GetSection("IPFSApi").Value;

            // AWS Email
            if (!string.IsNullOrEmpty(configuration.GetSection("AWSEmailServer").Value))
                AWSEmailServer = configuration.GetSection("AWSEmailServer").Value;
            if (!string.IsNullOrEmpty(configuration.GetSection("AWSEmailPassword").Value))
                AWSEmailPassword = configuration.GetSection("AWSEmailPassword").Value;
            if (!string.IsNullOrEmpty(configuration.GetSection("AWSEmailUsername").Value))
                AWSEmailUsername = configuration.GetSection("AWSEmailUsername").Value;

            // Google
            if (!string.IsNullOrEmpty(configuration.GetSection("RecaptchaWebsite").Value))
                RecaptchaWebsite = configuration.GetSection("RecaptchaWebsite").Value;
            if (!string.IsNullOrEmpty(configuration.GetSection("AuthenticatorSecret").Value))
                AuthenticatorSecret = configuration.GetSection("AuthenticatorSecret").Value;

            // Yota
            if (!string.IsNullOrEmpty(configuration.GetSection("YotaSdkId").Value))
                YotaSdkId = configuration.GetSection("YotaSdkId").Value;

            // Messagebird
            if (!string.IsNullOrEmpty(configuration.GetSection("MessageBirdAccessKey").Value))
                MessageBirdAccessKey = configuration.GetSection("MessageBirdAccessKey").Value;

            // Rebex
            if (!string.IsNullOrEmpty(configuration.GetSection("RebexLicencekey").Value))
                RebexLicencekey = configuration.GetSection("RebexLicencekey").Value;


            // NMKR
            if (!string.IsNullOrEmpty(configuration.GetSection("ApiUrl").Value))
                ApiUrl = configuration.GetSection("ApiUrl").Value;
            if (!string.IsNullOrEmpty(configuration.GetSection("CardanoCliApiUrl").Value))
                CardanoCliApiUrl = configuration.GetSection("CardanoCliApiUrl").Value;

            if (!string.IsNullOrEmpty(configuration.GetSection("StudioUrlMainnet").Value))
                StudioUrlMainnet = configuration.GetSection("StudioUrlMainnet").Value;
            if (!string.IsNullOrEmpty(configuration.GetSection("StudioUrlTestnet").Value))
                StudioUrlTestnet = configuration.GetSection("StudioUrlTestnet").Value;

            if (!string.IsNullOrEmpty(configuration.GetSection("CslBuildCbor").Value))
                CslBuildCbor = configuration.GetSection("CslBuildCbor").Value;
            if (!string.IsNullOrEmpty(configuration.GetSection("CslService").Value))
                CslService = configuration.GetSection("CslService").Value;
            if (!string.IsNullOrEmpty(configuration.GetSection("Paywindowlink").Value))
                Paywindowlink = configuration.GetSection("Paywindowlink").Value;
            if (!string.IsNullOrEmpty(configuration.GetSection("SFTP_Server").Value))
                SFTP_Server = configuration.GetSection("SFTP_Server").Value;
            if (!string.IsNullOrEmpty(configuration.GetSection("EnvironmentName").Value))
                EnvironmentName = configuration.GetSection("EnvironmentName").Value;

            // Store
            if (!string.IsNullOrEmpty(configuration.GetSection("StoreApiUrl").Value))
                StoreApiUrl = configuration.GetSection("StoreApiUrl").Value;

            // RabbitMQ
            if (!string.IsNullOrEmpty(configuration.GetSection("RabbitMqConnectionString").Value))
                RabbitMqConnectionString = configuration.GetSection("RabbitMqConnectionString").Value;
            if (!string.IsNullOrEmpty(configuration.GetSection("RabbitMqUsername").Value))
                RabbitMqUsername = configuration.GetSection("RabbitMqUsername").Value;
            if (!string.IsNullOrEmpty(configuration.GetSection("RabbitMqPassword").Value))
                RabbitMqPassword = configuration.GetSection("RabbitMqPassword").Value;


            // Mailerlite
            if (!string.IsNullOrEmpty(configuration.GetSection("MailerliteUrl").Value))
                MailerliteUrl = configuration.GetSection("MailerliteUrl").Value;
            if (!string.IsNullOrEmpty(configuration.GetSection("MailerliteKey").Value))
                MailerliteKey = configuration.GetSection("MailerliteKey").Value;
            if (!string.IsNullOrEmpty(configuration.GetSection("MailerliteGroupId").Value))
                MailerliteGroupId = configuration.GetSection("MailerliteGroupId").Value;

            configuration.GetSection("Crossmint").Bind(CrossmintSettings);
            configuration.GetSection("Maestro").Bind(MaestroConfiguration);
            configuration.GetSection("MaestroBitcoin").Bind(MaestroBitcoinConfiguration);

            configuration.GetSection("Iagon").Bind(IagonConfiguration);
            configuration.GetSection("Helios").Bind(HeliosConfiguration);

            configuration.GetSection("Redis").Bind(Redis);

            if (Redis==null || string.IsNullOrEmpty(Redis.Server))
            {
                Redis = new UsernamePasswordConfiguration();

                if (!string.IsNullOrEmpty(configuration.GetSection("RedisServer").Value))
                    Redis.Server = configuration.GetSection("RedisServer").Value;
                if (!string.IsNullOrEmpty(configuration.GetSection("RedisPassword").Value))
                    Redis.Password = configuration.GetSection("RedisPassword").Value;
            }

            if (!string.IsNullOrEmpty(configuration.GetSection("SolanaApiUrl").Value))
                SolanaApiUrl = configuration.GetSection("SolanaApiUrl").Value;
            if (!string.IsNullOrEmpty(configuration.GetSection("AptosApiUrl").Value))
                AptosApiUrl = configuration.GetSection("AptosApiUrl").Value;
            if (!string.IsNullOrEmpty(configuration.GetSection("AptosNodeApiKey").Value))
                AptosNodeApiKey = configuration.GetSection("AptosNodeApiKey").Value;
            if (!string.IsNullOrEmpty(configuration.GetSection("MidnightApiUrl").Value))
                MidnightApiUrl = configuration.GetSection("MidnightApiUrl").Value;
            if (!string.IsNullOrEmpty(configuration.GetSection("Masterpassword").Value))
            {
                byte[] bytes = Convert.FromBase64String(configuration.GetSection("Masterpassword").Value);
                Masterpassword = Encoding.UTF8.GetString(bytes);
            }
        }

        public static void CreateYamlFile(string filename)
        {
            string s = $$"""
                       UseTestnet: {{GetEnvVar("UseTestnet")}}
                       EnvironmentName: {{GetEnvVar("EnvironmentName")}}
                       Masterpassword: {{GetEnvVar("Masterpassword")}}
                       ConnectionString: {{GetEnvVar("ConnectionString")}}
                       CardanoCli: {{GetEnvVar("CardanoCli")}}
                       TempFilePath: {{GetEnvVar("TempFilePath")}}
                       
                       Redis:
                               Server: {{GetEnvVar("Redis_Server")}}
                               Password: {{GetEnvVar("Redis_Password")}}
                               Username: {{GetEnvVar("Redis_Username")}}
                               
                       Koios: {{GetEnvVar("Koios")}}
                       Era: {{GetEnvVar("Era")}}
                       TestnetMagicId: {{GetEnvVar("TestnetMagicId")}}
                       BlockfrostUrl: {{GetEnvVar("BlockfrostUrl")}}
                       BlockfrostApikey: {{GetEnvVar("BlockfrostApikey")}}
                       SolanaApiUrl: {{GetEnvVar("SolanaApiUrl")}}
                       AptosApiUrl: {{GetEnvVar("AptosApiUrl")}}
                       AptosNodeApiKey: {{GetEnvVar("AptosNodeApiKey")}}
                       MidnightApiUrl: {{GetEnvVar("MidnightApiUrl")}}
                       IPFSGateway: {{GetEnvVar("IPFSGateway")}}
                       IPFSApi: {{GetEnvVar("IPFSApi")}}
                       AWSEmailServer: {{GetEnvVar("AWSEmailServer")}}
                       AWSEmailPassword: {{GetEnvVar("AWSEmailPassword")}}
                       AWSEmailUsername: {{GetEnvVar("AWSEmailUsername")}}
                       RecaptchaWebsite: {{GetEnvVar("RecaptchaWebsite")}}
                       AuthenticatorSecret: {{GetEnvVar("AuthenticatorSecret")}}
                       MessageBirdAccessKey: {{GetEnvVar("MessageBirdAccessKey")}}
                       RebexLicencekey: {{GetEnvVar("RebexLicencekey")}}
                       ApiUrl: {{GetEnvVar("ApiUrl")}}
                       CardanoCliApiUrl: {{GetEnvVar("CardanoCliApiUrl")}}
                       ApiTokenPassword: {{GetEnvVar("ApiTokenPassword")}}
                       StudioUrlMainnet: {{GetEnvVar("StudioUrlMainnet")}}
                       StudioUrlTestnet: {{GetEnvVar("StudioUrlTestnet")}}
                       CslBuildCbor: {{GetEnvVar("CslBuildCbor")}}
                       Paywindowlink: {{GetEnvVar("Paywindowlink")}}
                       SFTP_Server: {{GetEnvVar("SFTP_Server")}}
                       RabbitMqConnectionString: {{GetEnvVar("RabbitMqConnectionString")}}
                       RabbitMqUsername: {{GetEnvVar("RabbitMqUsername")}}
                       RabbitMqPassword: {{GetEnvVar("RabbitMqPassword")}}
                       
                       Maestro:
                               ApiUrl: {{GetEnvVar("Maestro_ApiUrl")}}
                               ApiKey: {{GetEnvVar("Maestro_ApiKey")}}
                               
                       MaestroBitcoin:
                               ApiUrl: {{GetEnvVar("Maestro_Bitcoin_ApiUrl")}}
                               ApiKey: {{GetEnvVar("Maestro_Bitcoin_ApiUrl")}}        
                               
                       Helios:
                               ApiUrl: {{GetEnvVar("Helios_ApiUrl")}}
                               ApiKey: {{GetEnvVar("Helios_ApiKey")}}
                               
                       
                       
                       """;
            System.IO.File.WriteAllText(filename, s);
        }

        private static string GetEnvVar(string name)
        {
            var env = Environment.GetEnvironmentVariable(name);
            return env ?? "";
        }
    }

}

