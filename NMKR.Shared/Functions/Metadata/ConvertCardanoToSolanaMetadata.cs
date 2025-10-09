using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace NMKR.Shared.Functions.Metadata;

public class ConvertCardanoToSolanaMetadata : IConvertCardanoMetadata
{
    public string ConvertCip25CardanoMetadata(string cardanoMetadataString,string solanasymbol,string solanacollectiontransaction="", int? sellerFeeBasisPoints = null, List<string> creators = null)
    {
        // Parse the Cardano metadata string into a JObject
        if (string.IsNullOrWhiteSpace(cardanoMetadataString))
        {
            throw new ArgumentException("The Cardano metadata string is null or empty.");
        }

        var cardanoMetadata = JObject.Parse(cardanoMetadataString);
        if (!cardanoMetadata.ContainsKey("721"))
        {
            throw new ArgumentException("The Cardano metadata are not correct (721 is missing).");
        }

        var solanaMetadata = new JObject();
        var cardano721 = cardanoMetadata["721"] as JObject;

        if (cardano721 != null)
        {
            foreach (var policy in cardano721.Properties())
            {
                var assets = policy.Value as JObject;

                if (assets != null)
                {
                    foreach (var asset in assets.Properties())
                    {
                        var assetData = asset.Value as JObject;

                        // Name
                        if (assetData != null)
                        {
                            solanaMetadata["name"] = assetData["name"]?.ToString() ?? asset.Name;
                            solanaMetadata["symbol"] = solanasymbol;
                            if (!string.IsNullOrEmpty(solanacollectiontransaction))
                            {
                                solanaMetadata["collection"] = solanacollectiontransaction;
                            }
                            // Beschreibung
                            solanaMetadata["description"] = assetData["description"] is JArray descriptionArray
                                ? string.Join(" ", descriptionArray)
                                : assetData["description"]?.ToString() ?? "No description provided.";

                            // Bild
                            solanaMetadata["image"] = assetData["image"]?.ToString() ?? "No image provided.";

                            // Attribute
                            var attributes = new JArray();
                            foreach (var property in assetData.Properties())
                            {
                                // F¸ge alle Felder hinzu, die nicht explizit verarbeitet werden
                                if (property.Name != "name" && property.Name != "image" &&
                                    property.Name != "description" &&
                                    property.Name != "files")
                                {
                                    if (property.Value is JArray arrayValue)
                                    {
                                        // Wenn das Feld ein Array ist, f¸ge es als String hinzu
                                        attributes.Add(new JObject
                                        {
                                            ["trait_type"] = property.Name,
                                            ["value"] = string.Join(", ", arrayValue)
                                        });
                                    }
                                    else if (property.Value != null)
                                    {
                                        // F¸ge das Feld direkt hinzu
                                        attributes.Add(new JObject
                                        {
                                            ["trait_type"] = property.Name,
                                            ["value"] = property.Value.ToString()
                                        });
                                    }
                                }
                            }

                            solanaMetadata["attributes"] = attributes;

                            // Dateien
                            var files = new JArray();
                            if (assetData.ContainsKey("files") && assetData["files"] is JArray filesArray)
                            {
                                foreach (var file in filesArray)
                                {
                                    if (file is JObject fileObject)
                                    {
                                        files.Add(new JObject
                                        {
                                            ["uri"] = fileObject["src"]?.ToString(),
                                            ["type"] = fileObject["mediaType"]?.ToString(),
                                            ["name"] = fileObject["name"]?.ToString()
                                        });
                                    }
                                }
                            }

                            // Eigenschaften
                            var properties = new JObject
                            {
                                ["files"] = files
                            };

                            // Setze seller_fee_basis_points, wenn ¸bergeben
                            if (sellerFeeBasisPoints.HasValue)
                            {
                                solanaMetadata["seller_fee_basis_points"] = sellerFeeBasisPoints.Value;
                            }

                            // Setze creators, wenn ¸bergeben
                            if (creators != null && creators.Count > 0)
                            {
                                var creatorsArray = new JArray();
                                foreach (var creator in creators)
                                {
                                    creatorsArray.Add(new JObject
                                    {
                                        ["address"] = creator,
                                        ["share"] = 100 / creators.Count // Gleichm‰ﬂige Verteilung der Anteile
                                    });
                                }
                                properties["creators"] = creatorsArray;
                            }

                            solanaMetadata["properties"] = properties;
                        }
                    }
                }
            }
        }

        return solanaMetadata.ToString();
    }
}