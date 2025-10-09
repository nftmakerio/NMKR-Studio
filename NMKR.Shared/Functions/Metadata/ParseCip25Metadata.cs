using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace NMKR.Shared.Functions.Metadata
{
    public class ParseCip25Metadata : IMetadataParserInterface
    {
        public ParsedMetadata ParseMetadata(string metadata)
        {
            try
            {
                var jsonObject = JObject.Parse(metadata);
                var pm = ParseJToken(jsonObject, new ParsedMetadata(), new string[] { });

                if (string.IsNullOrEmpty(pm.MetadataType))
                    throw new Exception("Missing Metadata Type Key");
                if (string.IsNullOrEmpty(pm.PolicyId))
                    throw new Exception("Missing PolicyId");
                if (string.IsNullOrEmpty(pm.TokenName))
                    throw new Exception("Missing TokenName");
                if (string.IsNullOrEmpty(pm.Version))
                    throw new Exception("Missing Version");
              
                if (!string.IsNullOrEmpty(pm.Version) && pm.Version.StartsWith("2") && pm.TokenName!="<asset_name>")
                {
                    try
                    {
                        pm.TokenName = pm.TokenName.FromHex();
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Error decoding TokenName - Version 2.0 requires Hex Encoded Tokenname",e);
                    }
                }
                pm.MetadataStandard = MetadataStandard.CIP25;
                pm.OriginalMetadata = metadata;


                return pm;
            }
            catch (Exception e)
            {
               throw new Exception($"Error parsing CIP25 metadata - {e.Message}",e);
            }
        }


        private ParsedMetadata ParseJToken(JToken token,ParsedMetadata parsedMetadata,string[] lastkeys, int depth = 0, int? arrayindex=null)
        {
            string prefix = new string(' ', depth * 2); 

            switch (token.Type)
            {
                case JTokenType.Object:
                    foreach (var property in ((JObject) token).Properties())
                    {
                        parsedMetadata.Log += $"{prefix} {depth} Key: {property.Name}" + Environment.NewLine;

                        parsedMetadata = CheckForMetadataTypeKey(parsedMetadata, property.Name, lastkeys, depth);
                        parsedMetadata = CheckForPolicyId(parsedMetadata, property.Name, lastkeys, depth);
                        parsedMetadata = CheckForTokenName(parsedMetadata, property.Name, lastkeys, depth);

                        parsedMetadata = ParseJToken(property.Value, parsedMetadata,
                            ArrayHelper.Add(ref lastkeys, property.Name),depth + 1, arrayindex);
                        ArrayHelper.RemoveAt(ref lastkeys,lastkeys.Length-1);
                    }

                    break;
                case JTokenType.Array:
                    int index = 0;
                    foreach (var value in ((JArray) token))
                    {
                        parsedMetadata.Log += $"{prefix} {depth} Index: {index}" + Environment.NewLine;
                        parsedMetadata = ParseJToken(value, parsedMetadata, lastkeys, depth + 1, index);
                        index++;
                    }

                    break;
                default: // Für JTokenType.String, JTokenType.Integer, etc.
                    parsedMetadata.Log += $"{prefix}{String.Join('-',lastkeys)} - {depth} Value: {token.ToString()}" +
                                          Environment.NewLine;

                    parsedMetadata = CheckForPreviewImageSrc(parsedMetadata, token, lastkeys, depth);
                    parsedMetadata = CheckForPreviewImageMimetype(parsedMetadata, token, lastkeys, depth);

                    parsedMetadata = CheckForFilesSrc(parsedMetadata, token, lastkeys, depth, arrayindex??0);
                    parsedMetadata = CheckForFilesMimetype(parsedMetadata, token, lastkeys, depth, arrayindex??0);
                    parsedMetadata = CheckForFilesName(parsedMetadata, token, lastkeys, depth,arrayindex??0);

                    parsedMetadata = CheckForFields(parsedMetadata, token, lastkeys, arrayindex);
                    parsedMetadata = CheckForVersion(parsedMetadata, token, lastkeys, depth);

                    break;
            }

            return parsedMetadata;
        }
        private ParsedMetadata CheckForVersion(ParsedMetadata parsedMetadata, JToken token, string[] lastkeys, int depth)
        {
            if (lastkeys != null && lastkeys.Any() && lastkeys.Last().ToLower() == "version" && depth == 2)
            {
                parsedMetadata.Version = token.ToString();
            }

            return parsedMetadata;
        }

       


        private ParsedMetadata CheckForFields(ParsedMetadata parsedMetadata, JToken token, string[] keys, int? arrayindex)
        {
            if (keys.Length <= 3)
            {
                return parsedMetadata;
            }


            for (int i = 0; i < 3; i++)
            {
                ArrayHelper.RemoveAt(ref keys, 0);
            }

            var name=keys.LastOrDefault();
            var firstkey=keys.FirstOrDefault();
            if (!string.IsNullOrEmpty(firstkey))
                firstkey = firstkey.ToLower();

            if (string.IsNullOrEmpty(firstkey) || firstkey.ToLower() == "name" || firstkey.ToLower() == "image" ||
              firstkey.ToLower() == "mediatype")
                return parsedMetadata;


            if (firstkey == "files" && name.ToLower()=="name" || name.ToLower()=="src" || name.ToLower()=="mediatype")
            {
                return parsedMetadata;
            }


            string header = "";
            if (keys.Length > 1)
            {
                header = keys[keys.Length-2];
            }

            string description = "";

            if (token.Parent!=null && token.Parent.Type == JTokenType.Property && keys.Length>1)
            {
                description = keys[keys.Length - 1];
                ArrayHelper.RemoveAt(ref keys, keys.Length - 1);
            }
            /*
            if ((keys.Length ==3 && firstkey!="files") || (keys.Length==4 && firstkey=="files"))
            {
                description = keys[keys.Length - 1];
                ArrayHelper.RemoveAt(ref keys, keys.Length - 1);
            }*/

            string key=string.Join('/',keys);

            var field = parsedMetadata.MetadataFields.LastOrDefault(f => f.Key == key);

            if (firstkey.ToLower() == "files")
            {
                field = parsedMetadata.MetadataFiles.LastOrDefault()?.MetadataFields.FirstOrDefault(f => f.Key == key);
            }

            MetadataFieldTypes type = token.Type switch
            {
                JTokenType.Boolean => MetadataFieldTypes.Boolean,
                JTokenType.Integer => MetadataFieldTypes.Integer,
                _ =>MetadataFieldTypes.String,
            };

            if (field == null)
            {
                if (firstkey.ToLower() == "files")
                {
                    ArrayHelper.RemoveAt(ref keys, 0);
                    var displaykey = string.Join('/', keys);
                    field = new Metadatafields() { Key = key, DisplayKey = displaykey, FieldType = arrayindex != null ? MetadataFieldTypes.Array: type, FieldName = description == name ? "" : name, Header = header };
                    parsedMetadata.MetadataFiles.LastOrDefault()?.MetadataFields.Add(field);
                }
                else
                {
                    field = new Metadatafields() { Key = key,DisplayKey = key, FieldType = arrayindex != null ? MetadataFieldTypes.Array : type, FieldName = description==name?"": name, Header = header};
                    parsedMetadata.MetadataFields.Add(field);
                }
            }
            field.FieldValues.Add(new Metadatafield(){Description = description, Value = token.ToString(), FieldType=type});

            return parsedMetadata;
        }


        private ParsedMetadata CheckForFilesName(ParsedMetadata parsedMetadata, JToken token, string[] lastkeys, int depth, int arrayindex)
        {
            if (lastkeys != null && lastkeys.Any() && lastkeys.Last().ToLower() == "name" && depth == 6 && lastkeys[lastkeys.Length - 2].ToLower() =="files")
            {
                if (parsedMetadata.MetadataFiles.Count <= arrayindex)
                {
                    parsedMetadata.MetadataFiles.Add(new Metadatafiles(){FileName = token.ToString()});
                }
                else
                {
                    parsedMetadata.MetadataFiles[arrayindex].FileName = token.ToString();
                }
            }
            return parsedMetadata;
        }

        private ParsedMetadata CheckForFilesMimetype(ParsedMetadata parsedMetadata, JToken token, string[] lastkeys, int depth, int arrayindex)
        {
            if (lastkeys != null && lastkeys.Any() && lastkeys.Last().ToLower() == "mediatype" && depth == 6 && lastkeys[lastkeys.Length - 2].ToLower() == "files")
            {
                if (parsedMetadata.MetadataFiles.Count <= arrayindex)
                {
                    parsedMetadata.MetadataFiles.Add(new Metadatafiles() { MimeType = token.ToString() });
                }
                else
                {
                    parsedMetadata.MetadataFiles[arrayindex].MimeType = token.ToString();
                }
            }
            return parsedMetadata;
        }

        private ParsedMetadata CheckForFilesSrc(ParsedMetadata parsedMetadata, JToken token, string[] lastkeys, int depth, int arrayindex)
        {
            if (lastkeys != null && lastkeys.Any() && lastkeys.Last().ToLower() == "src" && depth == 6 && lastkeys[lastkeys.Length-2].ToLower() == "files")
            {
                if (parsedMetadata.MetadataFiles.Count <= arrayindex)
                {
                    parsedMetadata.MetadataFiles.Add(new Metadatafiles() { FileSrc = token.ToString() });
                }
                else
                {
                    parsedMetadata.MetadataFiles[arrayindex].FileSrc = token.ToString();
                }
            }
            return parsedMetadata;
        }


        private ParsedMetadata CheckForPreviewImageMimetype(ParsedMetadata parsedMetadata, JToken token, string[] lastkeys, int depth)
        {
            if (lastkeys != null && lastkeys.Any() && lastkeys.Last().ToLower() == "mediatype" && depth == 4)
            {
                parsedMetadata.PreviewImageMimeType = token.ToString();
            }
            return parsedMetadata;
        }

     
        private ParsedMetadata CheckForPreviewImageSrc(ParsedMetadata parsedMetadata, JToken token, string[] lastkeys, int depth)
        {
            if (lastkeys != null && lastkeys.Any() && lastkeys.Last().ToLower() == "image" && depth == 4)
            {
                parsedMetadata.PreviewImageSrc = token.ToString();
            }

            parsedMetadata.PreviewImageSrc ??= "";
            if (lastkeys != null && lastkeys.Any() && lastkeys.Last().ToLower() == "image" && depth == 5) // && (parsedMetadata.PreviewImageSrc.StartsWith("data:") || token.ToString().StartsWith("data:")))
            {
                parsedMetadata.PreviewImageSrc+= token.ToString();
            }
            return parsedMetadata;
        }

        private ParsedMetadata CheckForTokenName(ParsedMetadata parsedMetadata, string propertyName, string[] lastkeys, int depth)
        {
            if (!string.IsNullOrEmpty(parsedMetadata.MetadataType) && !string.IsNullOrEmpty(parsedMetadata.PolicyId) && depth == 2)
            {
                parsedMetadata.TokenName = propertyName;
            }
            return parsedMetadata;
        }

        private ParsedMetadata CheckForPolicyId(ParsedMetadata parsedMetadata, string propertyName, string[] lastkeys, int depth)
        {
            if (!string.IsNullOrEmpty(parsedMetadata.MetadataType) && depth == 1 && (propertyName.Length == 56 || propertyName == "<policy_id>"))
            {
                parsedMetadata.PolicyId=propertyName;
            }
            return parsedMetadata;
        }

        private ParsedMetadata CheckForMetadataTypeKey(ParsedMetadata parsedMetadata, string propertyName, string[] lastkeys, int depth)
        {
            if (propertyName == "721" || propertyName=="20" && depth==0)
            {
                parsedMetadata.MetadataType = propertyName;
            }

            return parsedMetadata;
        }
    }
}
