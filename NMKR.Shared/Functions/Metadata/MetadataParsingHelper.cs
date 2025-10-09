using System;
using System.Collections.Generic;
using System.Linq;
using NMKR.Shared.Classes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NMKR.Shared.Functions.Metadata
{
    public static class MetadataParsingHelper
    {
        /// <summary>
        /// Parses a normal NFT metadata into an instance of <see cref="NFTMetadata"/>
        /// </summary>
        /// <param name="metadata">the NFT's metadata as JSON</param>
        /// <param name="policyId">Policy id</param>
        /// <param name="tokenName">Token name</param>
        /// <returns>The NFT's metadata as instance of <see cref="NFTMetadata"/></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static NFTMetadata ParseNormalMetadata(string metadata, string policyId, string tokenName)
        {
            string name = null;
            string description = null;
            string previewImage = null;
            string previewImageHash = null;
            string previewImageMediaType = null;

            var files = new List<NFTFile>();
            var fields = new Dictionary<string, string>();
            var fieldsarray = new Dictionary<string, string[]>();


            metadata = metadata.Replace("<policy_id>", policyId);
            metadata = metadata.Replace("<asset_name>", tokenName);


            try
            {
                var jsonObject = JObject.Parse(metadata);

                var metadataKeyTokens = metadata.Contains("\"721\"")
                    ? jsonObject.SelectTokens("721")
                    : metadata.Contains("\"20\"")
                        ? jsonObject.SelectTokens("20")
                        : jsonObject.Descendants();

                var keyTokens = metadataKeyTokens as JToken[] ?? metadataKeyTokens.ToArray();
                if (keyTokens.Any())
                {

                    /*   foreach (var keyToken in jsonObject.Properties())
                       {
                           string st = keyToken.Type + " - " + keyToken.Path + " - "+ keyToken.Name + keyToken.Value.ToString();
                           Console.WriteLine(st);
                       }
                    */

                    var metadataToken = keyTokens.FirstOrDefault();

                    if (metadataToken != null)
                    {

                        var root = metadata.Contains("\"" + policyId + "\"") ? $"$.{policyId}.['{tokenName}']" : "";

                        var plainMetadataToken = metadataToken.SelectToken(root);

                        var nameToken = metadataToken.SelectToken($"{root}.name");
                        var previewImageToken = metadataToken.SelectToken($"{root}.image");
                        var previewImageMediaTypeToken =
                            metadataToken.SelectToken($"{root}.mediaType");
                        var descriptionToken = metadataToken.SelectToken($"{root}.description");
                        var filesToken = metadataToken.SelectToken($"{root}.files");


                        foreach (var tok1 in plainMetadataToken.OrEmptyIfNull())
                        {
                            var st = "{" + tok1.ToString() + "}";
                            try
                            {
                                /*   var tok2 = tok1.First();
                                   var valuex = tok2.Value<string>();
                                   var namex = tok2.Path.Split(".").LastOrDefault();
                                   if (namex != null && valuex != null) fields.Add(namex, valuex); */

                                if (st.Contains("[") && st.Contains("]"))
                                {
                                    //TODO: Array structures into JSON
                                    //    var values = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(st);
                                    //    fieldsarray.Add(values.First().Key, values.First().Value);
                                }
                                else
                                {
                                    var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(st);
                                    fields.Add(values.First().Key, values.First().Value);
                                }
                            }
                            catch (Exception ex)
                            {
                                GlobalFunctions.LogException(null, "Exception while parsing Metadata (1) " + ex.Message,
                                    st + " - " + (ex.StackTrace ?? "") + (ex.InnerException?.StackTrace ?? "") +
                                    (ex.Source ?? "") + Environment.NewLine + metadata + Environment.NewLine +
                                    policyId + " " + tokenName);
                            }
                        }


                        if (nameToken != null)
                        {
                            name = nameToken.Value<string>();
                        }

                        if (previewImageToken != null)
                        {
                            previewImage = IpfsHelper.ToIpfsGatewayUrl(previewImageToken.Value<string>());
                            previewImageHash = previewImageToken.Value<string>();
                        }

                        if (previewImageMediaTypeToken != null)
                        {
                            previewImageMediaType = previewImageMediaTypeToken.Value<string>();
                        }

                        if (descriptionToken != null)
                        {
                            description = descriptionToken.Value<string>();
                        }

                        if (filesToken != null)
                        {
                            foreach (var fileToken in filesToken)
                            {
                                if (fileToken is JObject fileObject)
                                {
                                    if (fileObject.ContainsKey("src") && fileObject.ContainsKey("mediaType"))
                                    {
                                        files.Add(new()
                                        {
                                            Url = IpfsHelper.ToIpfsGatewayUrl(fileObject["src"].Value<string>()),
                                            MediaType = fileObject["mediaType"].Value<string>(),
                                            Hash = fileObject["src"].Value<string>(),
                                            Name = fileObject["name"].Value<string>()
                                        });
                                    }
                                }
                            }
                        }

                        return new()
                        {
                            PolicyId = policyId,
                            TokenName = tokenName,
                            Metadata = plainMetadataToken?.ToString() ?? null,
                            Name = !string.IsNullOrEmpty(name) ? name : null,
                            Description = !string.IsNullOrEmpty(description) ? description : null,
                            PreviewImage = new()
                            {
                                Url = previewImage,
                                MediaType = previewImageMediaType,
                                Hash = previewImageHash
                            },
                            Files = files,
                            Fields = fields,
                            FieldsArray = fieldsarray,
                        };
                    }

                    return new()
                    {
                        PolicyId = policyId,
                        TokenName = tokenName,
                        Files = files,
                        Fields = fields,
                        FieldsArray = fieldsarray,
                    };
                }

                return new()
                {
                    PolicyId = policyId,
                    TokenName = tokenName,
                    Files = files,
                    Fields = fields,
                    FieldsArray = fieldsarray,
                };
            }
            catch (Exception ex)
            {
                GlobalFunctions.LogException(null, "Exception while parsing Metadata (2) " + ex.Message,
                    (ex.StackTrace ?? "") + (ex.InnerException?.StackTrace ?? "") + (ex.Source ?? "") +
                    Environment.NewLine + metadata + Environment.NewLine + policyId + " " + tokenName);
                return new()
                {
                    PolicyId = policyId,
                    TokenName = tokenName,
                    Name = !string.IsNullOrEmpty(name) ? name : null,
                    Description = !string.IsNullOrEmpty(description) ? description : null,
                    PreviewImage = new()
                    {
                        Url = previewImage,
                        MediaType = previewImageMediaType,
                        Hash = previewImageHash
                    },
                    Files = files,
                    Fields = fields,
                    FieldsArray = fieldsarray,
                };
            }
        }

        public static string GetMetadataForSpecificToken(string metadata, string policyId, string tokenNameInHex)
        {
            var jsonObject = JObject.Parse(metadata);

            var metadataKeyTokens = metadata.Contains("\"721\"")
                ? jsonObject.SelectTokens("721")
                : metadata.Contains("\"20\"")
                    ? jsonObject.SelectTokens("20")
                    : jsonObject.Descendants();

            var keyTokens = metadataKeyTokens as JToken[] ?? metadataKeyTokens.ToArray();
            if (keyTokens.Any())
            {



                var metadataToken = keyTokens.FirstOrDefault();

                if (metadataToken != null)
                {

                    string version = "1.0";
                    var root = metadata.Contains("\"" + policyId + "\"") ? $"$.{policyId}.['{tokenNameInHex.FromHex()}']" : "";

                    var plainMetadataToken = metadataToken.SelectToken(root)?.ToString();

                    if (string.IsNullOrEmpty(plainMetadataToken))
                    {
                        root = metadata.Contains("\"" + policyId + "\"") ? $"$.{policyId}.['{tokenNameInHex}']" : "";
                        plainMetadataToken = metadataToken.SelectToken(root)?.ToString();
                        version = "2.0";
                    }


                    if (string.IsNullOrEmpty(plainMetadataToken))
                        return null;


                    return GetEnvelopeForMetadata(plainMetadataToken, policyId, tokenNameInHex, version);
                }

            }

            return null;
        }

        private static string GetEnvelopeForMetadata(string plainMetadataToken, string policyId, string tokenNameInHex, string version)
        {
            string st =
                $"{{\"721\": {{\"{policyId}\": {{\"{(version == "1.0" ? tokenNameInHex.FromHex() : tokenNameInHex)}\":" +
                plainMetadataToken + $"}}, \"version\": \"{version}\"  }}}}";

            return JsonFormatter.FormatJson(st);
        }
    }
}
