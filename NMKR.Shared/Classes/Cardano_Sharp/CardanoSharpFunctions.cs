using NMKR.Shared.Functions.Koios;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using StackExchange.Redis;
using System;
using System.Linq;
using CardanoSharp.Wallet.Extensions.Models.Transactions;
using CardanoSharp.Wallet.Models.Addresses;
using CardanoSharp.Wallet.Models.Keys;
using CardanoSharp.Wallet.TransactionBuilding;
using Newtonsoft.Json;
using System.Collections.Generic;
using CardanoSharp.Wallet;
using CardanoSharp.Wallet.CIPs.CIP2.Models;
using CardanoSharp.Wallet.Enums;
using CardanoSharp.Wallet.Extensions;
using CardanoSharp.Wallet.Extensions.Models;
using CardanoSharp.Wallet.Models.Transactions.TransactionWitness;
using CardanoSharp.Wallet.Utilities;
using NMKR.Shared.Enums;
using NMKR.Shared.Classes.CardanoSerialisationLibClasses;
using CardanoSharp.Wallet.Models.Transactions;
using NMKR.Shared.Classes.CustodialWallets;
using System.Threading.Tasks;
using CardanoSharp.Wallet.Extensions.Models.Transactions.TransactionWitnesses;
using NMKR.Shared.Functions.Meastro;
using Newtonsoft.Json.Linq;
using NMKR.Shared.Classes.Vesting;

namespace NMKR.Shared.Classes.Cardano_Sharp
{
    public static class CardanoSharpFunctions
    {
        /// <summary>
        /// Gets the SKey/VKey from a Cli Key CBOR File and removes the OCTECT Characters (that the Key is 32 bits long - not 34 with the 2 OCTECT chars)
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetKeyFromCbor(string key)
        {
            var cbor = ConsoleCommand.GetCbor(key);
            if (string.IsNullOrEmpty(cbor))
            {
                return "";
            }
            return cbor.Substring(4);  // the first 2 chars are OCTECT Characters (58 = Cardano specific / 20 = 32 bits length)
        }


        private static string GetKeyJson(string key, string description, string type)
        {
            MatxRawClass keyfile = new MatxRawClass()
            {
                CborHex = $"58{(key.Length/2).ToString("X")}" + key, Type = type, 
                Description = description
            };

            return JsonConvert.SerializeObject(keyfile,Formatting.Indented);
        }


        /// <summary>
        /// Sends all or a part of the ADA from the sender to the receiver. No Tokens supported
        /// </summary>
        /// <param name="db"></param>
        /// <param name="redis"></param>
        /// <param name="senderaddress"></param>
        /// <param name="senderskey"></param>
        /// <param name="sendervkey"></param>
        /// <param name="receiveraddress"></param>
        /// <param name="adaToSend"></param>
        /// <param name="mainnet"></param>
        /// <param name="givenutxo"></param>
        /// <param name="buildTransaction"></param>
        /// <returns></returns>
        public static string SendAdaPart(EasynftprojectsContext db, IConnectionMultiplexer redis, string senderaddress,
            string senderskey, string sendervkey,
            string receiveraddress, long adaToSend, bool mainnet, TxInAddressesClass givenutxo,
            out BuildTransactionClass buildTransaction)
        {
            return SendAdaPart(db, redis, senderaddress, senderskey, sendervkey,
                new[] { new TxOutClass() { Amount = adaToSend, ReceiverAddress = receiveraddress } }, mainnet, givenutxo,
                out buildTransaction);
        }

        public static string SendAdaPart(EasynftprojectsContext db, IConnectionMultiplexer redis, string senderaddress, string senderskey, string sendervkey,
            TxOutClass[] txouts, bool mainnet, TxInAddressesClass givenutxo, out BuildTransactionClass buildTransaction)
        {
            buildTransaction = new BuildTransactionClass();
            var q = ConsoleCommand.GetQueryTip();
            if (q == null)
            {
                return "Error while getting Query Tip";
            }

            var utxopaymentaddress =
                JsonConvert.DeserializeObject<TxInAddressesClass>(JsonConvert.SerializeObject(givenutxo)) ??
                ConsoleCommand.GetNewUtxo(senderaddress);

            if (txouts.Sum(x => x.Amount) > utxopaymentaddress.LovelaceSummary)
                return "Not enough ADA available";



            var txBody = TransactionBodyBuilder.Create;
            foreach (var txin in utxopaymentaddress.TxIn)
            {
                txBody.AddInput(txin.TxHash, (uint)txin.TxId);
            }

            foreach (var txout in txouts)
            {
                txBody.AddOutput(new Address(txout.ReceiverAddress), (ulong)txout.Amount);
            }
            if (utxopaymentaddress.LovelaceSummary > txouts.Sum(x => x.Amount))
            {
                txBody.AddOutput(new Address(senderaddress), (ulong)(utxopaymentaddress.LovelaceSummary - txouts.Sum(x => x.Amount)));
            }

            txBody.SetTtl((uint)((q.Slot ?? 0) + 3600))
                .SetFee(0)
                .Build();


            var witnesses = TransactionWitnessSetBuilder.Create
                .AddVKeyWitness(new PublicKey(Convert.FromHexString(GetKeyFromCbor(sendervkey)), new byte[] { }), new PrivateKey(Convert.FromHexString(GetKeyFromCbor(senderskey)), new byte[] { }));

            var txBuilder = TransactionBuilder.Create
                .SetBody(txBody)
                .SetWitnesses(witnesses);

            var signedTx = BuildAndCalculateFees(redis, mainnet, txBuilder, txBody, ref buildTransaction);
            return SubmitTransaction(db, signedTx, ref buildTransaction);
        }

        public static AddWitnessResponse AddWitness(AddWitnessRequest request)
        {
            try
            {
                var transaction = request.TxCbor.HexToByteArray().DeserializeTransaction();
                if (transaction == null)
                {
                    throw new InvalidOperationException("Could not deserialize txCbor");
                }

                transaction.TransactionWitnessSet ??= new TransactionWitnessSet();

                var witnessSet = request.WitnessCbor.HexToByteArray().DeserializeTransactionWitnessSet();

                foreach (var vkeyWitness in witnessSet.VKeyWitnesses.OrEmptyIfNull())
                {
                    transaction.TransactionWitnessSet.VKeyWitnesses.Add(vkeyWitness);
                }

                foreach (var nativeScript in witnessSet.NativeScripts.OrEmptyIfNull())
                {
                    transaction.TransactionWitnessSet.NativeScripts.Add(nativeScript);
                }

                foreach (var bootstrap in witnessSet.BootStrapWitnesses.OrEmptyIfNull())
                {
                    transaction.TransactionWitnessSet.BootStrapWitnesses.Add(bootstrap);
                }


                var response = new AddWitnessResponse();
                response.Request = request;
                response.TxCbor = transaction.Serialize().ToStringHex();
                return response;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public static long? CalculateFees(IConnectionMultiplexer redis, bool mainnet, ITransactionBuilder txBuilder,
            ITransactionBodyBuilder txBody, ref BuildTransactionClass bt)
        {
            var prot = ConsoleCommand.GetProtocolParameters(redis, mainnet);
            if (prot == null)
                return null;

            var tx = txBuilder.Build();
            var ser=tx.Serialize();
            var fee= tx.CalculateBaseFee((uint)(prot.MinFeeA ?? 0), (uint)(prot.MinFeeB ?? 0));
          //  var fee = tx.CalculateFee((uint)(prot.MinFeeA ?? 0), (uint)(prot.MinFeeB ?? 0));
            bt.Fees = (long)fee;

            return bt.Fees;
        }


        public static CardanoSharp.Wallet.Models.Transactions.Transaction BuildAndCalculateFees(IConnectionMultiplexer redis, bool mainnet, ITransactionBuilder txBuilder, ITransactionBodyBuilder txBody, ref BuildTransactionClass bt)
        {
            var fee = CalculateFees(redis, mainnet, txBuilder, txBody, ref bt);
            if (fee== null) return null;

            txBody.SetFee((ulong)fee).Build();
            var tx = txBuilder.Build();
            tx.TransactionBody.TransactionOutputs.Last().Value.Coin -= (ulong)fee;
            return tx;
        }

        /// <summary>
        /// Sends all ADA and all Tokens to an address. Optional Message is supported (for sendback)
        /// </summary>
        /// <param name="db"></param>
        /// <param name="redis"></param>
        /// <param name="senderaddress"></param>
        /// <param name="senderskey"></param>
        /// <param name="sendervkey"></param>
        /// <param name="senderskeypassword"></param>
        /// <param name="receiveraddress"></param>
        /// <param name="mainnet"></param>
        /// <param name="buildtransaction"></param>
        /// <param name="txhash"></param>
        /// <param name="maxtx"></param>
        /// <param name="skippages"></param>
        /// <param name="sendbackmessage"></param>
        /// <param name="givenutxo"></param>
        /// <returns></returns>
        public static string SendAllAdaAndTokens(EasynftprojectsContext db, IConnectionMultiplexer redis, string senderaddress, string senderskey, string sendervkey,
          string senderskeypassword,
          string receiveraddress, bool mainnet, ref BuildTransactionClass buildtransaction, string txhash, int maxtx,
          int skippages = 0, string sendbackmessage = "", TxInAddressesClass givenutxo = null, Adminmintandsendaddress paywallet=null)
        {

            var q = ConsoleCommand.GetQueryTip();
            if (q == null)
            {
                return "Error while getting Query Tip";
            }
            long additionalovalacefrompaywallet = 150000;

            var utxopaymentaddress =
                JsonConvert.DeserializeObject<TxInAddressesClass>(JsonConvert.SerializeObject(givenutxo)) ??
                ConsoleCommand.GetNewUtxo(senderaddress);

            if (!string.IsNullOrEmpty(txhash))
                utxopaymentaddress = FilterTxInAddressesClass(utxopaymentaddress, txhash);

            utxopaymentaddress = GetMaxTx(utxopaymentaddress, maxtx, skippages);

            var auxData = AuxiliaryDataBuilder.Create;
            if (!string.IsNullOrEmpty(sendbackmessage))
            {
                auxData = AuxiliaryDataBuilder.Create
                    .AddMetadata(674, new The674() { Msg = new[] { sendbackmessage } });
            }



            // Set tokens
            List<CumulateCardanosharpTokens> tokens = new List<CumulateCardanosharpTokens>();
            var tokenAsset = TokenBundleBuilder.Create;
            foreach (var txInClass in utxopaymentaddress.TxIn)
            {
                foreach (var txInTokensClass in txInClass.Tokens.OrEmptyIfNull())
                {
                    var policyid = txInTokensClass.PolicyId;
                    var asset = txInTokensClass.TokennameHex;

                    var t = tokens.Find(x => x.PolicyId == policyid && x.Asset == asset);
                    if (t != null)
                    {
                        t.Quantity += txInTokensClass.Quantity;
                        continue;
                    }
                    tokens.Add(new CumulateCardanosharpTokens(){Asset = asset, PolicyId = policyid, Quantity = txInTokensClass.Quantity});
                }
            }

            foreach (var token in tokens)
            {
                tokenAsset.AddToken(GlobalFunctions.ConvertHexStringToByteArray(token.PolicyId), GlobalFunctions.ConvertHexStringToByteArray(token.Asset),
                    token.Quantity);
            }

            if (!tokens.Any())
                additionalovalacefrompaywallet = 0;


            var txBody = TransactionBodyBuilder.Create;
            foreach (var txin in utxopaymentaddress.TxIn)
            {
                txBody.AddInput(txin.TxHash, (uint)txin.TxId);
            }

            // If we have pay with tokens only - and we have a refund, we need to add more ada - i dont know why, but it only works with more then the user send
            txBody.AddOutput(new Address(receiveraddress), (ulong)(utxopaymentaddress.LovelaceSummary+(paywallet!=null ? additionalovalacefrompaywallet : 0)), tokenAsset);
            
            if (paywallet!=null)
            {
                var utxopaymentaddresspaywallet = ConsoleCommand.FilterAllTxInWithTokens(ConsoleCommand.GetNewUtxo(paywallet.Address));
                foreach (var txInClass in utxopaymentaddresspaywallet.TxIn)
                {
                    txBody.AddInput(txInClass.TxHash, (uint) txInClass.TxId);
                }
                // Last output for the fees
                txBody.AddOutput(new Address(paywallet.Address), (ulong) (utxopaymentaddresspaywallet.LovelaceSummary- additionalovalacefrompaywallet));
            }

            txBody.SetTtl((uint)((q.Slot ?? 0) + 3600))
                .SetFee(0)
                .Build();


            // Decrypt Keys - if necessary
            string skey = senderskey;
            string vkey = sendervkey;
            if (!string.IsNullOrEmpty(senderskeypassword))
            {
                skey = Encryption.DecryptString(senderskey, senderskeypassword);
                vkey = Encryption.DecryptString(sendervkey, senderskeypassword);
            }

            var witnesses = TransactionWitnessSetBuilder.Create
                .AddVKeyWitness(new PublicKey(Convert.FromHexString(GetKeyFromCbor(vkey)), new byte[] { }), new PrivateKey(Convert.FromHexString(GetKeyFromCbor(skey)), new byte[] { }));
            
            // Add additional keys if we have the paywallets
            if (paywallet != null)
            {
                var skey1 = Encryption.DecryptString(paywallet.Privateskey, GeneralConfigurationClass.Masterpassword + paywallet.Salt);
                var vkey1= Encryption.DecryptString(paywallet.Privatevkey, GeneralConfigurationClass.Masterpassword + paywallet.Salt);

                witnesses.AddVKeyWitness(new PublicKey(Convert.FromHexString(GetKeyFromCbor(vkey1)), new byte[] { }),
                    new PrivateKey(Convert.FromHexString(GetKeyFromCbor(skey1)), new byte[] { }));
            }

            var txBuilder = TransactionBuilder.Create
                .SetBody(txBody)
                .SetAuxData(auxData)
                .SetWitnesses(witnesses);

            var signedTx = BuildAndCalculateFees(redis, mainnet, txBuilder, txBody, ref buildtransaction);
            buildtransaction.LogFile += Environment.NewLine + JsonConvert.SerializeObject(txBody);
            
            if (paywallet != null)
                buildtransaction.NmkrCosts = buildtransaction.Fees + additionalovalacefrompaywallet;
            return SubmitTransaction(db, signedTx, ref buildtransaction);
        }
        public static TxInAddressesClass FilterTxInAddressesClass(TxInAddressesClass txInAddressesClass, string filtertxhash)
        {
            if (txInAddressesClass == null)
                return null;
            if (txInAddressesClass.TxIn == null || txInAddressesClass.TxIn.Length == 0)
                return txInAddressesClass;
            if (string.IsNullOrEmpty(filtertxhash))
                return txInAddressesClass;

            List<TxInClass> search = txInAddressesClass.TxIn.ToList();

            search = search.FindAll(x => x.TxHash == filtertxhash || x.TxHashId == filtertxhash).ToList();
            txInAddressesClass.TxIn = search.ToArray();

            return txInAddressesClass;
        }

        public static TxInAddressesClass GetMaxTx(TxInAddressesClass txInAddressesClass, int maxtx, int skippages=0)
        {
            if (txInAddressesClass == null)
                return null;
            if (txInAddressesClass.TxIn == null || txInAddressesClass.TxIn.Length == 0)
                return txInAddressesClass;
            if (maxtx == 0)
                return txInAddressesClass;
            if (maxtx>= txInAddressesClass.TxIn.Length)
                return txInAddressesClass;


            int skipCount = skippages * maxtx;

            // Filtere die TxIn-Einträge basierend auf Skip und Take
            var filteredTxIn = txInAddressesClass.TxIn
                .Skip(skipCount) // Überspringe die Einträge basierend auf skippages
                .Take(maxtx)     // Nimm nur die Anzahl von maxtx
                .ToArray();


            txInAddressesClass.TxIn = filteredTxIn;

            /*
            for (int i = txInAddressesClass.TxIn.Length - 1; i >= 0; i--)
            {
                if (i >= maxtx)
                {
                    txInAddressesClass.TxIn = txInAddressesClass.TxIn.RemoveFromArray(txInAddressesClass.TxIn[i]);
                    continue;
                }
            }*/

            return txInAddressesClass;
        }

        /// <summary>
        /// Submits a transaction via Blockfrost and/or Koios to the Chain
        /// </summary>
        /// <param name="db"></param>
        /// <param name="tx"></param>
        /// <param name="res"></param>
        /// <returns></returns>
        private static string SubmitTransaction(EasynftprojectsContext db, CardanoSharp.Wallet.Models.Transactions.Transaction tx, ref BuildTransactionClass res)
        {
            if (tx==null)
                return "ERROR";
            var signedTx = tx.Serialize();
            var s = Convert.ToHexString(signedTx);
            var json = JsonConvert.SerializeObject(tx, formatting:Formatting.Indented);

            // Always send to blockfrost
          //  var ok2 = BlockfrostFunctions.SubmitTransaction(signedTx);
            var ok2 = MaestroFunctions.SubmitTurboTransaction(signedTx);
            if (!ok2.Success)
            {
                var ok1 = KoiosFunctions.SubmitTransactionViaKoios(signedTx);
                if (!ok1.Success)
                {
                    return "ERROR";
                }
                else
                {
                    res.TxHash = ok1.TxHash.Replace("\"", "");
                    return "OK";
                }
            }
            else
            {
                KoiosFunctions.SubmitTransactionViaKoios(signedTx);
            }

            if (!string.IsNullOrEmpty(ok2.TxHash))
                res.TxHash = ok2.TxHash.Replace("\"", "");

            return "OK";
        }


        private static IAuxiliaryDataBuilder SetMetadata(BuildTransactionClass buildtransaction, string metadata)
        {
            // Find the Metadata types (721 and 20)
            var the721content = ConsoleCommand.GetThe721Content(metadata);
            var the20content = ConsoleCommand.GetThe20Content(metadata);
            var auxData = AuxiliaryDataBuilder.Create;
            if (!string.IsNullOrEmpty(the721content) && the721content!="null")
                auxData.AddMetadata(721, the721content);
            if (!string.IsNullOrEmpty(the20content) && the20content!="null")
                auxData.AddMetadata(20, the20content);
            buildtransaction.LogFile += metadata + Environment.NewLine;
            return auxData;
        }


        private static long GetSendbackToUser(IConnectionMultiplexer redis, MultipleTokensClass[] nft, string receiveraddress,
            bool mainnet, Nftproject project, ref BuildTransactionClass buildtransaction, long sendbackToUser,
            MintingCostsClass mintingcosts, int ux, string sendtoken)
        {
            if (string.IsNullOrEmpty(project.Minutxo) || project.Minutxo == nameof(MinUtxoTypes.twoadaeverynft))
            {
                sendbackToUser = mintingcosts.MinUtxo * nft.Length;
            }

            if (project.Minutxo == nameof(MinUtxoTypes.twoadaall5nft))
            {
                foreach (var tok in nft)
                {
                    ux++;
                    if (ux >= 5)
                    {
                        ux = 0;
                        sendbackToUser += mintingcosts.MinUtxo;
                    }
                }
            }

            if (project.Minutxo == nameof(MinUtxoTypes.minutxo))
            {
                sendbackToUser =
                    ConsoleCommand.CalculateRequiredMinUtxo(redis, receiveraddress, sendtoken, "", new Guid().ToString("N"),
                        mainnet, ref buildtransaction);
            }

            return sendbackToUser;
        }

        public static CreateNewPaymentAddressClass CreateNewPaymentAddress(bool mainnet, bool enterpriseaddress = true)
        {
            // Restore a Mnemonic
            IMnemonicService service = new MnemonicService();
            Mnemonic rememberMe = service.Generate(24, WordLists.English);

            return CreateAddress(mainnet, enterpriseaddress, rememberMe);

        }

        private static CreateNewPaymentAddressClass CreateAddress(bool mainnet, bool enterpriseaddress, Mnemonic rememberMe)
        {
            CreateNewPaymentAddressClass cnpac = new CreateNewPaymentAddressClass();
            PrivateKey rootKey = rememberMe.GetRootKey();

            // This path will give us our Payment Key on index 0
            string paymentPath = $"m/1852'/1815'/0'/0/0";
            // The paymentPrv is Private Key of the specified path.
            PrivateKey paymentPrv = rootKey.Derive(paymentPath);
            // Get the Public Key from the Private Key
            PublicKey paymentPub = paymentPrv.GetPublicKey(false);

            // This path will give us our Stake Key on index 0
            string stakePath = $"m/1852'/1815'/0'/2/0";
            // The stakePrv is Private Key of the specified path
            PrivateKey stakePrv = rootKey.Derive(stakePath);
            // Get the Public Key from the Stake Private Key
            PublicKey stakePub = stakePrv.GetPublicKey(false);


            var address = !enterpriseaddress
                ? AddressUtility.GetBaseAddress(paymentPub, stakePub, mainnet ? NetworkType.Mainnet : NetworkType.Preprod)
                : AddressUtility.GetEnterpriseAddress(paymentPub, mainnet ? NetworkType.Mainnet : NetworkType.Preprod);


            cnpac.SeedPhrase = rememberMe.Words;
            cnpac.Address = address.ToString();
            cnpac.privateskey = GetKeyJson(paymentPrv.Key.ToStringHex(), "Payment Signing Key", "PaymentExtendedSigningKeyShelley_ed25519_bip32");
            cnpac.privatevkey = GetKeyJson(paymentPub.Key.ToStringHex(), "Payment Verification Key", "PaymentExtendedVerificationKeyShelley_ed25519_bip32");
            cnpac.stakeskey = GetKeyJson(stakePrv.Key.ToStringHex(), "Stake Signing Key","");
            cnpac.stakevkey = GetKeyJson(stakePub.Key.ToStringHex(), "Stake Verification Key","");
            cnpac.pkh = HashUtility.Blake2b224(paymentPub.Key);

            return cnpac;
        }

        public static string SendAllAdaRoyalitySplit(EasynftprojectsContext db, IConnectionMultiplexer redis, TxInAddressesClass utxo, Splitroyaltyaddress address, bool mainnet, int maxTxIn, out List<TxOutClass> txouts, ref BuildTransactionClass buildtransaction)
        {
            string guid = GlobalFunctions.GetGuid();
            txouts = new List<TxOutClass>();

            string matxrawfile = $"{GeneralConfigurationClass.TempFilePath}matx{guid}.raw";
            string matxsignedfile = $"{GeneralConfigurationClass.TempFilePath}matx{guid}.signed";
            string signingkeyfile = $"{GeneralConfigurationClass.TempFilePath}signing{guid}.skey";

            var q = ConsoleCommand.GetQueryTip();
            if (q == null)
                return "Error while getting Query Tip";

            long ttl = (long)q.Slot + 50000;

            string selleraddress = address.Splitroyaltyaddressessplits.FirstOrDefault(x => x.IsMainReceiver == true)!.Address;
            utxo = CardanoSharpFunctions.GetMaxTx(utxo, maxTxIn);
            long restofada = utxo.LovelaceSummary;

            foreach (var splitroyaltyaddressessplit in address.Splitroyaltyaddressessplits)
            {
                if (splitroyaltyaddressessplit.IsMainReceiver == true)
                    continue;

                if (splitroyaltyaddressessplit.State != "active")
                    continue;

                if (splitroyaltyaddressessplit.Activefrom != null && splitroyaltyaddressessplit.Activefrom > DateTime.Now)
                    continue;

                if (splitroyaltyaddressessplit.Activeto != null && splitroyaltyaddressessplit.Activeto < DateTime.Now)
                    continue;

                long lovelace = Math.Max(1000000, utxo.LovelaceSummary / 100 * (splitroyaltyaddressessplit.Percentage / 100));
                txouts.Add(new TxOutClass() { Amount = lovelace, ReceiverAddress = splitroyaltyaddressessplit.Address, Percentage = splitroyaltyaddressessplit.Percentage });
                restofada -= lovelace;
            }


            // Commision for NMKR
            var txouts1 = new List<TxOutClass>();
            /*  long lovelacenmkr = address.Customer.Defaultsettings.Mintingcosts;
                       if (lovelacenmkr!=0)
                          txouts1.Add(new TxOutClass() { Lovelace = lovelacenmkr, ReceiverAddress = address.Customer.Defaultsettings.Mintingaddress });
                     */


            var txBody = TransactionBodyBuilder.Create;
            foreach (var txin in utxo.TxIn.OrEmptyIfNull())
            {
                txBody.AddInput(txin.TxHash, (uint)txin.TxId);
            }
            foreach (var txout in txouts1)
            {
                txBody.AddOutput(new Address(txout.ReceiverAddress), (ulong)txout.Amount);
            }
            foreach (var txout in txouts)
            {
                txBody.AddOutput(new Address(txout.ReceiverAddress), (ulong)txout.Amount);
            }
            txBody.AddOutput(new Address(selleraddress), (ulong)restofada);

            txBody.SetTtl((uint)((q.Slot ?? 0) + 3600))
                .SetFee(0)
                .Build();


            var skey = Encryption.DecryptString(address.Skey, address.Salt);
            var vkey = Encryption.DecryptString(address.Vkey, address.Salt);


            var witnesses = TransactionWitnessSetBuilder.Create
                .AddVKeyWitness(new PublicKey(Convert.FromHexString(GetKeyFromCbor(vkey)), new byte[] { }), new PrivateKey(Convert.FromHexString(GetKeyFromCbor(skey)), new byte[] { }));

            var txBuilder = TransactionBuilder.Create
                .SetBody(txBody)
                .SetWitnesses(witnesses);

            var signedTx = BuildAndCalculateFees(redis, mainnet, txBuilder, txBody, ref buildtransaction);
            return SubmitTransaction(db, signedTx, ref buildtransaction);
        }

        public static ITransactionBodyBuilder? CreateTransaction(CoinSelection csCoinSelection,
            CreateManagedWalletTransactionClass transaction, Querytip q)
        {
            var txBody = TransactionBodyBuilder.Create;
            foreach (var txin in csCoinSelection.Inputs)
            {
                txBody.AddInput(txin.TransactionId, txin.TransactionIndex);
            }

            foreach (var output in transaction.Receivers)
            {
                var token = TokenBundleBuilder.Create;
                foreach (var nativeAsset in output.SendTokens.OrEmptyIfNull())
                {
                    var policyid = GlobalFunctions.ConvertHexStringToByteArray(nativeAsset.PolicyId);
                    var asset = GlobalFunctions.ConvertHexStringToByteArray(nativeAsset.AssetNameInHex);
                    token.AddToken(policyid, asset, nativeAsset.Quantity);
                }
                txBody.AddOutput(new Address(output.ReceiverAddress), (ulong)output.ReceiverLovelace, token);
            }

            // Calcuate Change
            var changetoken = TokenBundleBuilder.Create;
            foreach (var selectedUtxo in csCoinSelection.SelectedUtxos)
            {
                foreach (var balanceAsset in selectedUtxo.Balance.Assets.OrEmptyIfNull())
                {
                    var policyid = balanceAsset.PolicyId;
                    var asset = balanceAsset.Name;
                    var amount = balanceAsset.Quantity;

                    long sendmount = FindSendAmount(transaction.Receivers, policyid, asset);
                    if (amount - sendmount > 0)
                    {
                        changetoken.AddToken(GlobalFunctions.ConvertHexStringToByteArray(policyid), GlobalFunctions.ConvertHexStringToByteArray(asset), amount - sendmount);
                    }
                }
            }

            if (csCoinSelection.ChangeOutputs.Any())
                txBody.AddOutput(csCoinSelection.ChangeOutputs[0].Address, csCoinSelection.ChangeOutputs[0].Value.Coin,
                    changetoken);

            txBody.SetTtl((uint)((q.Slot ?? 0) + 3600))
                .SetFee(0)
                .Build();

            return txBody;
        }
        public static TransactionOutputValue TransactionOutputValue(long lovelace, string tokenpolicyid, string tokenname, long tokencount)
        {
            TransactionOutputValue toutput = new TransactionOutputValue() {Coin = (ulong) lovelace};

            Dictionary<byte[], NativeAsset> assets = new Dictionary<byte[], NativeAsset>();

            assets.Add(tokenpolicyid.HexToByteArray(), new NativeAsset()
            {
                Token = new Dictionary<byte[], long>()
                {
                    {tokenname.ToHex().HexToByteArray(), tokencount}
                }
            });

            toutput.MultiAsset = assets;

            return toutput;
        }
        public static IEnumerable<TransactionOutput> GetOutput(string receiver, VestTokensClass vestTokens)
        {
            List<TransactionOutput> toutput = new List<TransactionOutput>();

            Dictionary<byte[], NativeAsset> assets = new Dictionary<byte[], NativeAsset>();

            foreach (var tokensBaseClass in vestTokens.Tokens.OrEmptyIfNull())
            {
                assets.Add(tokensBaseClass.PolicyId.HexToByteArray(), new NativeAsset()
                {
                    Token = new Dictionary<byte[], long>()
                    {
                        {tokensBaseClass.AssetNameInHex.HexToByteArray(), tokensBaseClass.CountToken}
                    }
                });
            }

            var t1 = new TransactionOutput()
            {
                Address = new Address(receiver).GetBytes(),
                Value = new TransactionOutputValue()
                {
                    Coin = (ulong)(vestTokens.Lovelace??2000000),
                    MultiAsset = assets,
                }
            };
            toutput.Add(t1);


            return toutput;
        }
        public static string SendTransaction(EasynftprojectsContext db, IConnectionMultiplexer redis, CoinSelection csCoinSelection, CreateManagedWalletTransactionClass transaction, string senderaddress, string skey, string vkey, bool mainnet, ref BuildTransactionClass bt)
        {
            var q = ConsoleCommand.GetQueryTip();
            if (q == null)
                return "Error while getting Query Tip";


            var txBody = CreateTransaction(csCoinSelection, transaction, q);

            var witnesses = TransactionWitnessSetBuilder.Create
                .AddVKeyWitness(new PublicKey(Convert.FromHexString(GetKeyFromCbor(vkey)), new byte[] { }), new PrivateKey(Convert.FromHexString(GetKeyFromCbor(skey)), new byte[] { }));

            var txBuilder = TransactionBuilder.Create
                .SetBody(txBody)
                .SetWitnesses(witnesses);

            var signedTx = BuildAndCalculateFees(redis, mainnet, txBuilder, txBody, ref bt);
            GlobalFunctions.LogMessage(db, $"Build Custodial Wallet {senderaddress} Transaction 1",
                JsonConvert.SerializeObject(txBuilder,
                    new JsonSerializerSettings() {ReferenceLoopHandling = ReferenceLoopHandling.Ignore}));
            GlobalFunctions.LogMessage(db, $"Build Custodial Wallet {senderaddress} Transaction 2",
                JsonConvert.SerializeObject(txBody,
                    new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
            GlobalFunctions.LogMessage(db, $"Build Custodial Wallet {senderaddress} SignedTx",
                JsonConvert.SerializeObject(signedTx,
                    new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
            return SubmitTransaction(db, signedTx, ref bt);
        }

        private static long FindSendAmount(TransactionReceiversClass[] transactionReceivers, string policyid, string asset)
        {
            return (from output in transactionReceivers
                from nativeAsset in output.SendTokens.OrEmptyIfNull()
                let policyid1 = nativeAsset.PolicyId
                let asset1 = nativeAsset.AssetNameInHex
                where policyid == policyid1 && asset1 == asset
                select nativeAsset.Quantity).Sum();
        }

        public static CreateNewPaymentAddressClass ImportPaymentAddress(bool isMainnet, ImportManagedWalletClass managedWalletClass)
        {
            // Restore a Mnemonic
            IMnemonicService service = new MnemonicService();
            try
            {
                string seed = string.Join(" ", managedWalletClass.SeedWords);
                Mnemonic rememberMe = service.Restore(seed, WordLists.English);
                return CreateAddress(isMainnet, managedWalletClass.EnterpriseAddress, rememberMe);
            }
            catch (Exception e)
            {
                return new CreateNewPaymentAddressClass() {ErrorCode = 4491, ErrorMessage = e.Message};
            }
        }
        public static async Task<CreateDecentralPaymentByCslResultClass> CreateDecentralPaymentByCardanoSharp(
            CreateMintAndSendParametersClass cmaspc, EasynftprojectsContext db, IConnectionMultiplexer redis,
            bool mainnet)
        {
            CreateDecentralPaymentByCslResultClass result = new CreateDecentralPaymentByCslResultClass();
            var cctc = ConsoleCommand.CreateCslTransactionClass(cmaspc, db, redis, mainnet);

            var transactionBody = TransactionBodyBuilder.Create;
            foreach (var txIn in cctc.TxIns.OrEmptyIfNull())
            {
                transactionBody.AddInput(txIn.TransactionHash, txIn.TransactionIndex);
            }

            foreach (var txOut in cctc.TxOuts.OrEmptyIfNull())
            {
                if (txOut.Tokens == null)
                {
                    transactionBody.AddOutput(new Address(txOut.AddressBech32), (ulong)(txOut.Lovelace??0));
                }
                else
                {
                    var tokenBundle = TokenBundleBuilder.Create;
                    foreach (var token in txOut.Tokens)
                    {
                        tokenBundle.AddToken(token.PolicyId.HexToByteArray(), token.TokenName.HexToByteArray(), token.Count??0);
                    }

                    transactionBody.AddOutput(new Address(txOut.AddressBech32), (ulong)(txOut.Lovelace ?? 0), tokenBundle);
                }
            }

            // Set Witnesses;
            var witnessSet = TransactionWitnessSetBuilder.Create;

            // Minting
            var tokenBundleBuilder = TokenBundleBuilder.Create;
            foreach (var mint in cctc.Mints.OrEmptyIfNull())
            {
                tokenBundleBuilder.AddToken(
                    GlobalFunctions.ConvertHexStringToByteArray(mint.PolicyId), 
                    GlobalFunctions.ConvertHexStringToByteArray(mint.TokenName), mint.Count ?? 1);
                var policyclass = GetPkhFromPolicyscript(mint.PolicyScriptJson);

                var policyScript = ScriptAllBuilder.Create
                    .SetScript(NativeScriptBuilder.Create.SetKeyHash(GlobalFunctions.ConvertHexStringToByteArray(policyclass.Pkh)));

                if (policyclass.Slot != null)
                    policyScript.SetScript(NativeScriptBuilder.Create.SetInvalidAfter((uint)(policyclass.Slot??0)));

                witnessSet.AddScriptAllNativeScript(policyScript);
            }

          //  witnessSet.MockVKeyWitness(2);
          var auxDataBuilder = ConvertMetadata(cctc); 

            // Sending to Base Address, includes 100 ADA and the Token we are minting
            transactionBody
                .SetMint(tokenBundleBuilder)
                .SetTtl((uint)(cctc.Ttl??0))
                .SetFee((ulong)(cctc.Fees??0))
                .Build();

            /*
            witnessSet
                .AddVKeyWitness(scriptPubKey, scriptPrivKey)
                .AddVKeyWitness(utxoPubKey, utxoPrivKey);
            */

            var transactionBuilder = TransactionBuilder.Create
                .SetBody(transactionBody)
                .SetWitnesses(witnessSet)
                .SetAuxData(auxDataBuilder);

            var transaction = transactionBuilder.Build();

            // calculate and update transaction fee
            
            if (cctc.Fees == null || cctc.Fees==0)
            {
                cctc.Fees = transaction.CalculateAndSetFee(numberOfVKeyWitnessesToMock: 3);
            }

            transaction = transactionBuilder.Build();
            
            Cbor c = new Cbor() { cbor = transaction.Serialize().ToStringHex(), calculatedOrSetFee = cctc.Fees??0 };
            result.CslResult = JsonConvert.SerializeObject(c);
            
            return result;
        }

        private static IAuxiliaryDataBuilder ConvertMetadata(CslCreateTransactionClass cctc)
        {
            // Does not work actually - we need an other approach
            var auxDataBuilder = AuxiliaryDataBuilder.Create;
            foreach (var metadatum in cctc.Metadata)
            {
               
                JObject jObject = JObject.Parse(metadatum.Json);
                string policyId = jObject.Properties().First().Name;
                JObject firstElement = (JObject)jObject[policyId];
                if (firstElement != null)
                {
                    string tokenName = firstElement.Properties().First().Name;
                    JObject secondElementContent = (JObject)firstElement[tokenName];
                    var mt = secondElementContent.ToString();
                    var tokenMeta = new Dictionary<string, object>
                    {
                        {
                            tokenName, deserializeToDictionary(mt)
                        }
                    };

                    var policyMeta = new Dictionary<string, object>
                    {
                        { policyId, tokenMeta },
                        { "version", "1.0" }
                    };

                    auxDataBuilder.AddMetadata(Convert.ToInt32(metadatum.Key), policyMeta);
                }
            }

            return auxDataBuilder;
        }
        private static Dictionary<string, object> deserializeToDictionary(string jo)
        {
            var values = JsonConvert.DeserializeObject<Dictionary<string, object>>(jo);
            var values2 = new Dictionary<string, object>();
            foreach (KeyValuePair<string, object> d in values)
            {
                // if (d.Value.GetType().FullName.Contains("Newtonsoft.Json.Linq.JObject"))
                if (d.Value is JObject)
                {
                    values2.Add(d.Key, deserializeToDictionary(d.Value.ToString()));
                }
                else
                {
                    values2.Add(d.Key, d.Value);
                }
            }
            return values2;
        }
        private static string RemoveFirstAndLastChar(string metadatumJson)
        {
            // Trims all and then Remove first and last character when the first is { and the last is }
            return metadatumJson.Trim().TrimStart('{').TrimEnd('}');
        }

        private static PolicyScriptClass GetPkhFromPolicyscript(string policyScriptJson)
        {
            var res = new PolicyScriptClass();

            PolicyScript ps = JsonConvert.DeserializeObject<PolicyScript>(policyScriptJson);
            if (ps is { Scripts: not null } && ps.Scripts.Any())
            {
                var t = ps.Scripts.Find(x => x.Type == "before");
                if (t != null)
                {
                    res.Slot=t.Slot;
                }
                var t1 = ps.Scripts.Find(x => x.Type == "sig");
                if (t1 != null)
                {
                    res.Pkh = t1.KeyHash;
                }
            }

            return res;
        }
    }

    public class PolicyScriptClass
    {
        public string Pkh { get; set; }
        public long? Slot { get; set; }
    }

    public class CumulateCardanosharpTokens
    {
        public string PolicyId { get; set; }
        public string Asset { get; set; }
        public long Quantity { get; set; }
    }
}
