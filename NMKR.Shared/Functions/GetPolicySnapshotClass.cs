using NMKR.Shared.Classes;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions.Koios;
using NMKR.Shared.Functions.Solana;

namespace NMKR.Shared.Functions
{
    public class HandleAssetEventArgs
    {
        public HandleAssetEventArgs(string assetname, long totalAssets, long currentAsset, string description)
        {
            AssetName = assetname;
            TotalAssets = totalAssets;
            CurrentAsset=currentAsset;
            Description = description;
        }
        public string AssetName { get;  }
        public long TotalAssets { get; } 
        public long CurrentAsset { get; }
        public string Description { get; }
    }
    public class GetPolicySnapshotClass
    {
        public delegate void HandleAssetEventHandler(object sender, HandleAssetEventArgs e);
        public event HandleAssetEventHandler HandleAssetEvent;
        public async Task<NmkrAssetPolicySnapshot[]> GetAllAddressesForSpecificPolicyIdAsync(
          IConnectionMultiplexer redis, string policyId, bool cumulate, bool withMintingInformation, Blockchain blockchain)
        {
            if (blockchain == Blockchain.Cardano)
            {
                return await GetSnapshopFromCardanoBlockchainAsync(redis, policyId, cumulate, withMintingInformation);
            }
            if (blockchain == Blockchain.Solana)
            {
                return await GetSnapshopFromSolanaBlockchainAsync(redis, policyId);
            }
            return null;
        }

        private async Task<NmkrAssetPolicySnapshot[]> GetSnapshopFromSolanaBlockchainAsync(IConnectionMultiplexer redis, string collectionid)
        {
            List<NmkrAssetPolicySnapshot> snapshot = new List<NmkrAssetPolicySnapshot>();
            var assets = await SolanaFunctions.GetAllAssetsForCollectionAsync(collectionid);

            foreach (var solanaItem in assets)
            {
                NmkrAssetAddress as1 = new NmkrAssetAddress()
                {
                    PolicyId = collectionid,
                    AssetName = solanaItem.Content.Metadata.Name,
                    AssetNameInHex = solanaItem.Content.Metadata.Name.ToHex(),
                    SolanaDescription = solanaItem.Content.Metadata.Description,
                    SolanaSymbol = solanaItem.Content.Metadata.Symbol,
                    SolanaTokenStandard = solanaItem.Content.Metadata.TokenStandard,
                    TotalSupply = 1,
                    Decimals = solanaItem.TokenInfo.Decimals ?? 0,
                    Address = solanaItem.TokenInfo.AssociatedTokenAddress,
                    Quantity = 1,

                };

                if (snapshot.Any(x => x.Address == solanaItem.Ownership.Owner))
                {
                    var f = snapshot.Find(x => x.StakeAddress == solanaItem.Ownership.Owner);
                    f.TotalQuantity += 1;
                    var assetx = f.AssetsOnStakeAddress;
                    ArrayHelper.Add(ref assetx, as1);
                    f.AssetsOnStakeAddress = assetx;
                    continue;
                }

                NmkrAssetPolicySnapshot asset = new NmkrAssetPolicySnapshot()
                {
                    Address = solanaItem.Ownership.Owner, StakeAddress = solanaItem.Ownership.Owner,
                    AssetsOnStakeAddress = new[] {as1}, TotalQuantity = 1
                };
                snapshot.Add(asset);
            }

            return snapshot.ToArray();

        }

        private async Task<NmkrAssetPolicySnapshot[]> GetSnapshopFromCardanoBlockchainAsync(IConnectionMultiplexer redis, string policyId, bool cumulate,
            bool withMintingInformation)
        {
            List<NmkrAssetPolicySnapshot> snapshot = new List<NmkrAssetPolicySnapshot>();
            var assets = await KoiosFunctions.GetAllAssetsFromPolicyidAsync(redis, policyId);

            try
            {
                if (assets != null)
                {
                    List<ConsoleCommand.GetParallelAddressInfo>
                        urls = new List<ConsoleCommand.GetParallelAddressInfo>();
                    int i = 0;
                    foreach (var asset in assets)
                    {
                        i++;
                        HandleAssetEvent?.Invoke(this,
                            new HandleAssetEventArgs(asset.AssetNameAscii, assets.Length, i, "Catching Asset"));

                        var multiplier =
                            await GlobalFunctions.GetFtTokensMultiplierAsync(policyId, asset.AssetName);

                        urls.Add(new ConsoleCommand.GetParallelAddressInfo()
                        {
                            url =
                                $"{GeneralConfigurationClass.KoiosApi}/asset_addresses?_asset_policy={policyId}&_asset_name={asset.AssetName}",
                            address = new NmkrAssetAddress()
                            {
                                PolicyId = policyId,
                                Fingerprint = asset.Fingerprint,
                                MintingTxHash = asset.MintingTxHash,
                                TotalSupply = asset.TotalSupply,
                                AssetName = asset.AssetNameAscii,
                                Multiplier = multiplier.Multiplier,
                                Decimals = multiplier.Decimals,
                                CreationTime = asset.CreationTime,
                            }
                        });
                    }


                    HandleAssetEvent?.Invoke(this, new HandleAssetEventArgs("", 0, 0, "...Please wait..."));
                    var res = await ConsoleCommand.DownloadUrlsAsync(urls, 100);

                    i = 0;
                    foreach (var value in res)
                    {
                        i++;

                        if (value.data == null || value.url == null || value.url.address == null)
                            continue;

                        ConsoleCommand.AssetAddressx[] assetAddresses =
                            JsonConvert.DeserializeObject<ConsoleCommand.AssetAddressx[]>(value.data);

                        HandleAssetEvent?.Invoke(this,
                            new HandleAssetEventArgs(value.url.address.AssetName, res.Count, i,
                                "Processing Asset"));
                        foreach (var assetAddress in assetAddresses)
                        {
                            var assetnew = new NmkrAssetAddress()
                            {
                                PolicyId = policyId,
                                Quantity = (assetAddress.Quantity ?? 0),
                                Address = assetAddress.PaymentAddressPaymentAddress,
                                AssetName = value.url.address.AssetName,
                                AssetNameInHex = value.url.address.AssetName.ToHex(),
                                Fingerprint = value.url.address.Fingerprint,
                                TotalSupply = value.url.address.TotalSupply,
                                Multiplier = value.url.address.Multiplier,
                                MintingTxHash = value.url.address.MintingTxHash,
                                CreationTime = value.url.address.CreationTime,
                                MintingTransactionInformation = withMintingInformation
                                    ? await GetMintingTransactionInformation(value.url.address.MintingTxHash,
                                        policyId, value.url.address.AssetName.ToHex())
                                    : null
                            };

                            bool c = false;
                            string stake =
                                Bech32Engine.GetStakeFromAddress(assetAddress.PaymentAddressPaymentAddress);
                            if (cumulate)
                            {
                                if (!string.IsNullOrEmpty(stake))
                                {
                                    var f = snapshot.Find(x => x.StakeAddress == stake);
                                    if (f != null)
                                    {
                                        c = true;
                                        f.TotalQuantity += (assetAddress.Quantity ?? 0);

                                        var asset = f.AssetsOnStakeAddress;
                                        ArrayHelper.Add(ref asset, assetnew);

                                        f.AssetsOnStakeAddress = asset;
                                    }
                                }
                            }

                            if (!c)
                            {
                                snapshot.Add(new NmkrAssetPolicySnapshot()
                                {
                                    AssetsOnStakeAddress = new[] {assetnew},
                                    TotalQuantity = (assetAddress.Quantity ?? 0),
                                    StakeAddress = stake,
                                    Address = assetAddress.PaymentAddressPaymentAddress,
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }

            return snapshot.ToArray();
        }

        private async Task<MintingTransactionInformation> GetMintingTransactionInformation(string addressMintingTxHash, string mintedassetpolicyid, string mintedassetnameinhex)
        {
            MintingTransactionInformation mti = new MintingTransactionInformation();
            if (string.IsNullOrEmpty(addressMintingTxHash))
                return mti;

            var transaction = await KoiosFunctions.GetTransactionInformationAsync(addressMintingTxHash);
            if (transaction != null && transaction.Any())
            {
                foreach (var koiosTransactionAssetsClass in transaction.First().AssetsMinted.OrEmptyIfNull())
                {
                    if (koiosTransactionAssetsClass.PolicyId == mintedassetpolicyid && koiosTransactionAssetsClass.AssetName == mintedassetnameinhex)
                    {
                        mti.Quantity = koiosTransactionAssetsClass.Quantity ?? 0;
                        break;
                    }
                }

                foreach (var output in transaction.First().Outputs.OrEmptyIfNull())
                {
                    foreach (var asset in output.AssetList.OrEmptyIfNull())
                    {
                        if (asset.PolicyId == mintedassetpolicyid && asset.AssetName == mintedassetnameinhex)
                        {
                            mti.Address = output.PaymentAddr.Bech32;
                            break;
                        }
                    }
                }

                mti.Slot = transaction.First().AbsoluteSlot;
            }

            return mti;
        }

    }
}
