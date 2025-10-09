using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NMKR.BackgroundService.HostedServices.Services;
using NMKR.BackgroundService.HostedServices.StaticFunctions;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Functions.Cli;
using NMKR.Shared.Functions.SystemFunctions;
using NMKR.Shared.Model;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace NMKR.BackgroundService.HostedServices
{
    public class CheckPaymentAddressesService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        private readonly ILogger _iLogger;
        //   private readonly IOptions<GeneralConfigurationClass> _generalConfiguration;

        private const int DelayMilliseconds = 1000;
        private readonly IConnectionMultiplexer _redis;
        readonly IBus _bus;
        private string nodeversion { get; set; } = "";
        private readonly bool mainnet = !GeneralConfigurationClass.UseTestnet;

        public CheckPaymentAddressesService(IServiceScopeFactory serviceScopeFactory,
            ILogger<CheckPaymentAddressesService> iLogger,
            IConnectionMultiplexer redis, IBus bus /* ,IOptions<GeneralConfigurationClass> generalConfiguration*/)
        {
            this._serviceScopeFactory = serviceScopeFactory;
            _iLogger = iLogger;
            _redis = redis;
            _bus = bus;

            StaticBackgroundServerClass._iLogger = iLogger;
            StaticBackgroundServerClass._redis = redis;
            StaticBackgroundServerClass._bus = bus;

        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await using (EasynftprojectsContext db = new(GlobalFunctions.optionsBuilder.Options))
            {
                var bg = await (from a in db.Backgroundservers
                    where a.Id == GlobalFunctions.ServerId
                    select a).FirstOrDefaultAsync(cancellationToken: cancellationToken);

                if (bg != null)
                {
                    bg.Stopserver = true;
                    await db.SaveChangesAsync(cancellationToken);
                }
            }

            await base.StopAsync(cancellationToken);
        }

        protected override async Task<Task> ExecuteAsync(CancellationToken cancellationToken)
        {
            int c = 0;
            bool isactive = false;

            string envhostname = Environment.GetEnvironmentVariable("HOSTNAME");
            string operatingsystem = Environment.OSVersion.VersionString;


            string strHostName = string.Empty;
            // Getting Ip address of local machine...
            // First get the host name of local machine.
            strHostName = Dns.GetHostName();
            // Then using host name, get the IP address list..
            Console.WriteLine(@"Hostname: "+strHostName);

            if (string.IsNullOrEmpty(envhostname))
            {
                envhostname= strHostName;
            }

            IPHostEntry ipEntry = await Dns.GetHostEntryAsync(strHostName, cancellationToken);
            IPAddress[] addr = ipEntry.AddressList;
            addr=addr.Where(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToArray();

            var VersionNumber = System.Reflection.Assembly.GetExecutingAssembly().GetName()?.Version?.ToString();

            _iLogger.LogInformation(DateTime.Now.ToShortDateString() + " " +
                                    DateTime.Now.ToLongTimeString() +
                                    Environment.MachineName +
                                    "Connect to database " +
                                    " - " + c + " - Version: " + VersionNumber);


            while (!cancellationToken.IsCancellationRequested)
            {
                if (GlobalFunctions.ServerId == 0)
                {
                    await GetServerId(cancellationToken, envhostname, addr, VersionNumber, operatingsystem);

                    if (CheckIfRedisKeyExists($"checkserver{GlobalFunctions.ServerId}", out string value2))
                    {
                        GlobalFunctions.ServerId = 0;
                    }
                    if (GlobalFunctions.ServerId == 0)
                    {
                        await Task.Delay(10000, cancellationToken);
                        continue;
                    }

                    SaveToRedis($"checkserver{GlobalFunctions.ServerId}", DateTime.Now.ToLongDateString(),
                        new(0, 0, 10));

                    _iLogger.LogInformation(DateTime.Now.ToShortDateString() + " " +
                                            DateTime.Now.ToLongTimeString() +
                                            " Backgroundtask started " +
                                            Environment.MachineName +
                                            " - " + c + $" - ID: {GlobalFunctions.ServerId}" + " - Version: " + VersionNumber);

                    //  GlobalFunctions.InstallNewCardanoCli();
                    GlobalFunctions.CheckMainPassword();

                    _iLogger.LogInformation(
                        CheckIfRedisKeyExists($"checkserver{GlobalFunctions.ServerId}", out string value)
                            ? $"Redis works - {value}"
                            : $"REDIS ERROR - {GeneralConfigurationClass.Redis.Server}");

                    var ip = await GlobalFunctions.WhatIsMyIp();
                    _iLogger.LogInformation($"My external IP is - {ip}");


                    if (Directory.Exists(GeneralConfigurationClass.TempFilePath))
                        GlobalFunctions.EmptyFolder(GeneralConfigurationClass.TempFilePath);
                    else
                        Directory.CreateDirectory(GeneralConfigurationClass.TempFilePath);

                }
                else
                {
                    await CheckIfServerIdIsStillCorrect(cancellationToken, envhostname, addr);
                }

                if (GlobalFunctions.ServerId == 0)
                {
                    await Task.Delay(10000, cancellationToken);
                    continue;
                }


                c++;
                await using EasynftprojectsContext db = new(GlobalFunctions.optionsBuilder.Options);
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    db.Database.SetCommandTimeout(300);
                    if (GlobalFunctions.ServerId != 0)
                    {
                        try
                        {

                            var server = (from a in db.Backgroundservers
                                where a.Id == GlobalFunctions.ServerId
                                select a).AsNoTracking().FirstOrDefault();

                            if (server != null)
                            {

                                if (server.Stopserver)
                                {
                                    _iLogger.LogInformation(DateTime.Now.ToShortDateString() + " " +
                                                            DateTime.Now.ToLongTimeString() +
                                                            $"Stopserver is set - Application will terminate {Environment.MachineName} - {c} - Version: {VersionNumber}");

                                    await Task.Delay(1000, cancellationToken);
                                    await db.Database.ExecuteSqlRawAsync(
                                        $"update backgroundserver set state='notactive', actualtask='' where id={GlobalFunctions.ServerId} and name='{envhostname}'",
                                        cancellationToken: cancellationToken);
                                    await db.Database.CloseConnectionAsync();
                                    Environment.Exit(0);
                                }

                                if (server.Name != envhostname)
                                {
                                    
                                    _iLogger.LogError(DateTime.Now.ToShortDateString() + " " +
                                                            DateTime.Now.ToLongTimeString() +
                                                            $"Server found not the correct name {envhostname} - {server.Name} - {GlobalFunctions.ServerId} - Version: {VersionNumber}");
                                    GlobalFunctions.ServerId = 0;
                                    continue;
                                }


                                if (server.State != "active")
                                {
                                    await db.Database.ExecuteSqlRawAsync(
                                        $"update backgroundserver set state='active', actualtask='' where id={GlobalFunctions.ServerId} and name='{envhostname}'",
                                        cancellationToken: cancellationToken);
                                }



                                _iLogger.LogInformation(DateTime.Now.ToShortDateString() + " " +
                                                        DateTime.Now.ToLongTimeString() +
                                                        "Backgroundtask started " +
                                                        Environment.MachineName +
                                                        " - " + c + " - Version: " + VersionNumber);


                                if (c % 100 == 0 || c == 1 || !isactive)
                                {
                                    BuildTransactionClass bt = new();
                                    var tip = CliFunctions.GetQueryTipFromCli(mainnet, ref bt);
                                    if (tip != null)
                                        await SaveQueryTip(db, GlobalFunctions.ServerId, tip);

                                    if (!isactive)
                                    {
                                        if (tip == null || tip.SyncProcess != "100.00")
                                        {
                                            await StaticBackgroundServerClass.LogAsync(db,
                                                $"Server is not ready yet - waiting - Version: {VersionNumber} - Syncprocess: {(tip != null ? tip.SyncProcess : "")}",
                                                bt.LogFile,
                                                GlobalFunctions.ServerId);
                                            await Task.Delay(DelayMilliseconds, cancellationToken);
                                            continue;
                                        }
                                        else
                                        {
                                            GlobalFunctions.SaveStringToRedis(_redis, "Era", tip.Era, 86400);

                                            GetInstalledMemory.GetRamBytes(out var free, out var total);
                                            string installedmemory =
                                                $"Total: {GlobalFunctions.GetPrettyFileSizeString((long)total)} - Free: {GlobalFunctions.GetPrettyFileSizeString((long)free)}";

                                            await StaticBackgroundServerClass.LogAsync(db,
                                                $"Server is ready - Version: {VersionNumber} - Syncprocess: {(tip != null ? tip.SyncProcess : "")}",
                                                "", GlobalFunctions.ServerId);

                                            await db.Database.ExecuteSqlRawAsync(
                                                $"update backgroundserver set state='active', pauseserver=0, lastlifesign=NOW(), installedmemory='{installedmemory}'  where id={GlobalFunctions.ServerId}",
                                                cancellationToken: cancellationToken);

                                            isactive = true;

                                            nodeversion = ConsoleCommand.GetNodeVersion();
                                            await db.Database.ExecuteSqlRawAsync(
                                                $"update backgroundserver set nodeversion='{nodeversion}' where id={GlobalFunctions.ServerId}",
                                                cancellationToken: cancellationToken);
                                        }
                                    }
                                }

                                
                                await GlobalFunctions.UpdateLifesignAsync(db, GlobalFunctions.ServerId);

                                if (server.Pauseserver)
                                {
                                    try
                                    {
                                        await Task.Delay(10000, cancellationToken);
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e.Message);
                                    }

                                    continue;
                                }


                                // here
                                await CheckLifesign(db, cancellationToken);


                                // Call Tasks
                                string taskname = "";
                                int taskno = 0;
                                string oldtask = "none1";
                                try
                                {
                                    IEnumerable<Type> commands = AppDomain.CurrentDomain.GetAssemblies()
                                        .SelectMany(x => x.GetTypes())
                                        .Where(t => t.GetInterfaces().Contains(typeof(IBackgroundServices)));
                                    taskname = "none";
                                    foreach (Type type in commands.OrEmptyIfNull())
                                    {
                                        taskno++;
                                        taskname = type.Name;
                                        try
                                        {
                                            IBackgroundServices command =
                                                (IBackgroundServices)Activator.CreateInstance(type);
                                            if (command == null)
                                                continue;
                                            await command.Execute(db, cancellationToken,
                                                c, server, mainnet, GlobalFunctions.ServerId, _redis, _bus);
                                        }
                                        catch (Exception e)
                                        {
                                            await GlobalFunctions.LogExceptionAsync(db,
                                                $"Exception in ExecuteBackgroundTask {e.Message} {taskname}",
                                                e.StackTrace, GlobalFunctions.ServerId);
                                        }

                                        if (cancellationToken.IsCancellationRequested)
                                            break;
                                        oldtask = taskname;
                                        taskname = "";
                                    }
                                }
                                catch (Exception)
                                {
                                    //   if (!e.Message.Contains("Unable to load one or more of the requested types") && taskno<40)
                                 /*   await GlobalFunctions.LogExceptionAsync(db,
                                        $"Exception in ExecuteBackgroundTask (2) {taskname} - {taskno} - {oldtask} - {z}",
                                        e.Message + Environment.NewLine + e.StackTrace, GlobalFunctions.ServerId);*/
                                }

                                _iLogger.LogInformation(DateTime.Now.ToShortDateString() + " " +
                                                        DateTime.Now.ToLongTimeString() +
                                                        " Backgroundtask ended " +
                                                        Environment.MachineName +
                                                        " - " + c + $" - ID: {GlobalFunctions.ServerId}" + " - Version: " + VersionNumber);

                                await StaticBackgroundServerClass.LogAsync(db,
                                    $"Backgroundtask ended {Environment.MachineName} - {c}", "",
                                    GlobalFunctions.ServerId);
                            }
                            else
                            {
                                await StaticBackgroundServerClass.LogAsync(db,
                                    $"Error - SERVERID NOT KNOWN IN DATABASE - {GlobalFunctions.ServerId}");
                            }
                        }
                        catch (Exception e)
                        {
                            await StaticBackgroundServerClass.EventLogException(db, 0, e, GlobalFunctions.ServerId);
                        }
                    }
                }

                _iLogger.LogInformation("Task ended");
                await Task.Delay(DelayMilliseconds, cancellationToken);

                if (c == 100000)
                    c = 0;

                await db.Database.CloseConnectionAsync();
            }

            await using (EasynftprojectsContext db = new(GlobalFunctions.optionsBuilder.Options))
            {
                isactive = false;
                // If the server is shutting down - delegate all tasks to an other server
                await db.Database.ExecuteSqlRawAsync(
                    $"update backgroundserver set state='notactive', ipaddress='',name='' where id={GlobalFunctions.ServerId} and name='{envhostname}'", cancellationToken: cancellationToken);
            }
            return Task.CompletedTask;
        }

        private async Task CheckIfServerIdIsStillCorrect(CancellationToken cancellationToken, string envhostname, IPAddress[] addr)
        {
            await using EasynftprojectsContext db = new(GlobalFunctions.optionsBuilder.Options);
            var digitaloceanserver = await (from a in db.Backgroundservers
                where a.Id==GlobalFunctions.ServerId
                select a).AsNoTracking().FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (digitaloceanserver == null)
            {
                GlobalFunctions.ServerId = 0;
                _iLogger.LogInformation($@"ServerId not found in database - resetting - {GlobalFunctions.ServerId}");
                return;
            }

            if (digitaloceanserver.Name != envhostname)
            {
                GlobalFunctions.ServerId = 0;

                _iLogger.LogInformation($@"ServerId is not correct anymore - resetting - {digitaloceanserver.Ipaddress} - {addr.First().ToString()} - {digitaloceanserver.Name} - {envhostname}");
                return;
            }
        }

        private async Task<bool> GetServerId(CancellationToken cancellationToken, string envhostname, IPAddress[] addr,
            string VersionNumber, string operatingsystem)
        {
            await using EasynftprojectsContext db = new(GlobalFunctions.optionsBuilder.Options);
            db.Database.SetCommandTimeout(300);

            _iLogger.LogInformation(@"Environment Hostname: " + envhostname);
            foreach (var ipAddress in addr)
            {
                _iLogger.LogInformation(@"IP-Address: " + envhostname + " "+ ipAddress.ToString());
            }

            if (!string.IsNullOrEmpty(envhostname))
            {
                var srv = await (from a in db.Backgroundservers
                    where a.Name == envhostname
                    select a).FirstOrDefaultAsync(cancellationToken);
                if (srv != null)
                {
                    _iLogger.LogInformation($@"Found Server {srv.Id} by Hostname: {envhostname}");
                    GlobalFunctions.ServerId = srv.Id;
                }
            }

            if (GlobalFunctions.ServerId == 0)
            {
                var digitaloceanserver = await (from a in db.Backgroundservers
                    where a.Digitaloceanserver && a.State != "deleted" && (a.Lastlifesign < DateTime.Now.AddSeconds(-30) || a.Name=="" || a.Ipaddress=="")
                                                select a).AsNoTracking().ToListAsync(cancellationToken: cancellationToken);

                if (digitaloceanserver.Count==0)
                {
                    await StaticBackgroundServerClass.LogAsync(db,
                        $"Error - No Digitalocean Server found - Please create one in the database "+ envhostname);
                    return true;
                }

                GlobalFunctions.ServerId = digitaloceanserver.First().Id;
            }
            if (GlobalFunctions.ServerId != 0)
                await db.Database.ExecuteSqlRawAsync(
                    $"update backgroundserver set state='notactive', lastlifesign=NOW(), stopserver=0, runningversion='{VersionNumber}', " +
                    $"operatingsystem='{operatingsystem}', ipaddress='{addr.First().ToString()}', name='{envhostname}' where id={GlobalFunctions.ServerId}", cancellationToken: cancellationToken);
            else
            {
                await StaticBackgroundServerClass.LogAsync(db,
                    $"Error - Server ID not found -  - Version: {VersionNumber}");
                return true;
            }

            return false;
        }

        private async Task<bool> CheckIfServerResponds(Backgroundserver backgroundserver)
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(5);

            try
            {
                Console.WriteLine($@"Checking Server {backgroundserver.Id} - {backgroundserver.Ipaddress} - http://"+backgroundserver.Ipaddress+":5000/api/check");
                // HEAD-Request verwendet weniger Bandbreite
                var response = await httpClient.SendAsync(
                    new HttpRequestMessage(HttpMethod.Head, "http://"+backgroundserver.Ipaddress+":5000/api/check"));
                return response.IsSuccessStatusCode;
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine($@"Timeout checking server {backgroundserver.Id}");
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine($@"Error checking server {backgroundserver.Id} - {e.Message}");
                return false;
            }
        }


        /// <summary>
        /// Saves the actual Tip from the node to the database (for information in the admintool)
        /// </summary>
        /// <param name="db"></param>
        /// <param name="serverid"></param>
        /// <param name="tip"></param>
        /// <returns></returns>
        private async Task SaveQueryTip(EasynftprojectsContext db, int serverid, Querytip tip)
        {
            var bg = await (from a in db.Backgroundservers
                where a.Id == GlobalFunctions.ServerId
                select a).FirstOrDefaultAsync();

            if (bg != null)
            {
                bg.Syncprogress = tip.SyncProcess;
                bg.Slot = tip.Slot.ToString();
                bg.Block = tip.Block.ToString();
                bg.Era = tip.Era;
                bg.Epoch = tip.Epoch.ToString();
                await db.SaveChangesAsync();
            }
        }

        private void SaveToRedis(string key, string parameter, TimeSpan expire)
        {
            IDatabase dbr = _redis.GetDatabase();
            dbr.StringSet(key, parameter, expiry: expire);
        }

        private bool CheckIfRedisKeyExists(string key, out string value)
        {
            value = null;
            IDatabase db = _redis.GetDatabase();
            var res = db.StringGet(key);
            // var a=db.ListLeftPop(new RedisKey("ddd"),10, CommandFlags.None);
            if (!res.IsNull)
                value = res.ToString();
            return !res.IsNull;
        }

        private async Task CheckLifesign(EasynftprojectsContext db, CancellationToken cancellationToken)
        {
            try
            {
                var t = await (from a in db.Backgroundservers
                    where a.State == "active" && a.Lastlifesign < DateTime.Now.AddMinutes(-30)
                    select a).ToListAsync(cancellationToken);

                foreach (var backgroundserver in t)
                {
                    backgroundserver.State = "notactive"; // Do not give this server more tasks
                    if (!backgroundserver.Pauseserver)
                    {
                        backgroundserver.Stopserver =
                            true; // This is, that the server will come online again, if we have stopped him mistakenly
                    }

                    await db.SaveChangesAsync(cancellationToken);
                }
            }
            catch (Exception e)
            {
                await StaticBackgroundServerClass.EventLogException(db, 33, e, GlobalFunctions.ServerId);
            }
        }
    }
}

