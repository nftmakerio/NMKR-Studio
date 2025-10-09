using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace NMKR.Shared.Functions.Metadata;

public class ConvertCardanoToAptosMetadata : IConvertCardanoMetadata
{
    public string ConvertCip25CardanoMetadata(string cardanoMetadataString, string symbol = "", string collection = "", int? sellerFeeBasisPoints = null, List<string> creators = null)
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

        var aptosMetadata = new JObject();
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
                            aptosMetadata["name"] = assetData["name"]?.ToString() ?? asset.Name;

                            // Beschreibung
                            aptosMetadata["description"] = assetData["description"] is JArray descriptionArray
                                ? string.Join(" ", descriptionArray)
                                : assetData["description"]?.ToString() ?? "No description provided.";

                            // Bild
                            aptosMetadata["image"] = assetData["image"]?.ToString() ?? "No image provided.";

                            // Attribute
                            var attributes = new JArray();
                            foreach (var property in assetData.Properties())
                            {
                                // Füge alle Felder hinzu, die nicht explizit verarbeitet werden
                                if (property.Name != "name" && property.Name != "image" &&
                                    property.Name != "description" &&
                                    property.Name != "files")
                                {
                                    if (property.Value is JArray arrayValue)
                                    {
                                        // Wenn das Feld ein Array ist, füge es als String hinzu
                                        attributes.Add(new JObject
                                        {
                                            ["trait_type"] = property.Name,
                                            ["value"] = string.Join(", ", arrayValue)
                                        });
                                    }
                                    else if (property.Value != null)
                                    {
                                        // Füge das Feld direkt hinzu
                                        attributes.Add(new JObject
                                        {
                                            ["trait_type"] = property.Name,
                                            ["value"] = property.Value.ToString()
                                        });
                                    }
                                }
                            }

                            aptosMetadata["attributes"] = attributes;

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
                            aptosMetadata["properties"] = new JObject
                            {
                                ["files"] = files,
                                /*   ["creators"] = new JArray
                            {
                                new JObject
                                {
                                    ["address"] = "0x123456789abcdef", // Beispieladresse
                                    ["share"] = 100
                                }
                            }*/
                            };
                        }
                    }
                }
            }
        }

        return aptosMetadata.ToString();
    }
}