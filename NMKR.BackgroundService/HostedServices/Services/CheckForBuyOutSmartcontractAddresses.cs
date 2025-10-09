using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NMKR.BackgroundService.HostedServices.StaticFunctions;
using NMKR.Shared.Classes;
using NMKR.Shared.Classes.Cardano_Sharp;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Functions.Api;
using NMKR.Shared.Model;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;
using DateTime = System.DateTime;

namespace NMKR.BackgroundService.HostedServices.Services
{
    public class CheckForBuyOutSmartcontractAddresses : IBackgroundServices
    {
        public async Task Execute(EasynftprojectsContext db, CancellationToken cancellationToken, int counter, Backgroundserver server,
            bool mainnet, int serverid, IConnectionMultiplexer redis, IBus bus)
        {
            var backgroundtask = BackgroundTaskEnums.checkbuyinsmartcontractaddresses;
            if (server.Checkbuyinsmartcontractaddresses == false)
                return;

            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(backgroundtask, mainnet, serverid, redis);
            await StaticBackgroundServerClass.LogAsync(db, $"{backgroundtask} {Environment.MachineName}", "", serverid);

            var addresses = await(from a in db.Buyoutsmartcontractaddresses
                where (a.State == "active" || a.State == "payment_received")
                select a).ToListAsync(cancellationToken: cancellationToken);


            foreach (var addr1 in addresses)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                // Check if an other server has already checked this address
                var check = await (from a in db.Buyoutsmartcontractaddresses
                    where a.Id==addr1.Id && (a.State == "active" || a.State == "payment_received")
                    select a).AsNoTracking().FirstOrDefaultAsync(cancellationToken: cancellationToken);
                if (check==null)
                    continue;

                BuildTransactionClass bt = new BuildTransactionClass();
                bt.LogFile += $"Check BuyOutSmartcontract Address{Environment.NewLine}";
                bt.LogFile += addr1.Address + Environment.NewLine;
                
                // Get Utxo
                var utxo = await ConsoleCommand.GetNewUtxoAsync(addr1.Address);
                long lovelace = utxo.LovelaceSummary;

                // Check for expired
                if (addr1.Expiredate < DateTime.Now && lovelace==0)
                {
                    addr1.State = "expired";
                    continue;
                }

                // Check for lovelace
                if (lovelace == 0)
                    continue;


                var txhash = utxo.GetFirstTxHash();
                var txid = string.IsNullOrEmpty(txhash)
                    ? await ConsoleCommand.GetTransactionIdAsync(addr1.Address)
                    : txhash;

                var senderaddress = await ConsoleCommand.GetSenderAsync(txid);
                if (string.IsNullOrEmpty(senderaddress))
                    continue;

                addr1.State = "inprogress";
                addr1.Receiveraddress = senderaddress;
                await db.SaveChangesAsync(cancellationToken);

                // Check correct amount
                if (lovelace != addr1.Lovelace + addr1.Additionalamount)
                {
                    await SendBackAsync(db, redis, mainnet, addr1, txhash, "Wrong amount", utxo, senderaddress, "refunded");
                    continue;
                }

                // Check for tokens
                if (utxo.TokensSum != 0)
                {
                    await SendBackAsync(db, redis, mainnet, addr1, txhash, "Please no tokens", utxo, senderaddress, "refunded");
                    continue;
                }

                BuyerClass buyer = new BuyerClass()
                {
                    Buyer = new TransactionAddressClass()
                    {
                        ChangeAddress = senderaddress,
                        Pkh = GlobalFunctions.GetPkhFromAddress(addr1.Address),
                        Addresses = new[] {new AddressTxInClass() {Address = addr1.Address, Utxo = utxo.TxIn}}
                    }
                };

                var result=await ApiFunctions.BuyDirectSaleAsync(addr1.Transactionid,buyer);
                if (result == null || result.State!=PaymentTransactionsStates.active || result.PaymentTransactionSubStateResult.PaymentTransactionSubstate!=PaymentTransactionSubstates.readytosignbybuyer)
                {
                    await SendBackAsync(db,redis,mainnet, addr1, txhash, "Smartcontract buy was not successful", utxo, senderaddress, "refunded");
                    continue;
                }

                string guid = GlobalFunctions.GetGuid();
                string matxrawfile = $"{GeneralConfigurationClass.TempFilePath}matx{guid}.raw";
                string matxsignedfile = $"{GeneralConfigurationClass.TempFilePath}matx{guid}.signed";
                string paymentskeyfile = $"{GeneralConfigurationClass.TempFilePath}payment{guid}.skey";
                string password = addr1.Salt + GeneralConfigurationClass.Masterpassword;

                MatxRawClass mrc = new() { CborHex = result.Cbor, Description = "", Type = "Unwitnessed Tx BabbageEra" };
                await File.WriteAllTextAsync(matxrawfile, JsonConvert.SerializeObject(mrc), cancellationToken);
                await File.WriteAllTextAsync(paymentskeyfile, Encryption.DecryptString(addr1.Skey, password), cancellationToken);

                var sign=ConsoleCommand.SignTransaction(new string[] {paymentskeyfile}, "", matxrawfile, matxsignedfile, mainnet,
                    ref bt);
                if (!sign)
                {
                    await SendBackAsync(db,redis,mainnet, addr1, txhash, "Smartcontract signing was not successful", utxo, senderaddress, "refunded");
                    continue;
                }

                if (!File.Exists(matxsignedfile))
                {
                    await SendBackAsync(db,redis,mainnet, addr1, txhash, "Smartcontract signing was not successful", utxo, senderaddress, "refunded");
                    continue;
                }

                MatxRawClass mrcSigned=JsonConvert.DeserializeObject<MatxRawClass>(await File.ReadAllTextAsync(matxsignedfile, cancellationToken));

                var submit=await ApiFunctions.SubmitTransactionAsync(addr1.Transactionid,result.SignGuid, mrcSigned.CborHex);
                if (submit==null || submit.State!=PaymentTransactionsStates.finished || submit.PaymentTransactionSubStateResult.PaymentTransactionSubstate!=PaymentTransactionSubstates.sold)
                {
                    await SendBackAsync(db,redis,mainnet, addr1, txhash, "Smartcontract submit was not successful", utxo, senderaddress, "refunded");
                    continue;
                }


                // TODO: Insert into Transactions
                addr1.State="finished";
                await db.SaveChangesAsync(cancellationToken);
                /*
                if (!string.IsNullOrEmpty(addr1.Transactionid))
                {
                    var paymenttransaction = await (from a in db.Preparedpaymenttransactions
                        where a.Transactionuid == addr1.Transactionid
                        select a).FirstOrDefaultAsync(cancellationToken: cancellationToken);

                    if (paymenttransaction != null)
                    {
                        paymenttransaction.IsActive = PaymentTransactionsStates.finished.ToString();
                        paymenttransaction.Smartcontractstate = PaymentTransactionSubstates.sold.ToString();
                        paymenttransaction.Buyeraddress = senderaddress;
                        await db.SaveChangesAsync(cancellationToken);
                    }
                }*/
            }




            // Reset the Display for the Admintool
            await StaticBackgroundServerClass.UpdateActualRunnningTaskAsync(BackgroundTaskEnums.none, mainnet, serverid,
                redis);
        }

        private async Task SendBackAsync(EasynftprojectsContext db, IConnectionMultiplexer redis, bool mainnet,
            Buyoutsmartcontractaddress addr, string txhash, string sendbackmessage, TxInAddressesClass utxo,
            string senderaddress, string newaddrstate)
        {
            await StaticBackgroundServerClass.LogAsync(db,
                $"Sending back all ADA and Tokens from address {addr.Address} to address {senderaddress}", "");
            BuildTransactionClass buildtransaction = new BuildTransactionClass();

            string password = "";
            if (!string.IsNullOrEmpty(addr.Salt))
            {
                password = addr.Salt + GeneralConfigurationClass.Masterpassword;
            }

            var s = CardanoSharpFunctions.SendAllAdaAndTokens(db,redis, addr.Address, addr.Skey,addr.Vkey, password, senderaddress,
                mainnet, ref buildtransaction,txhash,0,0, sendbackmessage);

            // TODO: Save refund log

            /*    try
                {
                    await GlobalFunctions.SaveRefundLogAsync(db, addr.Address, senderaddress, txhash,
                        s == "OK", buildtransaction.TxHash,
                        sendbackmessage, (int)addr.NftprojectId,
                        buildtransaction.LogFile, lovelace, buildtransaction.Fees);
                }
                catch
                {
                }*/


            addr.State = newaddrstate;
            addr.Outgoingtxhash=buildtransaction.TxHash;
            addr.Logfile=buildtransaction.LogFile;
            await db.SaveChangesAsync();
        }
    }
}
