using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NMKR.Shared.Classes;
using NMKR.Shared.Classes.CardanoSerialisationLibClasses;
using NMKR.Shared.Model;
using StackExchange.Redis;
using Guid = System.Guid;
using Metadatum = NMKR.Shared.Classes.CardanoSerialisationLibClasses.Metadatum;
using Token = NMKR.Shared.Classes.CardanoSerialisationLibClasses.Token;

namespace NMKR.Shared.Functions.Extensions
{
    public static class CliCommandExtensions
    {
        public static string GetTxOut(this String str, TxInAddressesClass[] utxo, string receiveraddress,
          long? donotincludeQuantity, string donotincludeTokenname, long adadiff, long fee, ref BuildTransactionClass bt)
        {
            // If we use changeaddress, we have to set the fee to 3 ADA - so, 2.xxx ADA changeaddress + fee

            if (utxo == null)
                return str;

            string tokens = GetTokensFromUtxo(utxo, donotincludeQuantity, donotincludeTokenname);

            ConsoleCommand.GetTxInHashes(utxo, out var com1, out var txincount, out var lovelacesummery, ref bt);

            long rest = lovelacesummery - adadiff - fee;
            string com2 = "";
            if (rest > 0)
            {
                com2 = !string.IsNullOrEmpty(tokens) ? $" --tx-out \"{receiveraddress.FilterToLetterOrDigit()} + {rest} lovelace + {tokens}\"" : $" --tx-out \"{receiveraddress} + {rest} lovelace\"";
            }

            return str + com2;
        }

        public static string GetTxOutWithTokensMinutxo(this String str, IConnectionMultiplexer redis, TxInAddressesClass[] utxo, string receiveraddress,
            long? donotincludeQuantity, string donotincludeTokenname, ref BuildTransactionClass bt)
        {
            if (utxo == null)
                return str;

            string tokens = GetTokensFromUtxo(utxo, donotincludeQuantity, donotincludeTokenname);

            ConsoleCommand.GetTxInHashes(utxo, out var com1, out var txincount, out var lovelacesummery, ref bt);

            if (!string.IsNullOrEmpty(tokens))
            {
                var rest = ConsoleCommand.CalculateRequiredMinUtxo(redis, receiveraddress, tokens, "",
                    GlobalFunctions.GetGuid(), GlobalFunctions.IsMainnet(),
                    ref bt);

                var com2 = $" --tx-out \"{receiveraddress.FilterToLetterOrDigit()} + {rest} lovelace + {tokens}\"";
                return str + com2;
            }

            return str;
        }


        public static string GetTxOutRestWithTokens(this String str, IConnectionMultiplexer redis, TxInAddressesClass[] utxo, string receiveraddress,
            long? donotincludeQuantity, string donotincludeTokenname, long lovelace, ref BuildTransactionClass bt)
        {
            if (utxo == null)
                return str;

            string tokens = GetTokensFromUtxo(utxo, donotincludeQuantity, donotincludeTokenname);

            ConsoleCommand.GetTxInHashes(utxo, out var com1, out var txincount, out var lovelacesummery, ref bt);

            if (!string.IsNullOrEmpty(tokens))
            {
                var com2 = $" --tx-out \"{receiveraddress.FilterToLetterOrDigit()} + {lovelace} lovelace + {tokens}\"";
                return str + com2;
            }
            else
            {
                var com2 = $" --tx-out \"{receiveraddress.FilterToLetterOrDigit()} + {lovelace} lovelace\"";
                return str + com2;
            }
        }


        private static string GetTokensFromUtxo(TxInAddressesClass[] utxo, long? donotincludeQuantity, string donotincludeTokenname)
        {
            string tokens = "";
            long quant = 0;

            foreach (var adr in utxo)
            {
                if (adr.TxIn == null)
                    continue;
                foreach (var txInClass in adr.TxIn)
                {
                    if (txInClass.Tokens == null || !txInClass.Tokens.Any())
                        continue;

                    foreach (var token in txInClass.Tokens)
                    {
                        long quant1;
                        if (donotincludeQuantity == null || string.IsNullOrEmpty(donotincludeTokenname))
                        {
                            quant1 = token.Quantity;
                        }
                        else
                        {

                            if (donotincludeTokenname == $"{token.PolicyId}.{token.TokennameHex}" || donotincludeTokenname == $"{token.PolicyId}.{token.Tokenname}" || donotincludeTokenname == $"{token.PolicyId}{token.TokennameHex}")
                            {
                                if (donotincludeQuantity - quant >= token.Quantity)
                                {
                                    quant += token.Quantity;
                                    continue;
                                }
                                else
                                {
                                    quant1 = token.Quantity - ((long)donotincludeQuantity - quant);
                                }
                            }
                            else quant1 = token.Quantity;
                        }

                        if (!string.IsNullOrEmpty(tokens))
                            tokens += " + ";
                        tokens += $"{quant1} {token.PolicyId}.{token.TokennameHex}";
                    }
                }
            }

            return tokens;
        }

        private static string GetTokensFromTxins(TxInClass[] txins)
        {
            string tokens = "";

            foreach (var txInClass in txins)
            {
                if (txInClass.Tokens == null || !txInClass.Tokens.Any())
                    continue;

                foreach (var token in txInClass.Tokens)
                {
                    long quant1;
                    quant1 = token.Quantity;

                    if (!string.IsNullOrEmpty(tokens))
                        tokens += " + ";
                    tokens += $"{quant1} {token.PolicyId}.{token.TokennameHex}";
                }
            }

            return tokens;
        }


        public static string GetTxInExecutionUnits(this String str, long plutusrequiredtime, long plutusrequiredspace)
        {
            return $"{str} --tx-in-execution-units \"({plutusrequiredtime}, {plutusrequiredspace})\"";
        }


        public static string GetTxOut(this String str, TxInAddressesClass[] utxo, string receiveraddress,
         long? donotincludeQuantity, string donotincludeTokenname, long adadiff, string minttokens, ref BuildTransactionClass bt)
        {
            long
                fee = 3000000; // We neded for the network fees and the minutxo - so approx. 2 ada - minutxo and the rest will go to the change-address 

            long quant = 0;
            long quant1 = 0;
            string tokens = "";
            foreach (var adr in utxo)
            {
                if (adr.TxIn == null)
                    continue;
                foreach (var txInClass in adr.TxIn)
                {
                    if (txInClass.Tokens == null || !txInClass.Tokens.Any())
                        continue;

                    foreach (var token in txInClass.Tokens)
                    {
                        if (donotincludeQuantity == null || string.IsNullOrEmpty(donotincludeTokenname))
                        {
                            quant1 = token.Quantity;
                        }
                        else
                        {

                            if (donotincludeTokenname == $"{token.PolicyId}.{token.TokennameHex}")
                            {
                                if (donotincludeQuantity - quant >= token.Quantity)
                                {
                                    quant += token.Quantity;
                                    continue;
                                }
                                else
                                {
                                    quant1 = token.Quantity - ((long)donotincludeQuantity - quant);
                                }
                            }
                            else quant1 = token.Quantity;
                        }

                        if (!string.IsNullOrEmpty(tokens))
                            tokens += " + ";
                        tokens += $"{quant1} {token.PolicyId}.{token.TokennameHex}";
                    }
                }
            }

            ConsoleCommand.GetTxInHashes(utxo, out var com1, out var txincount, out var lovelacesummery, ref bt);

            long rest = lovelacesummery - adadiff - fee;
            string com2 = "";
            if (rest > 0)
            {
                com2 = !string.IsNullOrEmpty(tokens) ?
                    $" --tx-out \"{receiveraddress.FilterToLetterOrDigit()}+{rest} lovelace +{minttokens}+{tokens}\"" :
                    $" --tx-out \"{receiveraddress.FilterToLetterOrDigit()}+{rest} lovelace +{minttokens}\"";
            }

            return str + com2;
        }

        public static string GetTxOut(this String str, Nftprojectsadditionalpayout[] additionalpayouts, long hastopay, long nftcount)
        {
            if (additionalpayouts == null)
                return str;
            if (!additionalpayouts.Any())
                return str;
            string command = "";
            foreach (var nftprojectsadditionalpayout in additionalpayouts)
            {
                long addvalue = ConsoleCommand.GetAdditionalPayoutwalletsValue(nftprojectsadditionalpayout, hastopay, nftcount);
                if (addvalue > 0)
                {
                    command += $" --tx-out {nftprojectsadditionalpayout.Wallet.Walletaddress.FilterToLetterOrDigit()}+{addvalue}";
                }
            }

            return str + command;
        }
        public static string GetTxOut(this String str, Lockedasset lockedasset, long fee)
        {
            long lovelace = lockedasset.Lovelace - fee;

            string tokens = "";
            foreach (var asset in lockedasset.Lockedassetstokens)
            {
                tokens += " + ";
                tokens += asset.Count + " " + asset.Policyid + "." + asset.Tokennameinhex;
            }

            return $"{str} --tx-out \"{lockedasset.Changeaddress.FilterToLetterOrDigit()}+{lovelace} lovelace{tokens}\"";
        }
        public static string GetTxOut(this String str, string receiveraddress, long lovelace)
        {
            if (lovelace <= 0)
                return str;

            return $"{str} --tx-out {receiveraddress.FilterToLetterOrDigit()}+{lovelace}";
        }
        public static string GetTxOut(this String str, string receiveraddress, long lovelace, string tokens)
        {
            if (lovelace <= 0)
                return str;

            return $"{str} --tx-out {receiveraddress.FilterToLetterOrDigit()}+{lovelace}+\"{tokens}\"";
        }
        public static string GetTxOut(this String str, string receiveraddress, string tokens)
        {
            if (string.IsNullOrEmpty(tokens))
                return $"{str} --tx-out {receiveraddress.FilterToLetterOrDigit()}+0";
            return $"{str} --tx-out {receiveraddress.FilterToLetterOrDigit()}+0+\"{tokens}\"";
        }

        public static string GetTxOutInlineDatumFile(this String str, string inlinedatumFile)
        {
            if (string.IsNullOrEmpty(inlinedatumFile))
                return str;
            if (!File.Exists(inlinedatumFile))
                return str;
            return $"{str} --tx-out-inline-datum-file {inlinedatumFile}";
        }
        public static string GetTxOut(this String str, SmartContractsPayoutsClass[] receivers)
        {
            if (receivers == null)
                return str;

            string st = "";
            foreach (var receiver in receivers)
            {
                if (receiver is not { lovelace: > 0 })
                    continue;

                st += string.IsNullOrEmpty(receiver.tokens)
                    ? $" --tx-out \"{receiver.address.FilterToLetterOrDigit()} + {receiver.lovelace} lovelace\""
                    : $" --tx-out \"{receiver.address.FilterToLetterOrDigit()} + {receiver.lovelace} lovelace + {receiver.tokens}\"";
            }

            return str + st;
        }

        public static string GetTxOutCip68(this String str, TxOut[] txouts, string referenceaddress, IConnectionMultiplexer redis)
        {
            if (txouts == null || !txouts.Any())
                return str;

            string txoutstr = "";
            long mintingcosts = 0;
            foreach (var txout in txouts)
            {
                string txoutstrtoken = "";
                string tokensstr = "";

                if (txout.Tokens != null && txout.Tokens.Any())
                {
                    foreach (var token in txout.Tokens)
                    {
                        if (!string.IsNullOrEmpty(tokensstr))
                            tokensstr += "+";
                        tokensstr +=
                            $"{token.Count} {token.PolicyId}.{ConsoleCommand.CreateMintTokenname("", token.TokenName.FromHex(), ConsoleCommand.Cip68Type.NftUserToken)}";
                    }

                    if (!string.IsNullOrEmpty(tokensstr))
                    {
                        txoutstrtoken += $"+\"{tokensstr}\"";
                    }


                    foreach (var token in txout.Tokens)
                    {
                        if (string.IsNullOrEmpty(token.Metadata)) continue;

                        BuildTransactionClass bt = new BuildTransactionClass();
                        string sendToken =
                            $"1 {token.PolicyId}.{ConsoleCommand.CreateMintTokenname("", token.TokenName.FromHex(), ConsoleCommand.Cip68Type.ReferenceToken)}";

                        string filename = $"{GeneralConfigurationClass.TempFilePath}Datum_{Guid.NewGuid():N}.json";
                        File.WriteAllText(filename, token.Metadata);

                        // Add the Reference Tokens
                        var minUtxo = txout.Lovelace == 0
                            ? 0
                            : ConsoleCommand.CalculateRequiredMinUtxo(redis, referenceaddress, sendToken,
                                filename, GlobalFunctions.GetGuid(), GlobalFunctions.IsMainnet(), ref bt);
                        mintingcosts += minUtxo;

                        txoutstr += $" --tx-out {referenceaddress}+{minUtxo}+\"{sendToken}\"";
                        txoutstr += $" --tx-out-inline-datum-file {filename}";
                    }
                }
                // The last txout must be the seller because of the mintingcosts, which are not clear before - and the buyer has to pay the annouced price
                if (txout.ReduceHereTheMintingcosts)
                {
                    string txoutstrtmp = $" --tx-out {txout.AddressBech32}+{txout.Lovelace - mintingcosts}";
                    txoutstr += txoutstrtmp + txoutstrtoken;
                    mintingcosts = 0;
                }
                else
                {
                    string txoutstrtmp = $" --tx-out {txout.AddressBech32}+{txout.Lovelace}";
                    txoutstr += txoutstrtmp + txoutstrtoken;
                }


            }

            return $"{str} {txoutstr}";
        }

        public static string GetRequiredSignerHash(this String str, string signerhash)
        {
            if (string.IsNullOrEmpty(signerhash))
                return str;
            return $"{str} --required-signer-hash {signerhash}";
        }
        public static string GetInvalidBefore(this String str, long slot)
        {
            return $"{str} --invalid-before {slot}";
        }
        public static string GetInvalidHereAfter(this String str, long slot)
        {
            return $"{str} --invalid-hereafter {slot}";
        }
        public static string GetTxOutDatumEmbedFile(this String str, string datunfile)
        {
            if (!File.Exists(datunfile))
                return str;

            return $"{str} --tx-out-datum-embed-file {datunfile}";
        }
        public static string GetTxOut(this String str, CslCreateTransactionClass cctc)
        {
            string s = "";
            foreach (var receiver in cctc.TxOuts.OrEmptyIfNull())
            {
                s += $" --tx-out \"{receiver.AddressBech32.FilterToLetterOrDigit()} + {receiver.Lovelace} lovelace";
                foreach (var token in receiver.Tokens.OrEmptyIfNull())
                {
                    s+=$" +{token.Count} {token.PolicyId}.{token.TokenName}";
                }
                s += "\"";
            }

            return str + s;
        }
        public static string GetTxOut(this String str, TxOutClass[] txouts)
        {
            if (txouts == null)
                return str;

            string s = "";
            foreach (var receiver in txouts)
            {
                s += $" --tx-out \"{receiver.ReceiverAddress.FilterToLetterOrDigit()} + {receiver.Amount} lovelace\"";
            }

            return str + s;
        }
        public static string GetTxInCollateral(this String str, string collateral)
        {
            if (string.IsNullOrEmpty(collateral))
                return str;

            string[] cols = collateral.Split(',');

            return $"{str} --tx-in-collateral {cols.First()}";
        }
        public static string GetTxInScriptFile(this String str, string scriptfile)
        {
            if (!File.Exists(scriptfile))
                return str;

            return $"{str} --tx-in-script-file {scriptfile}";
        }
        public static string GetTxInDatumFile(this String str, string datumfile)
        {
            if (!File.Exists(datumfile))
                return str;

            return $"{str} --tx-in-datum-file {datumfile}";
        }

        public static T ToEnum<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

       

        public static string GetTxInRedeemerFile(this String str, string redeemerfile)
        {
            if (!File.Exists(redeemerfile))
                return str;

            return $"{str} --tx-in-redeemer-file {redeemerfile}";
        }


        public static string GetTxIn(this String str, TxInAddressesClass[] utxo, ref BuildTransactionClass bt)
        {
            if (utxo == null)
                return str;

            ConsoleCommand.GetTxInHashes(utxo, out var command, out var txincount, out var lovelacesummery, ref bt);
            return str + command;
        }

        public static string GetTotalCollateral(this String str, long collateral, string optionalCollateralTxin = null)
        {
            // When we have a collateral, we dont need to specify the tx-total-collateral
            if (!string.IsNullOrEmpty(optionalCollateralTxin))
                return str;

            if (collateral == 0)
                return str;
            return str + $" --tx-total-collateral {collateral}";
        }

        public static string GetTxOutCollateral(this String str, string collateralChangeAddress, long collateralAmount, TxInAddressesClass[] utxo, string optionalCollateralTxin = null)
        {
            // When we have a collateral, we dont need to specify the tx-out-return
            if (!string.IsNullOrEmpty(optionalCollateralTxin))
                return str;

            if (string.IsNullOrEmpty(collateralChangeAddress))
                return str;

            if (utxo == null)
                return str;

            List<TxInClass> txins = new List<TxInClass>();
            int i = 0;
            foreach (var addressesClass in utxo)
            {
                if (addressesClass.TxIn == null)
                    continue;

                foreach (var txInClass in addressesClass.TxIn)
                {
                    i++;
                    txins.Add(txInClass);
                    if (i >= 3)
                        break;
                }

                if (i >= 3)
                    break;
            }

            string tokens = GetTokensFromTxins(txins.ToArray());
            if (!string.IsNullOrEmpty(tokens))
                tokens = " + " + tokens;
            return str + $" --tx-out-return-collateral \"{collateralChangeAddress.FilterToLetterOrDigit()} {txins.Sum(x => x.Lovelace) - collateralAmount} lovelace {tokens}\"";
        }
        public static string GetTxInCollateral(this String str, TxInAddressesClass[] utxo, string optionalcollateraltxin = null)
        {
            if (!string.IsNullOrEmpty(optionalcollateraltxin))
            {
                string[] coll = optionalcollateraltxin.Split(',');
                string res = str;
                foreach (var s in coll)
                {
                    res += $" --tx-in-collateral {s}";
                }

                return res;
            }

            if (utxo == null)
                return str;

            ConsoleCommand.GetTxInHashesCollateral(utxo, out var command);
            return str + command;
        }
        public static string GetTxIn(this String str, TxInAddressesClass utxo, ref BuildTransactionClass bt)
        {
            if (utxo == null)
                return str;

            ConsoleCommand.GetTxInHashes(new[] { utxo }, out var command, out var txincount, out var lovelacesummery, ref bt);
            return str + command;
        }
        public static string GetTxIn(this String str, TxInClass[] txins, ref BuildTransactionClass bt)
        {
            if (txins == null)
                return str;

            ConsoleCommand.GetTxInHashes(txins, out var command, out var txincount, out var lovelacesummery, ref bt);
            return str + command;
        }
        public static string GetTxIn(this String str, CslCreateTransactionClass cctc)
        {
            foreach (var txin in cctc.TxIns.OrEmptyIfNull())
            {
                str += $" --tx-in {txin.TransactionHashAndIndex}";
            }
            return str;
        }
        public static string GetTxIn(this String str, string txhashAndId)
        {
            if (string.IsNullOrEmpty(txhashAndId))
                return str;

            return $"{str} --tx-in {txhashAndId}";
        }
        public static string GetTxIn(this String str, TxIn[] txins)
        {
            if (txins == null || !txins.Any())
                return str;

            foreach (var txin in txins)
            {
                str += $" --tx-in {txin.TransactionHashAndIndex}";
            }

            return str;
        }

        public static string GetTxInExecutionUnits(this String str, long? memunits, long? timeunits)
        {
            if (memunits == null || timeunits == null)
                return str;

            return $"{str} --tx-in-execution-units \"({memunits}, {timeunits})\" ";
        }

        public static string GetScriptValid(this String str)
        {
            return $"{str} --script-valid";
        }

        public static string GetTxOutScripthash(this String str, string scripthash, long lovelave, long tokencount, string tokenname, long bidamount = 0)
        {
            string bid = "";
            if (bidamount != 0)
                bid = $" + {bidamount} lovelace";

            return $"{str} --tx-out \"{scripthash} + {lovelave} lovelace + {tokencount} {tokenname}{bid}\"";
        }
        public static string GetTxOutScripthash(this String str, string scripthash, long lovelave)
        {
            return $"{str} --tx-out \"{scripthash} {lovelave} lovelace \"";
        }
        public static string GetTxOutDatumHash(this String str, string datumhash)
        {
            if (string.IsNullOrEmpty(datumhash))
                return str;
            return $"{str} --tx-out-datum-hash {datumhash}";
        }
        public static string GetChangeAddress(this String str, string changeaddress)
        {
            if (string.IsNullOrEmpty(changeaddress))
                return str;
            return $"{str} --change-address {changeaddress}";
        }

        public static string GetJsonMetadataNoSchema(this String str, string metadatafíle)
        {
            if (string.IsNullOrEmpty(metadatafíle))
                return str;

            return $"{str} --json-metadata-no-schema";
        }

        public static string GetMint(this String str, string minttokens)
        {
            if (string.IsNullOrEmpty(minttokens))
                return str;

            return $"{str} --mint=\"{minttokens}\"";
        }
        public static string GetMint(this String str, CslCreateTransactionClass cctc)
        {
            string minttokensstr = "";

            foreach (var minttoken in cctc.Mints.OrEmptyIfNull())
            {
                if (!string.IsNullOrEmpty(minttokensstr))
                    minttokensstr += " + ";
                minttokensstr += minttoken.Count + " " + minttoken.PolicyId + "." + minttoken.TokenName;
            }

            return minttokensstr == "" ? str : $"{str} --mint=\"{minttokensstr}\"";
        }
        public static string GetMint(this String str, Token[] minttokens)
        {
            if (minttokens == null || !minttokens.Any())
                return str;

            string minttokensstr = "";

            foreach (var minttoken in minttokens)
            {
                if (!string.IsNullOrEmpty(minttokensstr))
                    minttokensstr += " + ";
                minttokensstr += minttoken.Count + " " + minttoken.PolicyId + "." + minttoken.TokenName.ToHex();
            }
            return $"{str} --mint=\"{minttokensstr}\"";
        }

        public static string GetMintCip68(this String str, Token[] minttokens)
        {
            if (minttokens == null || !minttokens.Any())
                return str;

            string minttokensstr = "";

            foreach (var minttoken in minttokens)
            {
                if (!string.IsNullOrEmpty(minttokensstr))
                    minttokensstr += " + ";
                minttokensstr += minttoken.Count + " " + minttoken.PolicyId + "." + ConsoleCommand.CreateMintTokenname("", minttoken.TokenName.FromHex(), ConsoleCommand.Cip68Type.NftUserToken);
                minttokensstr += " + 1 " + minttoken.PolicyId + "." + ConsoleCommand.CreateMintTokenname("", minttoken.TokenName.FromHex(), ConsoleCommand.Cip68Type.ReferenceToken);
            }
            return $"{str} --mint=\"{minttokensstr}\"";
        }
        public static string GetMintingScriptFile(this String str, CslCreateTransactionClass cctc)
        {
            string policystr = "";
            foreach (var txout in cctc.TxOuts.OrEmptyIfNull())
            {
                if (txout.Tokens == null || !txout.Tokens.Any()) continue;
                foreach (var token in txout.Tokens.GroupBy(x => x.PolicyScriptJson))
                {
                    if (string.IsNullOrEmpty(token.Key)) continue;
                    string filename = GeneralConfigurationClass.TempFilePath + GlobalFunctions.GetGuid() + ".json";
                    File.WriteAllText(filename, token.Key);
                    policystr += $" --minting-script-file {filename}";
                }
            }

            if (policystr=="")
                return str;

            return $"{str} {policystr}";
        }
        public static string GetMintingScriptFile(this String str, string policyfile)
        {
            if (string.IsNullOrEmpty(policyfile) || !File.Exists(policyfile))
                return str;

            return $"{str} --minting-script-file {policyfile}";
        }
        public static string GetMintingScriptFile(this String str, TxOut[] txouts)
        {
            if (txouts == null || !txouts.Any())
                return str;

            string policystr = "";
            foreach (var txout in txouts)
            {
                if (txout.Tokens == null || !txout.Tokens.Any()) continue;
                foreach (var token in txout.Tokens.GroupBy(x => x.PolicyScriptJson))
                {
                    if (string.IsNullOrEmpty(token.Key)) continue;
                    string filename = GeneralConfigurationClass.TempFilePath + GlobalFunctions.GetGuid() +
                                      ".json";
                    File.WriteAllText(filename, token.Key);
                    policystr += $" --minting-script-file {filename}";
                }
            }

            return $"{str} {policystr}";
        }
        public static string GetProtocolParamsFile(this String str, string protocolParamsFile)
        {
            return $"{str} --protocol-params-file {protocolParamsFile}";
        }
        public static string GetNetwork(this String str, bool mainnet)
        {
            return str + (mainnet ? " --mainnet" : (" --testnet-magic " + GeneralConfigurationClass.TestnetMagicId));
        }

        public static string GetPaymentScriptFile(this String str, string policyfile)
        {
            if (string.IsNullOrEmpty(policyfile) || !File.Exists(policyfile))
                return str;

            return $"{str} --payment-script-file {policyfile}";
        }
        public static string GetTransactionBuildWithLatestEra()
        {
            return "latest transaction build-estimate";
        }
        public static string GetTransactionBuildRawWithLatestEra()
        {
            return "latest transaction build-raw";
        }

        public static string GetTransactionCalculateMinRequiredUtxoLatestEra()
        {
            return "latest transaction calculate-min-required-utxo";
        }

        public static string GetAddress(this String str, string address)
        {
            return $"{str} --address {address}";
        }
        public static string GetCardanoMode(this String str)
        {
            return $"{str} --cardano-mode";
        }

        public static string GetCalculatePlutusScriptCost(this String str, string filename)
        {
            if (string.IsNullOrEmpty(filename))
                return str;
            return $"{str} --calculate-plutus-script-cost {filename}";
        }
        public static string GetTTL(this String str, CslCreateTransactionClass cctc)
        {
            return GetTTL(str,cctc.Ttl);
        }
        public static string GetTTL(this String str, long? ttl)
        {
            if (ttl == null || ttl == 0)
                return str;
            return $"{str} --ttl {ttl}";
        }

        public static string GetCddlFormat(this String str)
        {
            return $"{str}";

           // return $"{str} --cddl-format";
        }

        public static string GetWitnessOverride(this String str, long witnesses)
        {
            return $"{str} --witness-override {(witnesses + 3)}";
        }
        public static string GetTxFile(this String str, string txfile)
        {
            if (!System.IO.File.Exists(txfile))
                return str;

            return $"{str} --tx-file {txfile}";
        }
        public static string GetWitnessCount(this String str, int witnesscount)
        {
            return $"{str} --witness-count {witnesscount}";

        }
        public static string GetTxInCount(this String str, int txincount)
        {
            return $"{str} --tx-in-count {txincount}";
        }
        public static string GetTxOutCount(this String str, int txoutcount)
        {
            return $"{str} --tx-out-count {txoutcount}";

        }
        public static string GetPaymentVerifictionFile(this String str, string vkeyfile)
        {
            return $"{str} --payment-verification-key-file {vkeyfile}";
        }
        public static string GetOutFile(this string str, string outfile)
        {
            return $"{str} --out-file {outfile}";
        }
        public static string GetFees(this string str, long? fee)
        {
            if (fee == null)
                return str;

            return $"{str} --fee {fee}";
        }
        public static string GetFees(this string str, CslCreateTransactionClass cctc)
        {
            return cctc.Fees == null ? str : $"{str} --fee {cctc.Fees}";
        }
        public static string GetTxBodyFile(this string str, string bodyfile)
        {
            if (string.IsNullOrEmpty(bodyfile))
                return str;

            return $"{str} --tx-body-file {bodyfile}";
        }
        public static string GetMetadataJsonFile(this string str, string jsonfile)
        {
            if (string.IsNullOrEmpty(jsonfile))
                return str;

            return $"{str} --metadata-json-file {jsonfile}";
        }
        public static string GetMetadataJsonFile(this string str, CslCreateTransactionClass cctc)
        {
            return GetMetadataJsonFile(str,CreateMetadataStringFromMetadatumArray(cctc.Metadata), cctc.IncludeMetadataHashOnly);
        }

        private static string CreateMetadataStringFromMetadatumArray(Metadatum[] cctcMetadata)
        {
            if (cctcMetadata == null)
                return "";

            string res = "{";
            var groupbyKey =
                from a in cctcMetadata
                group a by a.Key into newGroup
                orderby newGroup.Key
                select newGroup;

            foreach (var key in groupbyKey)
            {
                var i = 0;
                foreach (var mt in cctcMetadata.OrEmptyIfNull().Where(x => x.Key == key.Key))
                {
                    i++;
                    if (i == 1)
                    {
                        res += $"\"{mt.Key}\":";

                    }

                    res += mt.Json;
                    if (i == cctcMetadata.Where(x => x.Key == key.Key).ToArray().Length)
                    {
                      //  res += "}";
                    }
                }
            }

            res += "}";

            return res;
        }

        public static string GetMetadataJsonFile(this string str, string metadata, bool includeMetadataHashOnly)
        {
            if (string.IsNullOrEmpty(metadata))
                return str;

            string jsonfile = GeneralConfigurationClass.TempFilePath + "Metadata_" + GlobalFunctions.GetGuid() + ".json";
            File.WriteAllText(jsonfile, metadata);


            return $"{str} --metadata-json-file {jsonfile}";
        }
        public static string GetSigningKeyFile(this string str, string keyfile)
        {
            if (string.IsNullOrEmpty(keyfile) || !File.Exists(keyfile))
                return str;

            return $"{str} --signing-key-file {keyfile}";
        }
        public static string GetSigningKeyFile(this string str, string[] keyfile)
        {
            if (keyfile == null)
                return str;

            string res = str;
            foreach (var s in keyfile)
            {
                res = res.GetSigningKeyFile(s);
            }

            return res;
        }

        public static string GetTransactionWitnessLatestEra()
        {
            return "latest transaction witness";
        }
        public static string GetWitnessFile(this string str, string witnessfile)
        {
            if (string.IsNullOrEmpty(witnessfile) || !File.Exists(witnessfile))
                return str;

            return $"{str} --witness-file {witnessfile}";
        }
        public static string GetWitnessFile(this string str, string[] witnessfiles)
        {
            if (witnessfiles == null)
                return str;

            string res = str;
            foreach (var s in witnessfiles)
            {
                res = res.GetWitnessFile(s);
            }

            return res;
        }

        public static string GetTransactionAssemble()
        {
            return "latest transaction assemble";
        }
        public static string GetTransactionSubmit()
        {
            return "latest transaction submit";
        }


        public static string GetTransactionCalculateMinFee()
        {
            return "conway transaction calculate-min-fee";
        }


        public static string GetQueryProtocolParameter()
        {
            return "query protocol-parameters";
        }
        public static string GetQueryUtxo()
        {
            return "query utxo";
        }
        public static string GetAddressBuild()
        {
            return "address build";
        }

        public static string GetAddressKeyHash()
        {
            return "address key-hash";
        }

        public static string GetTransactionSign()
        {
            return "latest transaction sign";
        }
    }
}
