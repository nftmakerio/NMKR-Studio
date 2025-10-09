using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using NMKR.Shared.Classes;

public class CliUTxOValue
{
    [JsonPropertyName("address")]
    public string Address { get; set; }

    [JsonPropertyName("datum")]
    public object Datum { get; set; }

    [JsonPropertyName("datumhash")]
    public object DatumHash { get; set; }

    [JsonPropertyName("inlineDatum")]
    public object InlineDatum { get; set; }

    [JsonPropertyName("inlineDatumRaw")]
    public object InlineDatumRaw { get; set; }

    [JsonPropertyName("referenceScript")]
    public object ReferenceScript { get; set; }

    [JsonPropertyName("value")]
    public Dictionary<string, object> Value { get; set; }
}

public class CardanoUTxOParser
{
    public List<TxInClass> ParseUTxOs(string jsonString)
    {
        var result = new List<TxInClass>();

        // JSON als JObject parsen für mehr Flexibilität
        var utxosJObject = JObject.Parse(jsonString);

        foreach (var utxoProperty in utxosJObject.Properties())
        {
            var txInKey = utxoProperty.Name; // z.B. "16c5998fe9b6dcd7d81d2d297e7d5255eb91112485e74aa1f855e1c4a7cb4ede#0"
            var utxoJObject = utxoProperty.Value as JObject;

            // TxHash und Output Index aus dem Key extrahieren
            var parts = txInKey.Split('#');
            var txHash = parts[0];
            var outputIndex = int.Parse(parts[1]);

            var txIn = new TxInClass
            {
                TxHash = txHash,
                TxId = outputIndex,
                Lovelace = 0,
                Tokens = new List<TxInTokensClass>()
            };

            // Value-Objekt verarbeiten
            var valueObject = utxoJObject["value"] as JObject;
            if (valueObject != null)
            {
                foreach (var valueProperty in valueObject.Properties())
                {
                    if (valueProperty.Name == "lovelace")
                    {
                        // Lovelace-Wert extrahieren
                        txIn.Lovelace = valueProperty.Value.Value<long>();
                    }
                    else
                    {
                        // Token-Daten (PolicyId -> Assets)
                        var policyId = valueProperty.Name;
                        var assetsObject = valueProperty.Value as JObject;

                        if (assetsObject != null)
                        {
                            foreach (var assetProperty in assetsObject.Properties())
                            {
                                var assetName = assetProperty.Name;
                                var quantity = assetProperty.Value.Value<long>();

                                txIn.Tokens.Add(new TxInTokensClass
                                {
                                    PolicyId = policyId, 
                                    TokennameHex = assetName,
                                    Tokenname = assetName.FromHex(),
                                    Quantity = quantity
                                });
                            }
                        }
                    }
                }
            }

            result.Add(txIn);
        }

        return result;
    }

}