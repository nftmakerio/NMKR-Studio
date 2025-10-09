using System;
using System.Collections.Generic;
using System.Linq;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Model;
using NMKR.Shared.PythonClasses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NMKR.Shared.Functions.Metadata
{
    public interface IMetadataParserInterface
    {
        public ParsedMetadata ParseMetadata(string metadata);
    }

    public enum MetadataStandard
    {
        Unknown,
        CIP68,
        CIP25
    }
    public class Metadatafiles {
        public string FileName { get; set; }
        public string MimeType { get; set; }
        public string FileSrc { get; set; }

        public string Category
        {
            get
            {
                switch (MimeType)
                {
                    case "image/png":
                    case "image/jpeg":
                    case "image/gif":
                    case "image/webp":
                        return "image";
                    case "audio/mpeg":
                    case "audio/ogg":
                    case "audio/wav":
                        return "audio";
                    case "video/mp4":
                    case "video/ogg":
                    case "video/webm":
                        return "video";
                    default:
                        return "misc";
                }
            }
        }

        public string FilesSrcIpfsHash => FileSrc?.Replace("ipfs://", "");
        public List<Metadatafields> MetadataFields { get; set; } = new List<Metadatafields>();
        public int ArrayIndex { get; set; }
    }

    public interface IMetadataFieldBaseClass
    {

    }
    public class Metadatafield 
    {
        public string Description { get; set; }
        public string Value { get; set; }
        public MetadataFieldTypes FieldType { get; set; }
    }

    public enum MetadataFieldTypes
    {
        String,
        Boolean,
        Integer,
        Array,
        List
    }

    public class Metadatafields 
    {
        public string Key { get; set; }
        public string DisplayKey { get; set; }
        public string FieldName { get; set; }
        public List<Metadatafield> FieldValues { get; set; } = new List<Metadatafield>();
        public string Header { get; set; }
        public MetadataFieldTypes FieldType { get; set; }
    }
    public class ParsedMetadata
    {
        public string MetadataType { get; set; }
        public string PolicyId { get; set; }
        public string TokenName { get; set; }
        public string Version { get; set; }
        public MetadataStandard MetadataStandard { get; set; }
        public string PreviewImageSrc { get; set; }
        public string PreviewImageMimeType { get; set; }
        public List<Metadatafiles> MetadataFiles { get; set; } = new List<Metadatafiles>();
        public List<Metadatafields> MetadataFields { get; set; } = new List<Metadatafields>();
        public string Log { get; set; }

        public string PreviewImageSrcIpfsHash => string.IsNullOrEmpty(PreviewImageSrc)?"": PreviewImageSrc.Replace("ipfs://", "");
        public string Storage
        {
            get
            {
                if (string.IsNullOrEmpty(PreviewImageSrc))
                {
                    return "unknown";
                }
                if (PreviewImageSrc.StartsWith("data:"))
                {
                    return "inline";
                }
                if (PreviewImageSrc.Contains("ipfs://"))
                {
                    return "IPFS";
                }
                if (PreviewImageSrc.Contains("https://") || PreviewImageSrc.Contains("http://"))
                {
                    return "URL";
                }

                if (PreviewImageSrc.Contains("iagon://"))
                {
                    return "Iagon";
                }

                return "unknown";
            }
        }

        public string PreviewImage
        {
            get
            {
                if (Storage == "inline")
                {
                    return PreviewImageSrc;
                }

                if (Storage == "URL")
                {
                    return PreviewImageSrc;
                }

                if (Storage == "Iagon")
                {
                    return PreviewImageSrc;
                }
                return GeneralConfigurationClass.IPFSGateway + PreviewImageSrcIpfsHash;
            }
        }

        public string OriginalMetadata { get; set; }
        public string Cip68Metadata { get; set; }

        public string ToCip68Metadata(string extrafield)
        {
            if (MetadataStandard == MetadataStandard.CIP68)
                return JsonConvert.SerializeObject(OriginalMetadata, Formatting.Indented);

            if (string.IsNullOrEmpty(Cip68Metadata))
            {

                try
                {
                    Cip68Metadata = ConvertCip25MetadataToCip68.ConverMetadata(OriginalMetadata,extrafield, PolicyId, TokenName);
                }
                catch
                {
                    Cip68Metadata = ToCip68Fallback(extrafield);
                }
            }

            return Cip68Metadata;
        }


        private string ToCip68Fallback(string extrafield)
        {
            SmartContractDatumClass smdc = new SmartContractDatumClass(0, null);
            var map1 = new SmartContractFieldsMapClass();
            map1.Map.Add(new SmartContractKeyValuesBytesFieldClass("name".ToHex(), TokenName.ToHexFilterPlaceholder()));
            map1.Map.Add(new SmartContractKeyValuesBytesFieldClass("image".ToHex(), PreviewImageSrc.ToHexFilterPlaceholder()));
            map1.Map.Add(new SmartContractKeyValuesBytesFieldClass("mediaType".ToHex(), PreviewImageMimeType.ToHexFilterPlaceholder()));

            map1 = AddFieldsToMap(map1, MetadataFields);
            map1 = AddFilesToMap(map1);

            smdc.Fields.Add(map1);

            // Plutus Data Extrafield
            if (!string.IsNullOrEmpty(extrafield))
            {
                if (extrafield.StartsWith("0x"))
                {
                    extrafield = extrafield.Substring(2);
                    smdc.Fields.Add(new SmartContractFieldsBytesClass(extrafield));
                }
                else
                {
                    smdc.Fields.Add(new SmartContractFieldsBytesClass(extrafield.ToHexFilterPlaceholder()));
                }
            }

            // Version
            smdc.Fields.Add(new SmartContractFieldsIntsClass(1));

            return JsonConvert.SerializeObject(smdc, Formatting.Indented);
        }

        private SmartContractFieldsMapClass AddFilesToMap(SmartContractFieldsMapClass map1)
        {
            var fileslist = new SmartContractFieldsListClass();
            foreach (var meta1File in MetadataFiles)
            {
                var map2 = new SmartContractFieldsMapClass();

                map2.Map.Add(new SmartContractKeyValuesBytesFieldClass("mediaType".ToHex(), meta1File.MimeType.ToHexFilterPlaceholder()));
                if (!string.IsNullOrEmpty(meta1File.FileName))
                    map2.Map.Add(new SmartContractKeyValuesBytesFieldClass("name".ToHex(), meta1File.FileName.ToHexFilterPlaceholder()));

                map2.Map.Add(new SmartContractKeyValuesBytesFieldClass("src".ToHex(), meta1File.FileSrc.ToHexFilterPlaceholder()));

                map2 = AddFieldsToMap(map2, meta1File.MetadataFields);

                fileslist.list.Add(map2);
            }

            var fileslistx = new SmartContractKeyBytesValuesFieldsClass("files".ToHex(), fileslist);
            map1.Map.Add(fileslistx);

            return map1;
        }

        private SmartContractFieldsMapClass AddFieldsToMap(SmartContractFieldsMapClass map1, List<Metadatafields> fields)
        {
            foreach (var field in fields)
            {
                switch (field.FieldType)
                {
                    case MetadataFieldTypes.String:
                        {
                            if (field.FieldValues.Count <= 1)
                            {
                                map1.Map.Add(new SmartContractKeyValuesBytesFieldClass(field.Key.ToHex(),
                                    field.FieldValues.FirstOrDefault()?.Value.ToHexFilterPlaceholder()));
                            }
                            else
                            {
                                var fieldArraylist = new SmartContractFieldsMapClass();
                                foreach (var fieldFieldValue in field.FieldValues)
                                {
                                    switch (fieldFieldValue.FieldType)
                                    {
                                        case MetadataFieldTypes.String:
                                            fieldArraylist.Map.Add(new SmartContractKeyValuesBytesFieldClass(fieldFieldValue.Description.ToHex(), fieldFieldValue.Value.ToHexFilterPlaceholder()));
                                            break;
                                        case MetadataFieldTypes.Boolean:
                                            fieldArraylist.Map.Add(new SmartContractKeyValuesBooleanFieldClass(fieldFieldValue.Description.ToHex(), fieldFieldValue.Value.ToLower() == "true"));
                                            break;
                                        case MetadataFieldTypes.Integer:
                                            fieldArraylist.Map.Add(new SmartContractKeyValuesIntFieldClass(fieldFieldValue.Description.ToHex(), Convert.ToInt64(fieldFieldValue.Value)));
                                            break;
                                        default:
                                            fieldArraylist.Map.Add(new SmartContractKeyValuesBytesFieldClass(fieldFieldValue.Description.ToHex(), fieldFieldValue.Value.ToHexFilterPlaceholder()));
                                            break;
                                    }
                                 //   var map2 = new SmartContractKeyValuesBytesFieldClass(fieldFieldValue.Description.ToHex(), fieldFieldValue.Value.ToHexFilterPlaceholder());
                                 //   fieldArraylist.Map.Add(map2);
                                }

                                var fieldsarraylistx =
                                    new SmartContractKeyBytesValuesFieldsClass(field.Header.ToHex(), fieldArraylist);
                                map1.Map.Add(fieldsarraylistx);
                            }

                            break;
                        }
                    case MetadataFieldTypes.Boolean:
                    {
                            map1.Map.Add(new SmartContractKeyValuesBooleanFieldClass(field.Key.ToHex(),
                                field.FieldValues.FirstOrDefault()?.Value.ToLower() == "true"));

                        break;
                    }
                    case MetadataFieldTypes.Integer:
                    {
                        map1.Map.Add(new SmartContractKeyValuesIntFieldClass(field.Key.ToHex(),
                           Convert.ToInt64(field.FieldValues.FirstOrDefault()?.Value)));

                            break;
                    }
                    case MetadataFieldTypes.Array:
                        {
                            var fieldArraylist = new SmartContractFieldsListClass();
                            var map2 = new SmartContractFieldsMapClass();
                            bool usemap2 = false;
                            foreach (var fieldFieldValue in field.FieldValues)
                            {
                                if (string.IsNullOrEmpty(fieldFieldValue.Description))
                                {
                                    fieldArraylist.list.Add(new SmartContractFieldsBytesClass(fieldFieldValue.Value.ToHex()));
                                }
                                else
                                {
                                    usemap2 = true;

                                    switch (fieldFieldValue.FieldType)
                                    {
                                        case MetadataFieldTypes.String:
                                            map2.Map.Add(new SmartContractKeyValuesBytesFieldClass(
                                                fieldFieldValue.Description.ToHex(),
                                                fieldFieldValue.Value.ToHexFilterPlaceholder()));
                                            break;
                                        case MetadataFieldTypes.Boolean:
                                            map2.Map.Add(new SmartContractKeyValuesBooleanFieldClass(
                                                fieldFieldValue.Description.ToHex(),
                                                fieldFieldValue.Value.ToLower() == "true"));
                                            break;
                                        case MetadataFieldTypes.Integer:
                                            map2.Map.Add(new SmartContractKeyValuesIntFieldClass(
                                                fieldFieldValue.Description.ToHex(),
                                                Convert.ToInt64(fieldFieldValue.Value)));
                                            break;
                                        default:
                                            map2.Map.Add(new SmartContractKeyValuesBytesFieldClass(
                                                fieldFieldValue.Description.ToHex(),
                                                fieldFieldValue.Value.ToHexFilterPlaceholder()));
                                            break;
                                    }

                                    /*    map2.Map.Add(new SmartContractKeyValuesBytesFieldClass(
                                            fieldFieldValue.Description.ToHex(),
                                            fieldFieldValue.Value.ToHexFilterPlaceholder()));*/
                                }
                            }

                            if (usemap2)
                                fieldArraylist.list.Add(map2);
                            
                            var fieldsarraylistx =
                                new SmartContractKeyBytesValuesFieldsClass(string.IsNullOrEmpty(field.DisplayKey) ? field.FieldName.ToHex() : field.DisplayKey.ToHex(), fieldArraylist);
                            map1.Map.Add(fieldsarraylistx);
                            break;
                        }
                    default:
                        foreach (var fieldFieldValue in field.FieldValues)
                        {
                            map1.Map.Add(new SmartContractKeyValuesBytesFieldClass(field.Key.ToHex(),
                                fieldFieldValue.Value.ToHexFilterPlaceholder()));
                        }
                        break;
                }
            }

            return map1;
        }
        public string ToCip25Metadata()
        {
            if (MetadataStandard == MetadataStandard.CIP25)
                return JsonConvert.SerializeObject(OriginalMetadata, Formatting.Indented);

            // TODO: Implement to CIP25 conversion
            return "";
        }

        public string ToAptosMetadata(Nftproject project)
        {
            var files = new JObject(
                new JProperty("files", GetSolanaMetadataFileSection(MetadataFiles)));

            var attributes = GetSolanaMetadataAttributes(new JArray(), MetadataFields);

            JObject rss =
                new JObject(
                    new JProperty("name", TokenName),
                    new JProperty("image", GeneralConfigurationClass.IPFSGateway + PreviewImageSrcIpfsHash));

          /*  if (project.IntegratecardanopolicyIdinmetadata == true && project.Enabledcoins.Contains(Coin.ADA.ToString()) == true)
                rss.Add(new JProperty("cardano_policyid", project.Policyid));*/

            if (files.Count > 0)
                rss.Add(new JProperty("properties", files));


            rss.Add(new JProperty("description",GetDescription(project)));
            
            if (!string.IsNullOrEmpty(project.Projecturl))
                rss.Add(new JProperty("external_url", project.Projecturl));
            if (!string.IsNullOrEmpty(project.Twitterurl))
                rss.Add(new JProperty("twitter", project.Twitterurl));
            if (!string.IsNullOrEmpty(project.Discordurl))
                rss.Add(new JProperty("discord", project.Discordurl));

            if (attributes.Count > 0)
                rss.Add(new JProperty("attributes", attributes));

            return rss.ToString();
        }


        public string ToSolanaMetadata(Nftproject project)
        {
            var files = new JObject(
                    new JProperty("category", "image"),
                    new JProperty("files", GetSolanaMetadataFileSection(MetadataFiles)));

            var attributes = GetSolanaMetadataAttributes(new JArray(), MetadataFields);

            JObject rss =
                new JObject(
                    new JProperty("name", TokenName),
                    new JProperty("symbol", project.Solanasymbol),
                    new JProperty("image", GeneralConfigurationClass.IPFSGateway + PreviewImageSrcIpfsHash));

            if (project.IntegratecardanopolicyIdinmetadata == true && project.Enabledcoins.Contains(Coin.ADA.ToString()) == true)
                rss.Add(new JProperty("cardano_policyid", project.Policyid));

            if (!string.IsNullOrEmpty(project.Solanacollectiontransaction))
            {
                rss.Add(new JProperty("collection", GetCollectionInformation(project)));
            }

            if (files.Count>0)
                rss.Add(new JProperty("properties", files));
            rss.Add(new JProperty("description",GetDescription(project)));

            if (!string.IsNullOrEmpty(project.Projecturl))
                rss.Add(new JProperty("external_url", project.Projecturl));
            if (!string.IsNullOrEmpty(project.Twitterurl))
                rss.Add(new JProperty("twitter", project.Twitterurl));
            if (!string.IsNullOrEmpty(project.Discordurl))
                rss.Add(new JProperty("discord", project.Discordurl));

            if (attributes.Count > 0)
                rss.Add(new JProperty("attributes", attributes));

            return rss.ToString();
        }

        private string GetDescription(Nftproject project)
        {
            var d = MetadataFields.FirstOrDefault(x => x.FieldName == "description");
           return d == null ? project.Description : d.FieldValues.FirstOrDefault()?.Value;
        }

        private JArray GetCollectionInformation(Nftproject project)
        {
            var res = new JArray
            {
                new JObject(
                    new JProperty("name", project.Projectname),
                    new JProperty("family", project.Solanacollectionfamily)
                )
            };
            return res;
        }

        private JArray GetSolanaMetadataAttributes(JArray array, IEnumerable<Metadatafields> fields)
        {
            foreach (var field in fields)
            {
                if (field.FieldValues.Count == 1 && string.IsNullOrEmpty(field.FieldValues.First().Description))
                    array.Add(
                        new JObject(
                        new JProperty("trait_type",field.FieldName), 
                        new JProperty("value", field.FieldValues.First().Value)));
                if (field.FieldValues.Count == 1 && !string.IsNullOrEmpty(field.FieldValues.First().Description))
                    array.Add(
                        new JObject(
                            new JProperty("trait_type", field.FieldValues.First().Description),
                            new JProperty("value", field.FieldValues.First().Value)));
                if (field.FieldValues.Count > 1)
                {
                    var obj = new JObject();
                    foreach (var fieldFieldValue in field.FieldValues)
                    {
                        if (!string.IsNullOrEmpty(fieldFieldValue.Description))
                            obj.Add(new JProperty(fieldFieldValue.Description, fieldFieldValue.Value));
                        else
                        {
                            obj.Add(new JValue(fieldFieldValue.Value));
                        }
                    }

                    array.Add(new JProperty(field.FieldName, obj));
                }
            }

            return array;
        }

        private JArray GetSolanaMetadataFileSection(IEnumerable<Metadatafiles> p1)
        {
            JArray res = new JArray();
            foreach (var files in p1)
            {
                var obj = new JObject(
                    new JProperty("type", files.MimeType),
                    new JProperty("uri",GeneralConfigurationClass.IPFSGateway+ files.FilesSrcIpfsHash)
                );
                if (files.MetadataFields.Count > 0)
                {
                    var attributes = GetSolanaMetadataAttributes(new JArray(), files.MetadataFields);
                    obj.Add(new JProperty("attributes", attributes));
                }
                res.Add(obj);
            }
            return res;
        }
    }
}
