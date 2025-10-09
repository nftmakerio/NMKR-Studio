using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CardanoSharp.Wallet.Models.Addresses;
using CardanoSharp.Wallet.Utilities;
using PeterO.Cbor2;

namespace NMKR.Shared.SmartContracts
{


    public static class JpgStoreSmartContractDatumParser
    {
        // Taken from: https://stackoverflow.com/a/724905
        private static string FromHex(string hex)
        {
            byte[] raw = new byte[hex.Length / 2];

            for (int i = 0; i < raw.Length; i++)
            {
                raw[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }

            return Encoding.UTF8.GetString(raw);
        }

        public static List<Output> Parse(string cborHex, CardanoSharp.Wallet.Enums.NetworkType network = CardanoSharp.Wallet.Enums.NetworkType.Mainnet)
        {
            var outputs = new List<Output>();

            var cborParsed = CBORObject.DecodeFromBytes(Convert.FromHexString(cborHex));
            var cborEntries = cborParsed.Values.ToList();

            // Extract the seller PKH
            //var sellerKeyHashBytes = cborEntries[0].GetByteString();
            //var sellerKeyHashHex = Convert.ToHexString(sellerKeyHashBytes);

            foreach (var output in cborEntries[1].Values.Select(x => x.Values))
            {
                byte[] pkhBytes = null;
                try
                {
                    // Extract the output PKH
                     pkhBytes = output
                        .ToList()[0]
                        .Values
                        .ToList()[0]
                        .Values
                        .ToList()[0]
                        .GetByteString();

                }
                catch
                {
                    pkhBytes = output
                        .ToList()[0]
                        .GetByteString();
                }
                //var pkhHex = Convert.ToHexString(pkhBytes);

                // Extract the output stake key
                var stakeKeyBytes = Array.Empty<byte>();
                ICollection<CBORObject> stakeKeyBytesPath = null;

                try
                {
                    stakeKeyBytesPath = output
                        .ToList()[0]
                        .Values
                        .ToList()[1]
                        .Values
                        .ToList()[0]
                        .Values
                        .ToList()[0]
                        .Values;
                }
                catch
                {
                    // ignored
                }

                if (stakeKeyBytesPath!=null && stakeKeyBytesPath.Any())
                {
                    stakeKeyBytes = stakeKeyBytesPath
                        .ToList()[0]
                        .GetByteString();
                }

                //var stakeKeyHex = Convert.ToHexString(stakeKeyBytes);

                // Construct the output address
                Address address;

                if (stakeKeyBytes.Length > 0)
                {
                    address = AddressUtility.GetBaseAddress(pkhBytes, stakeKeyBytes, network);
                }
                else
                {
                    address = AddressUtility.GetEnterpriseAddress(pkhBytes, network);
                }

                var addressBech32 = address.ToString();

                var amountsObjects = output
                    .ToList()[1]
                    .Entries
                    .ToDictionary(x => x.Key, x => x.Value);

                var emptyObject = CBORObject.FromObject(Array.Empty<byte>());

                foreach (var (key, value) in amountsObjects)
                {
                    // Check if it is an lovelace entry
                    if (key.Equals(emptyObject))
                    {
                        long lovelace = 0;
                        try
                        {
                            var lovelace11 = value.Values.First(x => !x.IsNumber || (x.IsNumber && x.AsNumber().IsNaN()))
                                .Values.First();
                            lovelace= lovelace11.AsInt64Value();
                        }
                        catch
                        {
                            var lovelace11 = value.Values.First();
                            lovelace = lovelace11.AsInt64Value();
                        }
                        outputs.Add(new Output(addressBech32, lovelace, new List<OutputToken>()));

                    }
                    // Handle token entries
                    else
                    {
                        var tokens = new List<OutputToken>();

                        // First extract the policy id
                        var policyIdBytes = key.GetByteString();
                        var policyId = Convert.ToHexString(policyIdBytes);

                        // Iterate over token entries
                        foreach (var tokenObject in value.Values.Where(x => x.Type == CBORType.Map))
                        {
                            foreach (var (tokenNameObject, countObject) in tokenObject.Entries)
                            {
                                var tokenNameBytes = tokenNameObject.GetByteString();
                                var tokenNameHex = Convert.ToHexString(tokenNameBytes);
                                var tokenName = FromHex(tokenNameHex);

                                var count = countObject.AsInt64Value();

                                tokens.Add(new OutputToken(policyId, tokenName, count));
                            }
                        }

                        outputs.Add(new Output(addressBech32, 0L, tokens));
                    }
                }
            }

            return outputs;
        }

        public record Output(string AddressBech32, long Lovelace, List<OutputToken> Tokens);

        public record OutputToken(string PolicyId, string TokenName, long Count = 1);
    }
}

