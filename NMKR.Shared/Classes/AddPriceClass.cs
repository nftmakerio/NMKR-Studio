using System;
using System.Collections.Generic;
using System.Linq;
using NMKR.Shared.Classes.Koios;
using NMKR.Shared.Functions.Koios;
using StackExchange.Redis;

namespace NMKR.Shared.Classes
{
    public class AddPriceClass
    {
        public int? Id { get; set; }
        public int NftProjectId { get; set; }
        public long CountNft { get; set; }
        public double Price { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public bool Activated { get; set; }
        public string Currency { get; set; }

        private string _additionalTokenPolicyId = "";
        private string _additionalTokenAssetName = "";
        private long? _additionalTokenCount;
        private IConnectionMultiplexer _redis;

        public AddPriceClass(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }
        public string AdditionalTokenPolicyId
        {
            get => _additionalTokenPolicyId;
            set
            {
                _additionalTokenPolicyId = value;
                UpdateTokenInformation();
            }
        }
        public string AdditionalTokenAssetName
        {
            get => _additionalTokenAssetName;
            set
            {
                _additionalTokenAssetName = value;
                UpdateTokenInformation();
            }
        }
        public long? AdditionalTokenCount
        {
            get => _additionalTokenCount;
            set
            {
                _additionalTokenCount = value;
                UpdateTokenInformation();
            }
        }

        private string _tokenInformation = "";
        public string TokenInformation => _tokenInformation;
        public List<KoiosPolicyAssetsClass> Tokenlist = new List<KoiosPolicyAssetsClass>();
        private string tokenlistpolicyid = "";
        private void UpdateTokenInformation()
        {
            _tokenInformation = "";
            if (!string.IsNullOrEmpty(AdditionalTokenPolicyId) && !string.IsNullOrEmpty(AdditionalTokenAssetName) && AdditionalTokenPolicyId.Length==56)
            {
                var assetinfo = KoiosFunctions.GetTokenInformation(AdditionalTokenPolicyId, AdditionalTokenAssetName.ToHex());
                if (assetinfo != null && assetinfo.Any() && assetinfo.First() != null)
                {
                    _tokenInformation = $"{assetinfo.First().AssetName} - {assetinfo.First().Description} - Ticker: {assetinfo.First().Ticker} - Decimals: {assetinfo.First().Decimals}";
                }
            }

            if (!string.IsNullOrEmpty(AdditionalTokenPolicyId) &&
                AdditionalTokenPolicyId.Length == 56)
            {
                if (Tokenlist.Any() && tokenlistpolicyid == AdditionalTokenPolicyId)
                    return;

                var tokens = KoiosFunctions.GetAllAssetsFromPolicyid(_redis, AdditionalTokenPolicyId);
                   Tokenlist.Clear();

                   tokenlistpolicyid = tokens.Any() ? AdditionalTokenPolicyId : "";

                Tokenlist.AddRange(ConsoleCommand.RemoveCip68Prefix(tokens
                    .Where(x => x.AssetName.StartsWith("000643b0") == false).ToArray()));
            }
        }
    }
}
