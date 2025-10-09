using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NMKR.BackgroundService.HostedServices.StaticFunctions;
using NMKR.Shared.Blockchains;
using NMKR.Shared.Blockchains.APTOS;
using NMKR.Shared.Blockchains.Solana;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;


namespace NMKR.BackgroundService.HostedServices.Services
{
    public class CheckForCreateCollection : IBackgroundServices
    {
        public async Task Execute(EasynftprojectsContext db, CancellationToken cancellationToken, int counter, Backgroundserver server,
            bool mainnet, int serverid, IConnectionMultiplexer redis, IBus bus)
        {
            if (server.Checkpoliciessolana == false)
                return;

            var projects = await (from a in db.Nftprojects
                    .Include(a => a.Customer)
                where  (a.Solanacollectiontransaction == "<PENDING>" || a.Aptoscollectiontransaction=="<PENDING>") && a.State == "active"
                select a).Take(10).ToListAsync(cancellationToken: cancellationToken);



            foreach (var project in projects)
            {
                if (project.Solanacollectiontransaction == "<PENDING>")
                    await CreateCollectionAsync(db, new SolanaBlockchainFunctions(), Blockchain.Solana, project,
                        cancellationToken, serverid);
                if (project.Aptoscollectiontransaction == "<PENDING>")
                    await CreateCollectionAsync(db, new AptosBlockchainFunctions(), Blockchain.Aptos, project,
                        cancellationToken, serverid);

            }
        }

        private async Task CreateCollectionAsync(EasynftprojectsContext db, IBlockchainFunctions blockchainFunctions, Blockchain blockchain, Nftproject project, CancellationToken cancellationToken, int serverid)
        {
            await StaticBackgroundServerClass.LogAsync(db,
                  $"Adding {blockchain.ToString()} NFT Collection {project.Projectname}",
                  "", serverid);

            var paywallet = await GlobalFunctions.GetNmkrPaywalletAndBlockAsync(db, 0, $"CheckForCreateCollection{blockchain.ToString()}", "",blockchain.ToCoin());
         
            if (paywallet != null)
            {
                var col = await blockchainFunctions.CreateNewCollectionClass(project, paywallet);
                var collectionAddress = await blockchainFunctions.CreateCollectionAsync(col);
                if (collectionAddress != null && !collectionAddress.Contains("Error"))
                {
                    await StaticBackgroundServerClass.LogAsync(db,
                        $"Adding {blockchain.ToString()} NFT Collection {project.Projectname} - successful",
                        collectionAddress, serverid);

                    switch (blockchain)
                    {
                        case Blockchain.Solana:
                            project.Solanacollectioncreated = DateTime.Now;
                            project.Solanacollectiontransaction = collectionAddress;
                            break;
                        case Blockchain.Aptos:
                            project.Aptoscollectioncreated = DateTime.Now;
                            project.Aptoscollectiontransaction = collectionAddress;
                            break;
                    }


                    await db.SaveChangesAsync(cancellationToken);


                    // Send Notification to User
                    Onlinenotification on = new()
                    {
                        Created = DateTime.Now,
                        CustomerId = project.CustomerId,
                        Notificationmessage =
                            $"{blockchain.ToString()} Collection for Project {project.Projectname} was successfully created",
                        State = "new",
                        Color = "success"
                    };
                    await db.Onlinenotifications.AddAsync(on, cancellationToken);
                    await db.SaveChangesAsync(cancellationToken);
                }

                await GlobalFunctions.UnlockPaywalletAsync(db, paywallet);
            }
        }
    }
}
