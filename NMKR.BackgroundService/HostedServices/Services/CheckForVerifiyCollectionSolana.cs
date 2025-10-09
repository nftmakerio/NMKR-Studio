using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NMKR.BackgroundService.HostedServices.StaticFunctions;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Functions.Solana;
using NMKR.Shared.Model;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace NMKR.BackgroundService.HostedServices.Services
{
    public class CheckForVerifiyCollectionSolana : IBackgroundServices
    {
        public async Task Execute(EasynftprojectsContext db, CancellationToken cancellationToken, int counter, Backgroundserver server,
            bool mainnet, int serverid, IConnectionMultiplexer redis, IBus bus)
        {
            if (server.Checkpoliciessolana == false)
                return;


            var nfts = await (from a in db.Nfts
                    .Include(a => a.Nftproject)
                    .ThenInclude(a => a.Customer)
                where a.Nftproject.Enabledcoins.Contains(Coin.SOL.ToString()) && a.Nftproject.Solanacollectiontransaction != null &&
                      a.Nftproject.Solanacollectiontransaction != "<PENDING>"
                      && a.Verifiedcollectionsolana == "mustbeadded" && a.Solanatokenhash != null
                      && a.State == "sold" && a.Selldate != null
                      && a.Selldate.Value.AddMinutes(1) < System.DateTime.UtcNow &&
                      a.Selldate.Value.AddMinutes(60) > System.DateTime.UtcNow
                select a).Take(20).ToListAsync(cancellationToken: cancellationToken);


            foreach (var nft in nfts)
            {
                await StaticBackgroundServerClass.LogAsync(db,
                    $"Adding Solana NFT to verified collection  {nft.Nftproject.Policyid}.{(nft.Nftproject.Tokennameprefix ?? "")}{nft.Name} - {nft.Assetid} - {nft.Nftproject.Projectname} - {nft.NftprojectId} - NFTID: {nft.Id}",
                    "", serverid);
                nft.Lastpolicycheck = DateTime.Now;
                await db.SaveChangesAsync(cancellationToken);

                var paywallet = await GlobalFunctions.GetNmkrPaywalletAndBlockAsync(db, 0,"CheckForVerifyCollectionSolana",null, Coin.SOL);
                if (paywallet != null)
                {
                    var projectWallet = SolanaFunctions.GetWallet(nft.Nftproject);
                    var projectAccount = projectWallet.GetAccount(nft.Id);
                    var verifyResult = await SolanaFunctions.AddToSolanaCollectionAsync(db, nft.Solanatokenhash,
                        nft.Nftproject.Solanacollectiontransaction, paywallet, SolanaFunctions.ConvertToSolanaKeysClass(nft.Nftproject));
                    if (verifyResult is {Result: not null})
                    {
                        await StaticBackgroundServerClass.LogAsync(db,
                            $"Success Adding Solana NFT to verified collection  {nft.Nftproject.Policyid}.{(nft.Nftproject.Tokennameprefix ?? "")}{nft.Name} - {nft.Assetid} - {nft.Nftproject.Projectname} - {nft.NftprojectId} - NFTID: {nft.Id}",
                            JsonConvert.SerializeObject(verifyResult), serverid);
                        nft.Verifiedcollectionsolana = "success";
                        nft.Verifiedcollectionsignature = verifyResult.Result.Response.Signature;

                        await db.SaveChangesAsync(cancellationToken);
                    }
                    else
                    {
                        await StaticBackgroundServerClass.LogAsync(db,
                            $"ERROR Adding Solana NFT to verified collection  {nft.Nftproject.Policyid}.{(nft.Nftproject.Tokennameprefix ?? "")}{nft.Name} - {nft.Assetid} - {nft.Nftproject.Projectname} - {nft.NftprojectId} - NFTID: {nft.Id}",
                            "", serverid);
                        await GlobalFunctions.UnlockPaywalletAsync(db, paywallet);
                    }
                }
                else break;

            }
        }
    }
}
