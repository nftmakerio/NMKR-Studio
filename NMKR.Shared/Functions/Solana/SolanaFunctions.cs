using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using NMKR.Shared.Blockchains;
using NMKR.Shared.Classes;
using NMKR.Shared.Classes.Blockfrost;
using NMKR.Shared.Classes.Helius;
using NMKR.Shared.Classes.Koios;
using NMKR.Shared.Classes.Solana;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions.Metadata;
using NMKR.Shared.Functions.Solana.Helios;
using NMKR.Shared.Model;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Solnet.Metaplex.NFT.Library;
using Solnet.Metaplex.Utilities;
using Solnet.Programs;
using Solnet.Rpc;
using Solnet.Rpc.Builders;
using Solnet.Rpc.Core.Http;
using Solnet.Rpc.Messages;
using Solnet.Rpc.Models;
using Solnet.Rpc.Types;
using Solnet.SDK.Nft;
using Solnet.Wallet;
using Solnet.Wallet.Bip39;
using Solnet.Wallet.Utilities;
using StackExchange.Redis;
using Collection = Solnet.Metaplex.NFT.Library.Collection;
using Creator = Solnet.Metaplex.NFT.Library.Creator;
using File = NMKR.Shared.Classes.Solana.File;
using Properties = NMKR.Shared.Classes.Solana.Properties;
using Transaction = Solnet.Rpc.Models.Transaction;

namespace NMKR.Shared.Functions.Solana
{
    public static class SolanaFunctions
    {
        public static SolanaKeysClass ConvertToSolanaKeysClass(Adminmintandsendaddress address)
        {
            string payskey = Encryption.DecryptString(address.Privateskey,
                GeneralConfigurationClass.Masterpassword + address.Salt);
            return new SolanaKeysClass() { Address = address.Address, PublicKey = address.Address, SecretKey = payskey };
        }
        public static SolanaKeysClass ConvertToSolanaKeysClass(Nftproject project)
        {
            var wallet=GetWallet(project);
              return new SolanaKeysClass() { Address = wallet.Account.PublicKey, PublicKey = wallet.Account.PublicKey, SecretKey = wallet.Account.PrivateKey };
        }
        public static SolanaKeysClass ConvertToSolanaKeysClass(Burnigendpoint address)
        {
            string payskey = Encryption.DecryptString(address.Privateskey,
                address.Salt+GeneralConfigurationClass.Masterpassword );
            return new SolanaKeysClass() { Address = address.Address, PublicKey = address.Address, SecretKey = payskey };
        }
        public static ulong GetWalletBalance(string walletAddress)
        {
            var res = Task.Run(async () => await GetWalletBalanceAsync(walletAddress));
            return res.Result;
        }
        public static async Task<ulong> GetWalletBalanceAsync(string walletAddress)
        {
            if (string.IsNullOrEmpty(walletAddress))
                return 0;

            var rpcClient = GetRpcClient();
            var balance = await rpcClient.GetBalanceAsync(walletAddress);

            return balance?.Result?.Value ?? (ulong) 0;
        }

        private static IRpcClient GetRpcClient()
        {
            IRpcClient rpcClient =
                ClientFactory.GetClient(
                    $"{GeneralConfigurationClass.HeliosConfiguration.ApiUrl}/?api-key={GeneralConfigurationClass.HeliosConfiguration.ApiKey}",
                    null, null, null);
         //   IRpcClient rpcclient = ClientFactory.GetClient(Cluster.DevNet);
            return rpcClient;
        }

        public static CreateNewPaymentAddressClass CreateNewWallet()
        {
            var wallet = new Wallet(WordCount.TwentyFour, WordList.English);

            CreateNewPaymentAddressClass cn=new CreateNewPaymentAddressClass()
            {
                Address = wallet.Account.PublicKey.Key,
                privatevkey = wallet.Account.PrivateKey.Key,
                privateskey = wallet.Account.PrivateKey.Key,
                SeedPhrase = wallet.Mnemonic.ToString(),
                ErrorCode = 0, Blockchain = Blockchain.Solana
            };
            return cn;
        }



    

        public static async Task<CreateCollectionResultClass> CreateVerifiedCollectionAsync(EasynftprojectsContext db, SolanaCollectionClass collection)
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("POST"), $"{GeneralConfigurationClass.SolanaApiUrl}/createSolanaCollectionx");
            string st = JsonConvert.SerializeObject(collection, Formatting.Indented);

            request.Content = new StringContent(st);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<CreateCollectionResultClass>(body);
            }

            await GlobalFunctions.LogMessageAsync(db, "Error while creating solana collection",
                JsonConvert.SerializeObject(collection));
            return null;
        }

      

        /// <summary>
        /// Get the balance of NFTS of a wallet
        /// </summary>
        /// <param name="walletAddress"></param>
        /// <returns></returns>
        public static GetAssetsByOwnerResultClass GetAssetsByOwner(string walletAddress)
        {
            var res = Task.Run(async () => await GetAssetsByOwnerAsync(walletAddress));
            return res.Result;
        }
        public static async Task<GetAssetsByOwnerResultClass> GetAssetsByOwnerAsync(string walletAddress)
        {
            GetAssetsByOwnerClass getAssetsByOwnerClass = new GetAssetsByOwnerClass()
            {
                Jsonrpc = "2.0",
                Id = "my-id",
                Method = "getAssetsByOwner",
                Params = new Params()
                {
                    OwnerAddress = walletAddress,
                    Page = 1,
                    Limit = 1000,
                    DisplayOptions = new DisplayOptions()
                    {
                        ShowFungible = true
                    }
                }
            };

            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{GeneralConfigurationClass.HeliosConfiguration.ApiUrl}/?api-key={GeneralConfigurationClass.HeliosConfiguration.ApiKey}"),
                Content = new StringContent(JsonConvert.SerializeObject(getAssetsByOwnerClass))
                {
                    Headers =
                    {
                        ContentType = new MediaTypeHeaderValue("application/json")
                    }
                }
            };
            using var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode == false)
                return null;
            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<GetAssetsByOwnerResultClass>(body);

        }

        /// <summary>
        /// Removes duplicates from a list of signatures
        /// </summary>
        /// <param name="signatures"></param>
        /// <param name="allowEmptySignatures"></param>
        /// <returns></returns>
        private static List<SignaturePubKeyPair> DeduplicateTransactionSignatures(
            List<SignaturePubKeyPair> signatures, bool allowEmptySignatures = false)
        {
            var signaturesList = new List<SignaturePubKeyPair>();
            var signaturesSet = new HashSet<PublicKey>();
            var emptySgn = new byte[64];
            foreach (var sgn in signatures)
            {
                if (sgn.Signature.SequenceEqual(emptySgn) && !allowEmptySignatures)
                {
                    var notEmptySig = signatures.FirstOrDefault(
                        s => s.PublicKey.Equals(sgn.PublicKey) && !s.Signature.SequenceEqual(emptySgn));
                    if (notEmptySig != null && !signaturesSet.Contains(notEmptySig.PublicKey))
                    {
                        signaturesSet.Add(notEmptySig.PublicKey);
                        signaturesList.Add(notEmptySig);
                    }
                }
                if ((sgn.Signature.SequenceEqual(emptySgn) && !allowEmptySignatures) || signaturesSet.Contains(sgn.PublicKey)) continue;
                signaturesSet.Add(sgn.PublicKey);
                signaturesList.Add(sgn);
            }
            return signaturesList;
        }

        /// <summary>
        /// Signs a transaction
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="account"></param>
        /// <returns></returns>
        private static Task<Transaction> _SignTransaction(Transaction transaction, Account account)
        {
            transaction.Sign(account);
            return Task.FromResult(transaction);
        }
        /// <summary>
        /// Signs a transaction
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="account"></param>
        /// <returns></returns>
        private static async Task<Transaction> SignTransaction(Transaction transaction, Account account)
        {
            transaction.Sign(account);
            transaction.Signatures = DeduplicateTransactionSignatures(transaction.Signatures, allowEmptySignatures: true);
            var tx = await _SignTransaction(transaction, account);
            tx.Signatures = DeduplicateTransactionSignatures(tx.Signatures);
            return tx;
        }
        /// <summary>
        /// Signs and sends a transaction
        /// </summary>
        /// <param name="rpcClient"></param>
        /// <param name="transaction"></param>
        /// <param name="account"></param>
        /// <param name="skipPreflight"></param>
        /// <param name="commitment"></param>
        /// <returns></returns>
        private static async Task<BuildTransactionClass> SignAndSendTransaction
        (
            IRpcClient rpcClient,
            Transaction transaction,
            Account signAccount,
            BuildTransactionClass bt,
            bool skipPreflight = false,
            Commitment commitment = Commitment.Confirmed)
        {
            var signedTransaction = await SignTransaction(transaction, signAccount);
            var ser = signedTransaction.Serialize();
            var base64 = Convert.ToBase64String(ser);


            var i = 0;
            while (i<5)
            {
                i++;
                var res = await rpcClient.SendTransactionAsync(base64,
                    skipPreflight: skipPreflight, preFlightCommitment: commitment);
                await Task.Delay(500);

              bt.ErrorMessage = res.Reason;
              bt.TxHash ??= "";
              if (!string.IsNullOrEmpty(bt.TxHash) && res.Reason=="OK")
                  bt.TxHash += "|";
              if (res.Reason == "OK")
              {
                  bt.TxHash += res.Result;
                  break;
              }
              else
              {
                  bt.Log($"Try {i} - Error: " + res.Reason);
              }
            }

            return bt;
        }

/*
        public static async Task CloseAccountAndTransferAll(IRpcClient rpcClient, Account sourceAccount, PublicKey destinationPublicKey)
        {
            // Erstelle eine neue Transaktion
            var transaction = new TransactionBuilder()
                .SetRecentBlockHash((await rpcClient.GetRecentBlockHashAsync()).Result.Value.Blockhash)
                .SetFeePayer(sourceAccount.PublicKey);

            // Füge eine Transfer-Anweisung hinzu, um alle Lamports zu übertragen
            transaction.AddInstruction(SystemProgram.Transfer(sourceAccount.PublicKey, destinationPublicKey, sourceAccount.Balance));

            // Füge eine CloseAccount-Anweisung hinzu, um das Konto zu schließen
            transaction.AddInstruction(SystemProgram.CloseAccount(sourceAccount.PublicKey, destinationPublicKey, sourceAccount.PublicKey));

            // Signiere und sende die Transaktion
            var tx = transaction.Build(new List<Account> { sourceAccount });
            var serializedTx = tx.Serialize();
            var base64Tx = Convert.ToBase64String(serializedTx);
            var result = await rpcClient.SendTransactionAsync(base64Tx);
        }
*/
        /// <summary>
        /// Transfers an NFT to a destination
        /// </summary>
        /// <param name="activeRpcClient"></param>
        /// <param name="addressaccount"></param>
        /// <param name="destination"></param>
        /// <param name="tokenMint"></param>
        /// <param name="amount"></param>
        /// <param name="blockHash"></param>
        /// <param name="commitment"></param>
        /// <returns></returns>
        public static async Task<string> TransferNft(
            IRpcClient activeRpcClient,
            Account addressaccount,
            Solnet.Wallet.PublicKey destination,
            Solnet.Wallet.PublicKey tokenMint,
            ulong amount,
            LatestBlockHash blockHash,
            Commitment commitment = Commitment.Confirmed)
        {
            var sta = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(
                addressaccount.PublicKey,
                tokenMint);
            var ata = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(destination, tokenMint);
            var tokenAccounts = await activeRpcClient.GetTokenAccountsByOwnerAsync(destination, tokenMint, null);
            var transaction = new Transaction
            {
                RecentBlockHash = await GetBlockHash(activeRpcClient),
                FeePayer = addressaccount.PublicKey,
                Instructions = new List<TransactionInstruction>(),
                Signatures = new List<SignaturePubKeyPair>()
            };
            if (tokenAccounts.Result == null || tokenAccounts.Result.Value.Count == 0)
            {
                transaction.Instructions.Add(
                    AssociatedTokenAccountProgram.CreateAssociatedTokenAccount(
                        addressaccount,
                        destination,
                        tokenMint));
            }
            transaction.Instructions.Add(
                TokenProgram.Transfer(
                    sta,
                    ata,
                    amount,
                    addressaccount
                ));
            return (await SignAndSendTransaction(activeRpcClient, transaction, addressaccount, new BuildTransactionClass())).TxHash;
        }
        private static readonly IDictionary<string, (DateTime, string)> _commitmentCache = new Dictionary<string, (DateTime, string)>();
        
        /// <summary>
        /// Get the latest block hash
        /// </summary>
        /// <param name="activeRpcClient"></param>
        /// <param name="commitment"></param>
        /// <param name="useCache"></param>
        /// <param name="maxSeconds"></param>
        /// <returns></returns>
        public static async Task<string> GetBlockHash(
            IRpcClient activeRpcClient,
            Commitment commitment = Commitment.Confirmed,
            bool useCache = true,
            int maxSeconds = 0)
        {
            if (activeRpcClient == null) return null;
            var exists = _commitmentCache.TryGetValue(commitment.ToString(), out var cacheEntry);
            if (useCache && maxSeconds > 0)
            {
                switch (exists)
                {
                    case true when (DateTime.Now - cacheEntry.Item1).TotalSeconds < maxSeconds:
                        return cacheEntry.Item2;
                    case true:
                        _commitmentCache.Remove(commitment.ToString());
                        exists = false;
                        break;
                }
            }
            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            await GlobalFunctions.LogMessageAsync(db, "GetBlockHash", "GetBlockHash", 0);

            var blockhash = (await activeRpcClient.GetLatestBlockHashAsync(commitment)).Result?.Value?.Blockhash;
            if (exists) _commitmentCache.Remove(commitment.ToString());
            if (blockhash != null && useCache) _commitmentCache.Add(commitment.ToString(), (DateTime.Now, blockhash));
            return blockhash;
        }

        public static async Task<string> CloseAccount(IRpcClient activeRpcClient, PublicKey accountToClose, Account localWallet)
        {
            var closeInstruction = TokenProgram.CloseAccount(
                accountToClose,
                localWallet.PublicKey,
                localWallet.PublicKey,
                TokenProgram.ProgramIdKey);
            var blockHash = await activeRpcClient.GetRecentBlockHashAsync();

            var signers = new List<Account> { localWallet };
            var transactionBuilder = new TransactionBuilder()
                .SetRecentBlockHash(blockHash.Result.Value.Blockhash)
                .SetFeePayer(localWallet)
                .AddInstruction(closeInstruction);

            byte[] transaction = transactionBuilder.Build(signers);
            Transaction deserializedTransaction = Transaction.Deserialize(transaction);
            var res=await SignAndSendTransaction(activeRpcClient,deserializedTransaction, localWallet, new BuildTransactionClass());
            /*
            if (!transactionSignature.WasSuccessful)
            {
                LoggingService
                    .Log("Mint was not successfull: " + transactionSignature.Reason, true);
            }
            else
            {
                ServiceFactory.Resolve<TransactionService>().CheckSignatureStatus(transactionSignature.Result,
                    success =>
                    {
                        if (success)
                        {
                            LoggingService.Log("Mint Successfull! Woop woop!", true);
                        }
                        else
                        {
                            LoggingService.Log("Mint failed!", true);
                        }
                        MessageRouter.RaiseMessage(new NftMintFinishedMessage());
                    });
            }
            */
            return res.TxHash;
        }


        private static async Task<TokenAccount[]> GetTokenAccounts(IRpcClient activeRpcClient, PublicKey publicKey, Commitment commitment = Commitment.Confirmed)
        {
            var rpc = activeRpcClient;
            var result = await
                rpc.GetTokenAccountsByOwnerAsync(
                    publicKey,
                    null,
                    TokenProgram.ProgramIdKey,
                    commitment);
            return result.Result?.Value?.ToArray();
        }

        public static async Task GeTokensAsync(IRpcClient client, Account ownerAccount)
        {
            var tokens = await GetTokenAccounts(client, ownerAccount.PublicKey, Commitment.Confirmed);

            if (tokens is { Length: > 0 })
            {
                var tokenAccounts = tokens.OrderByDescending(
                    tk => tk.Account.Data.Parsed.Info.TokenAmount.AmountUlong);
                foreach (var item in tokenAccounts)
                {
                    if (!(item.Account.Data.Parsed.Info.TokenAmount.AmountUlong > 0)) break;
                    Console.WriteLine(item.Account.Data.Parsed.Info.TokenAmount.AmountUlong + " x " +
                                      item.Account.Data.Parsed.Info.Mint + " " +
                                      item.Account.Data.Parsed.Info.Delegate + " " +
                                      item.Account.Data.Program + " " +
                                      item.Account.Data.Parsed.Info.Owner + " " +
                                      item.Account.Data.Parsed.Info.State + " " +
                                      item.Account.Data.Parsed.Info.IsNative);

                    var loadTask = await SolanaNft.TryGetNftData(item.Account.Data.Parsed.Info.Mint,
                        client);

                    if (loadTask == null)
                        continue;

                    Console.WriteLine("Offchain Data: " + loadTask.metaplexData.data.offchainData?.collection?.name + " " +
                                      loadTask.metaplexData.data.offchainData?.collection?.family + " " +
                                      loadTask.metaplexData.data.offchainData?.name + " " +
                                      loadTask.metaplexData.data.offchainData?.description + " " +
                                      loadTask.metaplexData.data.offchainData?.symbol + " " +
                                      loadTask.metaplexData.data.offchainData?.default_image);

                }
            }
        }

        public enum CreateMasterEditionVersion
        {
            V1,
            V3,
        }

        public static TransactionInstruction CreateMasterEdition(
            ulong? maxSupply,
            PublicKey masterEditionKey,
            PublicKey mintKey,
            PublicKey updateAuthorityKey,
            PublicKey mintAuthority,
            PublicKey payer,
            PublicKey metadataKey,
            CreateMasterEditionVersion version = CreateMasterEditionVersion.V1)
        {
            List<AccountMeta> accountMetaList = new List<AccountMeta>()
        {
            AccountMeta.Writable(masterEditionKey, false),
            AccountMeta.Writable(mintKey, false),
            AccountMeta.ReadOnly(updateAuthorityKey, true),
            AccountMeta.ReadOnly(mintAuthority, true),
            AccountMeta.ReadOnly(payer, true),
            AccountMeta.ReadOnly(metadataKey, false),
            AccountMeta.ReadOnly(TokenProgram.ProgramIdKey, false),
            AccountMeta.ReadOnly(SystemProgram.ProgramIdKey, false),
            AccountMeta.ReadOnly(SysVars.RentKey, false)
        };
            return new TransactionInstruction()
            {
                ProgramId = MetadataProgram.ProgramIdKey.KeyBytes,
                Keys = (IList<AccountMeta>)accountMetaList,
                Data = EncodeCreateMasterEdition(maxSupply, version)
            };
        }

        public static byte[] EncodeCreateMasterEdition(
            ulong? maxSupply,
            CreateMasterEditionVersion version)
        {
            MemoryStream output = new MemoryStream();
            BinaryWriter binaryWriter = new BinaryWriter((Stream)output);
            switch (version)
            {
                case CreateMasterEditionVersion.V1:
                    binaryWriter.Write((byte)10);
                    break;
                case CreateMasterEditionVersion.V3:
                    binaryWriter.Write((byte)17);
                    break;
            }
            if (!maxSupply.HasValue)
            {
                binaryWriter.Write(new byte[1]);
            }
            else
            {
                binaryWriter.Write((byte)1);
                binaryWriter.Write(maxSupply.Value);
            }
            return output.ToArray();
        }

        /// <summary>
        /// Mints a NFT
        /// </summary>
        /// <param name="db"></param>
        /// <param name="address"></param>
        /// <param name="receiveraddress"></param>
        /// <returns></returns>
        public static async Task<BuildTransactionClass> MintFromNftAddressAsync(ulong lamportsOnAddress, Nftaddress address, string receiveraddress, Pricelistdiscount discount, BuildTransactionClass bt)
        {
            var rpcClient = GetRpcClient();
            var projectWallet=GetWallet(address.Nftproject);
            var addressWallet = GetWallet(address);

            Account addressAccount = addressWallet.Account;
            PublicKey receiverpublickey = new PublicKey(receiveraddress);

            

            // Prepare the transaction
            var minimumRentMintAccountDataSize = (await rpcClient.GetMinimumBalanceForRentExemptionAsync(TokenProgram.MintAccountDataSize)).Result;
            var minimumRentTokenAccountDataSize = (await rpcClient.GetMinimumBalanceForRentExemptionAsync(TokenProgram.TokenAccountDataSize)).Result;

            var lamports = lamportsOnAddress;
            int i = -1;
            foreach (var nfttonftaddress in address.Nfttonftaddresses)
            {
                if (lamports == 0)
                    break;

                i++;
                var nftAccount = projectWallet.GetAccount(nfttonftaddress.NftId);

                int tries = 0;
                do
                {
                    tries++;
                    if (tries > 5)
                        break;

                    // Then build a new transaction with the fees
                    bt.SolanaTransaction = await BuildTransactionMintFromNftaddress(rpcClient, address, lamports, nfttonftaddress.Nft, 
                        addressAccount, nftAccount,
                        minimumRentMintAccountDataSize, minimumRentTokenAccountDataSize, 5000,
                        receiverpublickey, i >= (address.Nfttonftaddresses.Count - 1),discount, bt);
                    var signers = new List<Account> {addressAccount, nftAccount };
                    var tx = Transaction.Deserialize(bt.SolanaTransaction.Build(signers));


                    // Sign and Send the transaction
                    if (tx != null)
                    {
                      /*  var priofees = await GetPriorityFeeEstimate("High", tx);
                        if (priofees != 0)
                            tx.Add(ComputeBudgetProgram.SetComputeUnitPrice(priofees));
                      */


                        var res = await SignAndSendTransaction(rpcClient, tx, addressAccount, bt, false,Commitment.Finalized);
                        await Task.Delay(500);
                        // Check Confirmation
                        if (!string.IsNullOrEmpty(res.TxHash))
                        {
                            bt.SolanaTransactionMetaSlotInfo = (await rpcClient.GetTransactionAsync(res.TxHash, Commitment.Finalized));
                            lamports = bt.NewCalculatedLamports;
                            break;
                        }
                        // Try again
                        await Task.Delay(1000);
                    }
                    else
                    {
                        // Transaction could not created - so break
                        break;
                    }
                } while (true);
            }

            return bt;

        }


        public static async Task<BuildTransactionClass> MintFromNftAddressCoreAsync(ulong lamportsOnAddress, Nftaddress address, string receiveraddress, Pricelistdiscount discount, Nftprojectsadditionalpayout[] additionalPayoutWallets, BuildTransactionClass bt)
        {
            var addressWallet = GetWallet(address);
            // First send the nfts
            foreach (var nfttonftaddress in address.Nfttonftaddresses)
            {
                int i = 0;
                do
                {
                    i++;
                    bt = await MintAndSendCoreAsync(nfttonftaddress.Nft, nfttonftaddress.Nft.Nftproject,
                        receiveraddress, addressWallet, bt);
                    if (bt.LastTransaction == "OK")
                        break;
                    await Task.Delay(1000);
                    if (i == 4)
                        break;
                } while (true);

                if (bt.LastTransaction != "OK")
                    break;
            }

            if (bt.LastTransaction != "OK")
            {
                bt.TxHash = ""; // This transaction will be marked as error
                return bt;
            }

            List<TxOutClass> txouts = new List<TxOutClass>();
            // Then calculate the discounts and send them back
            if (discount != null)
            {
                ulong discountInLamport = (ulong)(lamportsOnAddress * discount.Sendbackdiscount / 100);
                bt.Discount = (long)discountInLamport;
                txouts.Add(
                    new TxOutClass()
                    {
                        Amount = (long) discountInLamport,
                        ReceiverAddress = addressWallet.Account.PublicKey.Key,
                    }
                );
            }

            // Then send the mintingfees
            var mintingcosts = GlobalFunctions.GetMintingcosts2((int)address.NftprojectId, address.Nfttonftaddresses.Count,
                address.Price ?? 0);
            if (mintingcosts.CostsSolana != 0)
            {
                bt.MintingcostsTxOut = new TxOutClass()
                {
                    Amount = mintingcosts.CostsSolana,
                    ReceiverAddress = mintingcosts.MintingcostsreceiverSolana
                };
                txouts.Add(bt.MintingcostsTxOut);
            }

            // And finally send the rest to the seller
            var lamports = await GetWalletBalanceAsync(address.Address);
            ulong addvaluesum = 0;
            // Calculate the additional payout wallets

            bt.AdditionalPayouts = additionalPayoutWallets;
            foreach (var nftprojectsadditionalpayout in additionalPayoutWallets.OrEmptyIfNull())
            {
                ulong addvalue = GetAdditionalPayoutwalletsValueSolana(nftprojectsadditionalpayout,
                    lamports - (ulong)(bt.Discount ?? 0), address.Nfttonftaddresses.Count);
                if (addvalue <= 0) continue;

                var addval = new TxOutClass()
                {
                    Amount = (long)addvalue,
                    ReceiverAddress = nftprojectsadditionalpayout.Wallet.Walletaddress
                };
                txouts.Add(addval);

                nftprojectsadditionalpayout.Valuetotal = (long)addvalue;
                addvaluesum += addvalue;
            }



            ulong rest = lamports - (ulong)txouts.Sum(x => x.Amount);
            if (rest > 0)
            {
                bt.ProjectTxOut= new TxOutClass()
                {
                    Amount = (long)rest,
                    ReceiverAddress = address.Nftproject.Solanacustomerwallet.Walletaddress
                };
                txouts.Add(bt.ProjectTxOut);
            }

            if (txouts.Any())
            {
                bt=await SendSolAsync(addressWallet, txouts.ToArray(), bt);
            }
            else
            {
                bt.TxHash = bt.MintAssetAddress.FirstOrDefault()?.TxHash;
            }

            // Set the project txout also with the additional wallets - to display the correct value in the transactions
            bt.ProjectTxOut.Amount += (long)addvaluesum;
            bt.Fees = (long) (lamportsOnAddress - lamports);

            return bt;

        }
        public static ulong GetAdditionalPayoutwalletsValueSolana(Nftprojectsadditionalpayout nftprojectsadditionalpayout,
            ulong hastopay, long nftcount)
        {
            if (nftprojectsadditionalpayout.Valuetotal != null)
                return (ulong)nftprojectsadditionalpayout.Valuetotal * (ulong)nftcount;

            if (nftprojectsadditionalpayout.Valuepercent == null ||
                !(nftprojectsadditionalpayout.Valuepercent > 0)) return 0;
            var v = hastopay / 100 * nftprojectsadditionalpayout.Valuepercent;
            long v1 = Convert.ToInt64(v);
            v1 = Math.Max(1, v1);
            return (ulong)v1;
        }

        public static async Task<SolanaVerifiyCollectionResultClass> AddToSolanaCollectionAsync(EasynftprojectsContext db, string mintAddress, string collectionNft, Adminmintandsendaddress paywallet, SolanaKeysClass updateAuthority)
        {

            await GlobalFunctions.LogMessageAsync(db, "AddToSolanaCollectionAsync", "AddToSolanaCollectionAsync", 0);

            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("POST"), $"{GeneralConfigurationClass.SolanaApiUrl}/verifySolanaCollectionx");

            if (string.IsNullOrEmpty(GeneralConfigurationClass.SolanaApiUrl))
            {
                await GlobalFunctions.LogExceptionAsync(null, "Solana Api Url is not defined", "");
                return null;
            }

            SolanaVerifyCollectionClass ver = new SolanaVerifyCollectionClass()
            {
                CollectionAddress = collectionNft, NftMintAddress = mintAddress, 
                Payer = ConvertToSolanaKeysClass(paywallet), UpdateAuthority = updateAuthority
            };

            string st = JsonConvert.SerializeObject(ver, Formatting.Indented);

            request.Content = new StringContent(st);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<SolanaVerifiyCollectionResultClass>(body);
            }

            await GlobalFunctions.LogMessageAsync(db, "Error while adding nft to solana collection",
                mintAddress + Environment.NewLine + collectionNft + Environment.NewLine + JsonConvert.SerializeObject(updateAuthority));

            return null;
        }

        private static async Task<TransactionBuilder> BuildTransactionMintFromNftaddress(IRpcClient rpcClient, Nftaddress address,ulong lamports, Nft nft, 
            Account addressAccount,Account nftAccount, ulong minimumRentMintAccountDataSize, ulong minimumRentTokenAccountDataSize,ulong costssignature, 
            PublicKey receiverpublickey, bool sendresttoseller,Pricelistdiscount discount, BuildTransactionClass bt)
        {

            ulong fees = minimumRentMintAccountDataSize + (costssignature * 2);


            var metadata = new global::Solnet.Metaplex.NFT.Library.Metadata()
            {
                name = nft.Name,
                symbol = address.Nftproject.Solanasymbol??"TEST",
                uri = await GetNftOffchainMetadataLink(nft, address.Nftproject),
                sellerFeeBasisPoints = (uint)(address.Nftproject.SellerFeeBasisPoints??0),
                creators = new List<Creator> { new(nftAccount.PublicKey, 100, true) }
            };

            // Set Collection
            if (!string.IsNullOrEmpty(address.Nftproject.Solanacollectiontransaction))
            {
                PublicKey collectionkey = new PublicKey(address.Nftproject.Solanacollectiontransaction);
                metadata.collection = new Collection(collectionkey, false);
            }
            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            await GlobalFunctions.LogMessageAsync(db, "BuildTransactionMintFromNftaddress", "BuildTransactionMintFromNftaddress", 0);
            var blockHash = await rpcClient.GetLatestBlockHashAsync();
            var transaction = new TransactionBuilder()
                .SetRecentBlockHash(blockHash.Result.Value.Blockhash)
                .SetFeePayer(addressAccount)
                .AddInstruction(ComputeBudgetProgram.SetComputeUnitLimit(30000))
                .AddInstruction(ComputeBudgetProgram.SetComputeUnitPrice(1000000));

                fees += 2853600; // Token Metadata Program
                fees += 15616720; // Token Metadata Program

                var associatedTokenAccount = AssociatedTokenAccountProgram
                    .DeriveAssociatedTokenAccount(nftAccount.PublicKey, nftAccount.PublicKey);

                transaction.AddInstruction(
                        SystemProgram.CreateAccount(
                            addressAccount,
                            nftAccount.PublicKey,
                            minimumRentMintAccountDataSize,
                            TokenProgram.MintAccountDataSize,
                            TokenProgram.ProgramIdKey))
                    .AddInstruction(
                        TokenProgram.InitializeMint(
                            nftAccount.PublicKey,
                            0,
                            nftAccount,
                            nftAccount))
                    .AddInstruction(
                        AssociatedTokenAccountProgram.CreateAssociatedTokenAccount(
                            addressAccount,
                            nftAccount,
                            nftAccount.PublicKey))
                    .AddInstruction(
                        TokenProgram.MintTo(
                            nftAccount.PublicKey,
                            associatedTokenAccount,
                            1,
                            nftAccount))
                    .AddInstruction(MetadataProgram.CreateMetadataAccount(
                        PDALookup.FindMetadataPDA(nftAccount),
                        nftAccount.PublicKey,
                        nftAccount,
                        addressAccount,
                        nftAccount.PublicKey,
                        metadata,
                        TokenStandard.NonFungible,
                        true,
                        true,
                        null,
                        metadataVersion: MetadataVersion.V3));

                // Transfer the Token to the Buyer
                var sta = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(
                    nftAccount.PublicKey,
                    nftAccount);
                var ata = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(receiverpublickey, nftAccount);


                
                transaction = transaction
                    .AddInstruction(
                        CreateMasterEdition(
                            maxSupply: null,
                            masterEditionKey: PDALookup.FindMasterEditionPDA(nftAccount),
                            mintKey: nftAccount,
                            updateAuthorityKey: nftAccount,
                            mintAuthority: nftAccount,
                            payer: addressAccount,
                            metadataKey: PDALookup.FindMetadataPDA(nftAccount),
                            version: CreateMasterEditionVersion.V3
                        ))
                    .AddInstruction( // This is needed for the transfer to the receiver
                        AssociatedTokenAccountProgram.CreateAssociatedTokenAccount(
                            addressAccount,
                            receiverpublickey,
                            nftAccount))
                    .AddInstruction( // This is the transfer of the nft to the receiver
                        TokenProgram.Transfer(
                            sta,
                            ata,
                            1,
                            nftAccount
                        ));

                bt.TokenSource = sta;
                bt.TokenDestination = ata;
                bt.ProjectAccount = nftAccount;
                bt.MasterEditionKey = PDALookup.FindMasterEditionPDA(nftAccount);



            fees += minimumRentTokenAccountDataSize;
            ulong rest = (ulong) (lamports - fees);
            if (address.Price > 0)
            {
                // Add the rest of the SOL to the customer wallet
                if (address.Nftproject.Solanacustomerwallet != null && sendresttoseller)
                {
                    // Calculate the mintingcosts and add it to the transaction
                    var mintingcosts = GlobalFunctions.GetMintingcosts2((int)address.NftprojectId, address.Nfttonftaddresses.Count,
                        address.Price ?? 0);
                    if (mintingcosts.CostsSolana != 0)
                    {
                        bt.MintingcostsTxOut = new TxOutClass()
                        {
                            Amount = mintingcosts.CostsSolana,
                            ReceiverAddress = mintingcosts.MintingcostsreceiverSolana
                        };

                        fees += minimumRentTokenAccountDataSize;
                        PublicKey nmkrMintingAddressPublickey = new PublicKey(mintingcosts.MintingcostsreceiverSolana);
                        transaction = transaction.AddInstruction(
                            SystemProgram.Transfer(
                                addressAccount,
                                nmkrMintingAddressPublickey,
                                (ulong)mintingcosts.CostsSolana));
                    }
                    rest = (ulong)(lamports - fees -(ulong)mintingcosts.CostsSolana);


                    // Check if the user had a discount - then deduct the discount and send the rest of the sol to the seller wallet
                    if (discount != null)
                    {
                        ulong discountInLamport = (ulong) (address.Lovelace * discount.Sendbackdiscount / 100);
                        rest -= discountInLamport;
                        transaction = transaction.AddInstruction(
                            SystemProgram.Transfer(
                                addressAccount,
                                receiverpublickey,
                                discountInLamport));
                        bt.Discount = (long) discountInLamport;
                    }

                    // Send the rest to the seller
                    if (rest > 0)
                    {
                        PublicKey nmkrCustomerWalletPublickey =
                            new PublicKey(address.Nftproject.Solanacustomerwallet.Walletaddress);
                        transaction = transaction.AddInstruction(
                            SystemProgram.Transfer(
                                addressAccount,
                                nmkrCustomerWalletPublickey,
                                rest));
                    }

                    bt.NewCalculatedLamports = 0;
                    bt.ProjectTxOut = new TxOutClass()
                    {
                        Amount = (long) rest,
                        ReceiverAddress = address.Nftproject.Solanacustomerwallet.Walletaddress
                    };
                }
                else
                {
                    bt.NewCalculatedLamports = rest;
                }
            }
            else
            {
                // Free Nfts -Price is null and we send the rest to the buyer
                if (sendresttoseller)
                {
                    if (rest > 0)
                    {
                        transaction = transaction.AddInstruction(
                            SystemProgram.Transfer(
                                addressAccount,
                                receiverpublickey,
                                rest));
                    }
                    bt.NewCalculatedLamports = 0;
                }
            }

            bt.Fees = (long)fees;



            return transaction;
        }

      


        private static async Task<string> GetNftOffchainMetadataLink(Nft nfttonftaddressNft, Nftproject project)
        {
            var metadata=await GetSolanaMetadataAsync(nfttonftaddressNft, project);

            if (string.IsNullOrEmpty(metadata))
                return null;

            try
            {
                // Save To IPFS
                var path = GeneralConfigurationClass.TempFilePath;
                string filename = GlobalFunctions.GetGuid();
                await System.IO.File.WriteAllTextAsync(path + filename, metadata);
                var ipfs = await IpfsFunctions.AddFileAsync(path + filename);
                Ipfsadd ia = Ipfsadd.FromJson(ipfs);

                System.IO.File.Delete(path + filename);

                return $"{GeneralConfigurationClass.IPFSGateway}{ia.Hash}";
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public static async Task<string> GetSolanaMetadataAsync(int nftid, Nftproject project)
        {
            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            var nft = await (from a in db.Nfts
                    .Include(a => a.Nftproject)
                    .AsSplitQuery()
                    .Include(a => a.InverseMainnft)
                    .ThenInclude(a => a.Metadata)
                    .AsSplitQuery()
                    .Include(a => a.Metadata)
                    .AsSplitQuery()
                where a.Id == nftid
                select a).AsNoTracking().FirstOrDefaultAsync();
            if (nft == null)
                return null;

            return await GetSolanaMetadataAsync(nft, project);

        }
        public static async Task<string> GetSolanaMetadataAsync(Nft nfttonftaddressNft, Nftproject project)
        {
            try
            {
                GetMetadataClass gmc = new GetMetadataClass(nfttonftaddressNft.Id, "");
                IConvertCardanoMetadata conv = new ConvertCardanoToSolanaMetadata();
                return conv.ConvertCip25CardanoMetadata((await gmc.MetadataResultAsync()).MetadataCip25, project.Solanasymbol, project.Solanacollectiontransaction, project.SellerFeeBasisPoints);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
        public static string GetSolanaMetadataFromCip25Metadata(string cip25metadata, Nftproject project)
        {
            var parser = new ParseCip25Metadata();
            try
            {
                var parsedMetadata = parser.ParseMetadata(cip25metadata);
                var solanaMetadata = parsedMetadata.ToSolanaMetadata(project);

                return solanaMetadata;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
        public static Wallet GetWallet(Customer address)
        {
            string password = address.Salt;
            var seedphrase = Encryption.DecryptString(address.Solanaseedphrase, password);
            var wallet = new Wallet(seedphrase);
            return wallet;
        }
        public static Wallet GetWallet(Nftaddress address)
        {
            string password = address.Salt + GeneralConfigurationClass.Masterpassword;
            var seedphrase = Encryption.DecryptString(address.Seedphrase, password);
            var wallet = new Wallet(seedphrase);
            return wallet;
        }

        public static Wallet GetWallet(Nftproject project)
        {
            string password = project.Password;
            var seedphrase = Encryption.DecryptString(project.Solanaseedphrase, password);
            var wallet = new Wallet(seedphrase);
            return wallet;
        }
        public static Wallet GetWallet(Adminmintandsendaddress mintandsendaddress)
        {
            var seedphrase = Encryption.DecryptString(mintandsendaddress.Seed, GeneralConfigurationClass.Masterpassword + mintandsendaddress.Salt);
            var wallet = new Wallet(seedphrase);
            return wallet;
        }

        public static SolanaKeysClass ToSolanaKeysClass(this Wallet wallet)
        {
            return new SolanaKeysClass()
            {
                PublicKey = wallet.Account.PublicKey.Key,
                SecretKey = wallet.Account.PrivateKey.Key,
                Address=wallet.Account.PublicKey.Key,
            };
        }

        /// <summary>
        /// Returns the sender of a transaction
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        public static async Task<string> GetSenderAsync(Nftaddress addr)
        {
            IRpcClient rpcClient = ClientFactory.GetClient($"{GeneralConfigurationClass.HeliosConfiguration.ApiUrl}/?api-key={GeneralConfigurationClass.HeliosConfiguration.ApiKey}", null, null, null);

            var accountInfo = await rpcClient.GetSignaturesForAddressAsync(addr.Address);
            if (accountInfo.Result.Count == 0) return null;
            var sig = accountInfo.Result.First().Signature;
            var tx = await rpcClient.GetTransactionAsync(sig);

            if (tx==null || tx.Result==null || tx.Result.Transaction==null || tx.Result.Transaction.Message==null) return null;
            foreach (var messageAccountKey in tx.Result.Transaction.Message.AccountKeys)
            {
                if (!messageAccountKey.Equals(addr.Address) && messageAccountKey.Length>=32 && messageAccountKey.Length<=44 && IsValidSolanaPublicKey(messageAccountKey) && !messageAccountKey.Contains("11111111"))
                {
                    return messageAccountKey;
                }
            }
            return null;
        }

        public static async Task<BuildTransactionClass> SendAllCoinsAndTokensFromNftaddress(Nftaddress address, string receiveraddress, BuildTransactionClass bt,  string sendbackmessage)
        {
            var rpcClient = GetRpcClient();
            var addressWallet = GetWallet(address);

            Account addressAccount = addressWallet.Account;
            PublicKey receiverpublickey = new PublicKey(receiveraddress);


            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            await GlobalFunctions.LogMessageAsync(db, "SendAllCoinsAndTokensFromNftaddress", "SendAllCoinsAndTokensFromNftaddress", 0);
            // Prepare the transaction
            var blockHash = await rpcClient.GetLatestBlockHashAsync();

            // Then build a new transaction with the fees
            bt.SolanaTransaction = await BuildTransactionAllSolAndTokens(address, blockHash, addressAccount,  5000, receiverpublickey, bt, sendbackmessage);
            var tx = Transaction.Deserialize(bt.SolanaTransaction.Build(new List<Account> { addressAccount }));


            // Sign and Send the transaction
            if (tx != null)
            {
                bt = await SignAndSendTransaction(rpcClient, tx, addressAccount, bt);

                // Show Confirmation
                if (!string.IsNullOrEmpty(bt.TxHash))
                {
                    bt.SolanaTransactionMetaSlotInfo = (await rpcClient.GetTransactionAsync(bt.TxHash, Commitment.Confirmed));
                }
            }

            return bt;

        }


        public static async Task<BuildTransactionClass> SendAllCoinsAndTokens(ulong utxo, string seedphrase, string receiveraddress, BuildTransactionClass bt, string sendbackmessage)
        {
            var rpcClient = GetRpcClient();
            var addressWallet = new Wallet(seedphrase);

            Account addressAccount = addressWallet.Account;
            PublicKey receiverpublickey = new PublicKey(receiveraddress);


            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            await GlobalFunctions.LogMessageAsync(db, "SendAllCoinsAndTokens", "SendAllCoinsAndTokens", 0);
            // Prepare the transaction
            var blockHash = await rpcClient.GetLatestBlockHashAsync();

            // Then build a new transaction with the fees
            bt.SolanaTransaction = await BuildTransactionAllSolAndTokens(utxo, blockHash, addressAccount, 5000, receiverpublickey, bt, sendbackmessage);
            var tx = Transaction.Deserialize(bt.SolanaTransaction.Build(new List<Account> { addressAccount }));


            // Sign and Send the transaction
            if (tx != null)
            {
                bt = await SignAndSendTransaction(rpcClient, tx, addressAccount, bt);

                // Show Confirmation
                if (!string.IsNullOrEmpty(bt.TxHash))
                {
                    bt.SolanaTransactionMetaSlotInfo = (await rpcClient.GetTransactionAsync(bt.TxHash, Commitment.Confirmed));
                }
            }

            return bt;

        }
        private static async Task<TransactionBuilder> BuildTransactionAllSolAndTokens(ulong utxo, RequestResult<ResponseValue<LatestBlockHash>> blockHash,
            Account addressAccount, ulong costssignature, PublicKey receiverpublickey, BuildTransactionClass bt, string message)
        {
            ulong fees = (costssignature * 1);

            var transaction = new TransactionBuilder()
                .SetRecentBlockHash(blockHash.Result.Value.Blockhash)
                .SetFeePayer(addressAccount);
             //   .AddInstruction(ComputeBudgetProgram.SetComputeUnitLimit(30000))
             //   .AddInstruction(ComputeBudgetProgram.SetComputeUnitPrice(1000000));


            transaction = transaction.AddInstruction(
                SystemProgram.Transfer(
                    addressAccount,
                    receiverpublickey,
                    (utxo - fees)));

            if (!string.IsNullOrEmpty(message))
                transaction.AddInstruction(MemoProgram.NewMemo(addressAccount.PublicKey, message));

            bt.Fees = (long)fees;

            return transaction;
        }
        private static async Task<TransactionBuilder> BuildTransactionAllSolAndTokens(Nftaddress address, RequestResult<ResponseValue<LatestBlockHash>> blockHash,
            Account addressAccount,  ulong costssignature, PublicKey receiverpublickey, BuildTransactionClass bt, string message)
        {
            return await BuildTransactionAllSolAndTokens((ulong) (address.Lovelace ?? 0), blockHash, addressAccount,
                costssignature, receiverpublickey, bt, message);
        }

        public static async Task<TransactionMetaSlotInfo> GetTransactionAsync(string txhash)
        {
            if (string.IsNullOrEmpty(txhash))
                return null;
            var rpcClient = GetRpcClient();
            var tx=await rpcClient.GetTransactionAsync(txhash);
            return tx.Result;
        }
        public static GenericTransaction ToGenericTransaction(this TransactionMetaSlotInfo transaction)
        {
            if (transaction == null)
                return null;
            return new GenericTransaction()
            {
                Block = transaction.BlockTime.ToString(),
                Blockchain = Blockchain.Solana,
                Slot = transaction.Slot,
                /* Fees = transaction.Fees,
                 Hash = transaction..Hash,
                 Index = transaction.Index*/
            };
        }

        public static SolanaOffchainCollectionMetadataClass GetSolanaCollectionMetadata(string description, string image, string mimetype, string name, int sellerFeeBasisPoints, string symbol, string externalurl)
        {
            var solanaMetadata = new SolanaOffchainCollectionMetadataClass()
            {
                Description = description ?? "",
                Image = image,
                Name = name,
                SellerFeeBasisPoints = sellerFeeBasisPoints,
                Symbol = symbol,
                ExternalUrl = externalurl ?? ""
            };
            if (image != null)
            {
                solanaMetadata.Properties ??= new Properties();

                solanaMetadata.Properties.Files = new File[]
                {
                    new File()
                    {
                        Uri = image,
                        Type = mimetype
                    }
                };
            }

            return solanaMetadata;

        }



        public static async Task<string> CreateSolanaCollectionMetadataUri(string description, string image,string mimetype, string name, int sellerFeeBasisPoints, string symbol, string externalurl)
        {
            var solanaMetadata = GetSolanaCollectionMetadata(description, image, mimetype, name, sellerFeeBasisPoints, symbol, externalurl);

            // Save To IPFS
            var path = GeneralConfigurationClass.TempFilePath;
            string filename = GlobalFunctions.GetGuid();
            await System.IO.File.WriteAllTextAsync(path + filename, JsonConvert.SerializeObject(solanaMetadata, Formatting.Indented));
            var ipfs = await IpfsFunctions.AddFileAsync(path + filename);
            Ipfsadd ia = Ipfsadd.FromJson(ipfs);

            System.IO.File.Delete(path + filename);

            return $"{GeneralConfigurationClass.IPFSGateway}{ia.Hash}";
        }
        public static bool IsValidSolanaPublicKey(string publicKeyString)
        {
            try
            {
                var publicKey = new PublicKey(publicKeyString);
                return publicKey.KeyBytes.Length == 32;
            }
            catch
            {
                return false;
            }
        }
        // We accept as policy id the collection publickey or the mintauthority, the assetname is the tokenname from the metadata
        public static async Task<AssetsAssociatedWithAccount[]> GetAllAssetsInWalletAsync(IConnectionMultiplexer redis, string receiveraddress)
        {
            List<AssetsAssociatedWithAccount> assets = new List<AssetsAssociatedWithAccount>();
            var tokenAccounts = await GetAssetsByOwnerAsync(receiveraddress);
            if (tokenAccounts.Result == null || tokenAccounts.Result.Items.Length==0)
                return assets.ToArray();
            foreach (var tokenAccount in tokenAccounts.Result.Items.OrEmptyIfNull())
            {
                if (tokenAccount.Grouping != null && tokenAccount.Grouping.Any())
                {
                    var collection = tokenAccount.Grouping.FirstOrDefault(x => x.GroupKey == "collection");
                    if (collection != null)
                    {
                        assets.Add(new AssetsAssociatedWithAccount(collection.GroupValue, tokenAccount.Content.Metadata.Name.ToHex(), tokenAccount.TokenInfo?.Balance??1, Blockchain.Solana, tokenAccount.Id, tokenAccount.Content.Metadata.Symbol));
                    }
                    else
                    {
                        var ma  = tokenAccount.TokenInfo.MintAuthority;
                        if (!string.IsNullOrEmpty(ma))
                        {
                            assets.Add(new AssetsAssociatedWithAccount(ma, tokenAccount.Content.Metadata.Name.ToHex(), tokenAccount.TokenInfo?.Balance??1, Blockchain.Solana, tokenAccount.Id, tokenAccount.Content.Metadata.Symbol));
                        }
                    }
                }
            }

            return assets.ToArray();
        }

        public static async Task<BuildTransactionClass> MintAndSendAsync(Nft nft, 
            string receiveraddress, Adminmintandsendaddress payaddress, BuildTransactionClass buildtransaction)
        {
            var rpcClient = GetRpcClient();
          
            var paywallet = GetWallet(payaddress);
            Account payAccount = paywallet.Account;

            // Prepare the transaction
            var blockHash = await rpcClient.GetLatestBlockHashAsync();
            var minimumRentMintAccountDataSize =
                (await rpcClient.GetMinimumBalanceForRentExemptionAsync(TokenProgram.MintAccountDataSize)).Result;
            var minimumRentTokenAccountDataSize =
                (await rpcClient.GetMinimumBalanceForRentExemptionAsync(TokenProgram.TokenAccountDataSize)).Result;

            
                var projectWallet = GetWallet(nft.Nftproject);
                MintSolanaNftClass mintInfo = new MintSolanaNftClass(nft, 1, receiveraddress, projectWallet)
                    {NftAccount = projectWallet.GetAccount(nft.Id)};


            int tries = 0;
            do
            {
                tries++;
                if (tries > 3)
                    break;

                // Then build a new transaction with the fees
                buildtransaction.SolanaTransaction = await BuildTransactionMintAndSend(blockHash,
                    payAccount,
                    mintInfo, minimumRentMintAccountDataSize, minimumRentTokenAccountDataSize, 5000,
                    buildtransaction);

                var signers = new List<Account> {payAccount, mintInfo.NftAccount};

                var tx = Transaction.Deserialize(buildtransaction.SolanaTransaction.Build(signers));

                // Sign and Send the transaction
                if (tx != null)
                {
                 /*   var priofees=await GetPriorityFeeEstimate("Medium", tx);
                    if (priofees!=0)
                       tx.Add(ComputeBudgetProgram.SetComputeUnitPrice(2000));*/

                    var res = await SignAndSendTransaction(rpcClient, tx, payAccount, buildtransaction, false,
                        Commitment.Finalized);

                    // Check Confirmation
                    if (!string.IsNullOrEmpty(res.TxHash))
                    {
                        buildtransaction.SolanaTransactionMetaSlotInfo =
                            (await rpcClient.GetTransactionAsync(res.TxHash, Commitment.Finalized));

                        break;
                    }

                    // Try again
                    await Task.Delay(500);
                }
                else
                {
                    // Transaction could not created - so break
                    break;
                }
            } while (true);

            return buildtransaction;
        }

        public static async Task<BuildTransactionClass> MintAndSendCoreAsync(Nft nft, Nftproject project,
          string receiveraddress, Wallet paywallet, BuildTransactionClass buildtransaction)
        {
            var updateWallet = GetWallet(project);

            buildtransaction.BuyerTxOut=new TxOutClass()
            {
                Amount = 0,
                ReceiverAddress = receiveraddress
            };

            // Currently we only support 1 creator for the royalties and a 100% share.
            var creator = new CreatorsClass()
            {
                //    Address = project.Solanacustomerwallet.Walletaddress,
                Address = updateWallet.ToSolanaKeysClass().Address, // Updateauthority = Creator
                Verified = true,
                Share = 100
            };


            MintSolanaNftCoreClass mintObject = new MintSolanaNftCoreClass()
            {
                NftReceiverAddress = receiveraddress,
                CollectionAddress = project.Solanacollectiontransaction,
                Payer = paywallet.ToSolanaKeysClass(),
                UpdateAuthority = updateWallet.ToSolanaKeysClass(),
                ComputeUnitPrice = 200000,
                ComputeUnitLimit= 30000000,
                Metadata = new SolanaMetadataClass()
                {
                    Name = nft.Name,
                    Symbol = project.Solanasymbol,
                    Uri = await GetNftOffchainMetadataLink(nft, project),
                    SellerFeeBasisPoints = nft.Nftproject.SellerFeeBasisPoints ?? 0,
                }
            };
            if (nft.Nftproject.SellerFeeBasisPoints == null ||
                nft.Nftproject.SellerFeeBasisPoints == 0)
            {
                mintObject.Creators = new[] {creator};
            }


            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            await GlobalFunctions.LogMessageAsync(db, "MintAndSendCoreAsync", "MintAndSendCoreAsync", 0);

            buildtransaction.Log(JsonConvert.SerializeObject(mintObject, Formatting.Indented));
            ulong paywalletbalance = await GetWalletBalanceAsync(paywallet.Account.PublicKey);

            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("POST"), $"{GeneralConfigurationClass.SolanaApiUrl}/mintSolanaNftx");
            string st = JsonConvert.SerializeObject(mintObject, Formatting.Indented);

            request.Content = new StringContent(st);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return buildtransaction;

            var body = await response.Content.ReadAsStringAsync();
            var mintresult=JsonConvert.DeserializeObject<SolanaMintResultClass>(body);
            if (mintresult == null || mintresult.state.Contains("Error"))
            {
                buildtransaction.Log(body);
                buildtransaction.ErrorMessage = mintresult != null ? mintresult.state : "Solana Mint & Send - Error - Result was null";
                buildtransaction.LastTransaction = "Error";
            }
            else
            {
                buildtransaction.MintAssetAddress.Add(new MintCoreNftsResult(){MintAddress = mintresult.assetAddress, TxHash = mintresult.transactionHash, NftId = nft.Id});
                buildtransaction.TxHash=mintresult.transactionHash;
                buildtransaction.Log($"Mint was successful - TXHASH: {mintresult.transactionHash} - MintAdddress: {mintresult.assetAddress}");
                buildtransaction.LastTransaction = "OK";
                ulong paywalletbalancenew = await GetWalletBalanceAsync(paywallet.Account.PublicKey);
                buildtransaction.Fees = (long)(paywalletbalance - paywalletbalancenew);
            }

            return buildtransaction;
        }


        public static async Task<BuildTransactionClass> BurnSolanaNftAsync(Adminmintandsendaddress paywallet, Burnigendpoint endpoint, string nft, BuildTransactionClass buildtransaction)
        {

            BurnSolanaNftCoreClass burnObject = new BurnSolanaNftCoreClass()
            {
                NftMintAddress = nft,
                WalletAddress = ConvertToSolanaKeysClass(endpoint),
                Payer = ConvertToSolanaKeysClass(paywallet),
            };
            buildtransaction.Log(JsonConvert.SerializeObject(burnObject, Formatting.Indented));

            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("POST"), $"{GeneralConfigurationClass.SolanaApiUrl}/burnSolanaNftx");
            string st = JsonConvert.SerializeObject(burnObject, Formatting.Indented);

            request.Content = new StringContent(st);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return buildtransaction;

            var body = await response.Content.ReadAsStringAsync();
            var mintresult = JsonConvert.DeserializeObject<SolanaMintResultClass>(body);
            if (mintresult == null || mintresult.state.Contains("Error"))
            {
                buildtransaction.Log(body);
                buildtransaction.ErrorMessage = mintresult != null ? mintresult.state : "Solana Burn - Error - Result was null";
            }
            else
            {
                buildtransaction.MintAssetAddress.Add(new MintCoreNftsResult() { MintAddress = mintresult.assetAddress, TxHash = mintresult.transactionHash });
                buildtransaction.TxHash = mintresult.transactionHash;
            }

            return buildtransaction;
        }


        public static async Task<ulong> GetPriorityFeeEstimate(string priorityLevel, Transaction transaction)
        {
            using HttpClient httpClient = new HttpClient();
            var p = new HeliusGetPriorityFeesTransactionParam()
            {
                Transaction = Encoders.Base58.EncodeData(transaction.Serialize()),
                Options = new HeliusGetPriorityFeesTransactionOptions()
                    {PriorityLevel = priorityLevel}
            };

            HeliusGetPriorityFeesTransactionClass options = new HeliusGetPriorityFeesTransactionClass
            {
                Jsonrpc="2.0",
                Id="NMKR",
                Method= "getPriorityFeeEstimate",
                Params = new[] {p}
            };
            string st = JsonConvert.SerializeObject(options);
            var content = new StringContent(st, Encoding.UTF8, "application/json");

            var heliusUrl = $"{GeneralConfigurationClass.HeliosConfiguration.ApiUrl}/?api-key={GeneralConfigurationClass.HeliosConfiguration.ApiKey}";
            var response = await httpClient.PostAsync(heliusUrl, content);

            if (response.IsSuccessStatusCode)
            {
                string responseString = await response.Content.ReadAsStringAsync();
                dynamic jsonResponse = JObject.Parse(responseString);

               // Console.WriteLine("Fee in function for {0} : {1}", priorityLevel, jsonResponse?.result?.priorityFeeEstimate);

                return jsonResponse?.result?.priorityFeeEstimate;
            }

            return 0;
        }


        private static async Task<TransactionBuilder> BuildTransactionMintAndSend(RequestResult<ResponseValue<LatestBlockHash>> blockHash, Account payAccount, MintSolanaNftClass mintInfo, ulong minimumRentMintAccountDataSize, ulong minimumRentTokenAccountDataSize,ulong costssignature,  BuildTransactionClass bt)
        {
            ulong fees = minimumRentMintAccountDataSize + (costssignature * 2);

            var transaction = new TransactionBuilder()
                .SetRecentBlockHash(blockHash.Result.Value.Blockhash)
                .SetFeePayer(payAccount)
                .AddInstruction(ComputeBudgetProgram.SetComputeUnitLimit(30000))
                .AddInstruction(ComputeBudgetProgram.SetComputeUnitPrice(1000000));


            mintInfo.Metadata = new global::Solnet.Metaplex.NFT.Library.Metadata()
                {
                    name = mintInfo.Nft.Name,
                    symbol = mintInfo.Nft.Nftproject.Solanasymbol,
                    uri = await GetNftOffchainMetadataLink(mintInfo.Nft, mintInfo.Nft.Nftproject),
                    sellerFeeBasisPoints = 0,
                    creators = new List<Creator> {new(mintInfo.NftAccount.PublicKey, 100, true)}
                };
                // Set Collection
                if (!string.IsNullOrEmpty(mintInfo.Nft.Nftproject.Solanacollectiontransaction))
                {
                    PublicKey collectionkey = new PublicKey(mintInfo.Nft.Nftproject.Solanacollectiontransaction);
                    mintInfo.Metadata.collection = new Collection(collectionkey, false);
                }

                fees += 2853600; // Token Metadata Program
                fees += 15616720; // Token Metadata Program

                var associatedTokenAccount = AssociatedTokenAccountProgram
                    .DeriveAssociatedTokenAccount(mintInfo.NftAccount.PublicKey, mintInfo.NftAccount.PublicKey);

                transaction.AddInstruction(
                        SystemProgram.CreateAccount(
                            payAccount,
                            mintInfo.NftAccount.PublicKey,
                            minimumRentMintAccountDataSize,
                            TokenProgram.MintAccountDataSize,
                            TokenProgram.ProgramIdKey))
                    .AddInstruction(
                        TokenProgram.InitializeMint(
                            mintInfo.NftAccount.PublicKey,
                            0,
                            mintInfo.NftAccount,
                            mintInfo.NftAccount))
                    .AddInstruction(
                        AssociatedTokenAccountProgram.CreateAssociatedTokenAccount(
                            payAccount,
                            mintInfo.NftAccount,
                            mintInfo.NftAccount.PublicKey))
                    .AddInstruction(
                        TokenProgram.MintTo(
                            mintInfo.NftAccount.PublicKey,
                            associatedTokenAccount,
                            1,
                            mintInfo.NftAccount))
                    .AddInstruction(MetadataProgram.CreateMetadataAccount(
                        PDALookup.FindMetadataPDA(mintInfo.NftAccount),
                        mintInfo.NftAccount.PublicKey,
                        mintInfo.NftAccount,
                        payAccount,
                        mintInfo.NftAccount.PublicKey,
                        mintInfo.Metadata,
                        TokenStandard.NonFungible,
                        true,
                        true,
                        null,
                        metadataVersion: MetadataVersion.V3));

            // Transfer the Token to the Buyer
            var sta = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(mintInfo.NftAccount.PublicKey, mintInfo.NftAccount);
                var ata = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(mintInfo.ReceiverPublickKey, mintInfo.NftAccount);

                transaction = transaction
                    .AddInstruction(
                        CreateMasterEdition(
                            maxSupply: null,
                            masterEditionKey: PDALookup.FindMasterEditionPDA(mintInfo.NftAccount),
                            mintKey: mintInfo.NftAccount,
                            updateAuthorityKey: mintInfo.NftAccount,
                            mintAuthority: mintInfo.NftAccount,
                            payer: payAccount,
                            metadataKey: PDALookup.FindMetadataPDA(mintInfo.NftAccount),
                            version: CreateMasterEditionVersion.V3
                        ))
                    .AddInstruction( // This is needed for the transfer to the receiver
                        AssociatedTokenAccountProgram.CreateAssociatedTokenAccount(
                            payAccount,
                            mintInfo.ReceiverPublickKey,
                            mintInfo.NftAccount))
                    .AddInstruction( // This is the transfer of the nft to the receiver
                        TokenProgram.Transfer(
                            sta,
                            ata,
                            1,
                            mintInfo.NftAccount
                        ));

                mintInfo.TokenSource = sta;
                mintInfo.TokenDestination = ata;
                mintInfo.MasterEditionKey = PDALookup.FindMasterEditionPDA(mintInfo.ReceiverPublickKey);

            bt.SolanaMintInfo = mintInfo;
           
            bt.Fees = (long)fees;
            return transaction;
        }


        public static async Task<SolanaItem[]> GetAllAssetsForCollectionAsync(string collection)
        {
            GetAssetsByOwnerClass getAssetsByOwnerClass = new GetAssetsByOwnerClass()
            {
                Method = "getAssetsByGroup",
                Params = new Params()
                {
                    GroupKey = "collection",
                    GroupValue = collection,
                    Page = 1,
                    Limit = 1000,
                }
            };
            string st = JsonConvert.SerializeObject(getAssetsByOwnerClass);
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{GeneralConfigurationClass.HeliosConfiguration.ApiUrl}/?api-key={GeneralConfigurationClass.HeliosConfiguration.ApiKey}"),
                Content = new StringContent(st)
                {
                    Headers =
                    {
                        ContentType = new MediaTypeHeaderValue("application/json")
                    }
                }
            };
            using var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode == false)
                return null;
            var body = await response.Content.ReadAsStringAsync();
            var res= JsonConvert.DeserializeObject<SolanaGetAssetsByGroupClass>(body);
            if (res.Result?.Items==null)
                return null;
            return res.Result?.Items;
        }

        public static BlockfrostAssetClass? GetAssetFromSolanaBlockchain(Nftproject project, string tokenPubkey)
        {
            var res = Task.Run(async () => await GetAssetFromSolanaBlockchainAsync(project,tokenPubkey));
            return res.Result;
        }

        public static async Task<BlockfrostAssetClass?> GetAssetFromSolanaBlockchainAsync(Nftproject project, string tokenPubkey)
        {
            var rpcClient = GetRpcClient();
            var token = await rpcClient.GetTokenAccountInfoAsync(tokenPubkey);


            BlockfrostAssetClass asset = new BlockfrostAssetClass()
            {
                Asset = "",
                PolicyId = "",
                AssetName = "",
                Fingerprint = "",
                Quantity = 0,
                InitialMintTxHash = "",
                MintOrBurnCount = 0,
                Blockchain = Blockchain.Solana,
            };


            if (token == null)
                return null;

            if (token.Reason != "OK")
                return null;

            if (token.Result == null)
                return null;

            if (token.Result.Value == null)
                return null;

            asset.Fingerprint = token.Result.Value.Owner;
            asset.InitialMintTxHash = token.Result.Value.Owner;
            asset.PolicyId = project.Solanacollectiontransaction;
            asset.Quantity = 1;

            return asset;
        }

        public static KoiosAccountInfoClass GetStakePoolInformation(IConnectionMultiplexer redis, string address)
        {
            var res = Task.Run(async () => await GetStakePoolInformationAsync(redis, address));
            return res.Result;
        }
        internal static async Task<KoiosAccountInfoClass> GetStakePoolInformationAsync(IConnectionMultiplexer redis, string receiveraddress)
        {
            // TODO: Get solana stakepool information
            return null;
        }

        public static async Task<BuildTransactionClass> SendSolAsync(string seed, TxOutClass[] addresses,
            BuildTransactionClass buildtransaction)
        {
            return await (SendSolAsync(new Wallet(seed), addresses, buildtransaction));
        }
        public static async Task<BuildTransactionClass> SendSolAsync(Wallet paywallet, TxOutClass[] addresses, BuildTransactionClass buildtransaction)
        {
            var rpcClient = GetRpcClient();

            Account payAccount = paywallet.Account;


            // Prepare the transaction

            var minimumRentMintAccountDataSize =
                (await rpcClient.GetMinimumBalanceForRentExemptionAsync(TokenProgram.MintAccountDataSize)).Result;

            int tries = 0;
            do
            {
                tries++;
                if (tries > 3)
                    break;

                await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
                await GlobalFunctions.LogMessageAsync(db, "SendSolAsync", "SendSolAsync", 0);

                var blockHash = await rpcClient.GetLatestBlockHashAsync();
                // Then build a new transaction with the fees
                buildtransaction.SolanaTransaction = BuildTransactionSendToMultipleAddressed(blockHash,
                    payAccount, addresses, minimumRentMintAccountDataSize, 5000,
                    buildtransaction);

                var signers = new List<Account>
                {
                    payAccount
                };

                var tx = Transaction.Deserialize(buildtransaction.SolanaTransaction.Build(signers));


                // Sign and Send the transaction
                if (tx != null)
                {
                    var res = await SignAndSendTransaction(rpcClient, tx, payAccount, buildtransaction, false,
                        Commitment.Finalized);

                    // Check Confirmation
                    if (!string.IsNullOrEmpty(res.TxHash))
                    {
                        buildtransaction.SolanaTransactionMetaSlotInfo =
                            (await rpcClient.GetTransactionAsync(res.TxHash, Commitment.Finalized));

                        break;
                    }
                    else
                    {
                        buildtransaction.LogFile += "Transaction could not signed"+Environment.NewLine;
                    }

                    // Try again
                    await Task.Delay(500);
                }
                else
                {
                    buildtransaction.LogFile+= "Transaction could not deserialized" + Environment.NewLine; 
                    // Transaction could not created - so break
                    break;
                }
            } while (true);

            return buildtransaction;
        }


        private static TransactionBuilder BuildTransactionSendToMultipleAddressed(RequestResult<ResponseValue<LatestBlockHash>> blockHash, Account payAccount, TxOutClass[] addresses, ulong minimumRentMintAccountDataSize, ulong costssignature, BuildTransactionClass bt)
        {
            ulong fees = (costssignature * 1);

            var transaction = new TransactionBuilder()
                .SetRecentBlockHash(blockHash.Result.Value.Blockhash)
                .SetFeePayer(payAccount);
             //   .AddInstruction(ComputeBudgetProgram.SetComputeUnitLimit(30000))
             //   .AddInstruction(ComputeBudgetProgram.SetComputeUnitPrice(1000000));

            int i = 0;
            foreach (var address in addresses)
            {
                i++;
                ulong lamports = (ulong) address.Amount;
                if (i == addresses.Length)
                    lamports -= fees;
                PublicKey receiverpublickey = new PublicKey(address.ReceiverAddress);
                transaction = transaction.AddInstruction(
                    SystemProgram.Transfer(
                        payAccount,
                        receiverpublickey,
                        lamports));
            }

            bt.Fees = (long)fees;

            return transaction;
        }
    }
}
