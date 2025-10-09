using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NMKR.Shared.Functions.Metadata
{

    public class ParseCip68Metadata : IMetadataParserInterface
    {
        private class LastKeysClass
        {
            public string Key { get; set; }
            public int Depth { get; set; }
            public int Index { get; set; }
        }

        private List<LastKeysClass> lastKeys = new List<LastKeysClass>();

        public ParsedMetadata ParseMetadata(string metadata)
        {
            try
            {
                var jsonObject = JObject.Parse(metadata);
                ParsedMetadata pm = new ParsedMetadata();
                ParseJToken(jsonObject, jsonObject,new List<JsonPathClass>(), ref pm);

                if (string.IsNullOrEmpty(pm.TokenName))
                    throw new Exception("Missing TokenName");
                if (string.IsNullOrEmpty(pm.PreviewImageSrc))
                    throw new Exception("Missing Preview Image");

                pm.MetadataStandard = MetadataStandard.CIP68;
                pm.OriginalMetadata = metadata;

                return pm;
            }
            catch (Exception e)
            {
                throw new Exception($"Error parsing CIP68 metadata - {e.Message}", e);
            }
        }


        public class JsonPathClass
        {
            public string jsonpath { get; set; }
            public int pos { get; set; }
        }

        private int index = 0;

        private List<JsonPathClass> ParseJToken(JObject original, JToken token, List<JsonPathClass> path, ref ParsedMetadata pm, int depth = 0, int? arrayindex = null)
        {
            string prefix = new string(' ', depth * 2);

            for (var i = path.Count - 1; i >= 0; i--)
            {
                var p = path[i];
                if (depth <= p.pos)
                {
                    path.Remove(p);
                }
            }


            if (token.Path.EndsWith("v"))
            {
                string tok = ReplaceLastOccurrence(token.Path, ".v", ".k.bytes");
                var token2 = original.SelectToken(tok);
                path.Add(new JsonPathClass() { jsonpath = FromHexString(token2.ToString()), pos = depth });
            }


            switch (token.Type)
            {
                case JTokenType.Object:
                    foreach (var property in ((JObject)token).Properties())
                    {
                        Console.WriteLine($@"{prefix} {depth} Key: {property.Name}");

                        path = ParseJToken(original, property.Value, path,ref pm,
                            depth + 1, arrayindex);
                    }

                    break;
                case JTokenType.Array:
                    index++;
                    foreach (var value in ((JArray)token))
                    {
                        Console.WriteLine($@"{prefix} {depth} Index: {index}");
                        path = ParseJToken(original, value, path,ref pm, depth + 1, index);
                    }

                    break;
                default: // Für JTokenType.String, JTokenType.Integer, etc.
                    bool f = false;
                    if (token.Path.EndsWith("v.bytes"))
                    {
                        string s = FromHexString(token.ToString());
                        AddFieldString(path.Last().jsonpath, s, path,index, ref pm);
                        f = true;
                    }
                    if (token.Path.EndsWith("v.int"))
                    {
                        string s = token.ToString();
                        if (s.ToLower()=="true" || s.ToLower()=="false")
                            AddFieldBool(path.Last().jsonpath, s, path, index, ref pm);
                        else
                            AddFieldInt(path.Last().jsonpath, Convert.ToInt64(s), path, index, ref pm);
                        f = true;
                    }

                    if (!f && token.Path.EndsWith(".bytes") && token.Path.Contains(".v.list"))
                    {
                        string s = FromHexString(token.ToString());
                        AddFieldString(path.Last().jsonpath, s, path, index, ref pm);
                    }
                    break;
            }

            return path;
        }

        static string FromHexString(string hex)
        {
            var bytes = new byte[hex.Length / 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }

            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        private string ReplaceLastOccurrence(string source, string find, string replace)
        {
            int place = source.LastIndexOf(find);

            if (place == -1)
                return source;

            string result = source.Remove(place, find.Length).Insert(place, replace);
            return result;
        }

        private string WriteJsonPath(List<JsonPathClass> jsonpath)
        {
            string st = "";
            for (var i = 0; i < jsonpath.Count; i++)
            {
                var VARIABLE = jsonpath[i];
                if (!string.IsNullOrEmpty(st))
                    st += "/";
                st += VARIABLE.jsonpath;
            }

            return st;
        }

    

     
        private void AddFieldInt(string description, long intValue, List<JsonPathClass> jsonpath,int arrayindex, ref ParsedMetadata pm)
        {
            if (jsonpath.Any() && jsonpath.First().jsonpath == "files")
            {
                if (jsonpath.Count > 1)
                    AddFilesFieldx(description, intValue.ToString(), MetadataFieldTypes.Integer, jsonpath,arrayindex, ref pm);
            }
            else
            {
                AddFieldx(description, intValue.ToString(), MetadataFieldTypes.Integer, jsonpath, ref pm);
            }
            
        }

        private void AddFilesFieldx(string description, string toString, MetadataFieldTypes integer,
            List<JsonPathClass> jsonpath, int arrayindex, ref ParsedMetadata pm)
        {
            var files = pm.MetadataFiles.Find(x => x.ArrayIndex == arrayindex);
            if (files == null)
            {
                files = new Metadatafiles()
                {
                    ArrayIndex = arrayindex
                };
                pm.MetadataFiles.Add(files);
            }

            if (description.ToLower() == "name")
                files.FileName = toString;
            else if (description.ToLower() == "src")
                files.FileSrc = toString;
            else if (description.ToLower() == "mediatype")
                files.MimeType = toString;
            else
            {
                files.MetadataFields.Add(new Metadatafields()
                {
                    Key = description,
                    DisplayKey = description,
                    FieldType = integer,
                    FieldName = jsonpath.First().jsonpath,
                    FieldValues = new List<Metadatafield>()
                    {
                        new Metadatafield()
                        {
                            Description = "",
                            Value = toString
                        }
                    }
                });
            }
        }

        private void AddFieldString(string description, string stringValue, List<JsonPathClass> jsonpath,int arrayindex, ref ParsedMetadata pm)
        {
            if (jsonpath.Any() && jsonpath.First().jsonpath == "files")
            {
                if (jsonpath.Count > 1)
                    AddFilesFieldx(description, stringValue, MetadataFieldTypes.String, jsonpath, arrayindex, ref pm);
            }
            else
            {
                AddFieldx(description, stringValue,  MetadataFieldTypes.String, jsonpath, ref pm);
            }
        }
        private void AddFieldBool(string description, string boolValue, List<JsonPathClass> jsonpath, int arrayindex, ref ParsedMetadata pm)
        {
            if (jsonpath.Any() && jsonpath.First().jsonpath == "files")
            {
                if (jsonpath.Count > 1)
                    AddFilesFieldx(description, boolValue, MetadataFieldTypes.Boolean, jsonpath, arrayindex, ref pm);
            }
            else
            {
                AddFieldx(description, boolValue, MetadataFieldTypes.Boolean, jsonpath, ref pm);
            }
        }
        private void AddFieldx(string description, string stringValue,  MetadataFieldTypes type, List<JsonPathClass> jsonpath,
            ref ParsedMetadata pm)
        {
            if (jsonpath.Count == 1)
            {
                if (description.ToLower() == "name")
                    pm.TokenName = stringValue;
                else if (description.ToLower() == "image")
                    pm.PreviewImageSrc = stringValue;
                else if (description.ToLower() == "mediatype")
                    pm.PreviewImageMimeType = stringValue;
                else
                {
                    var field = pm.MetadataFields.FirstOrDefault(x => x.Key == description);
                    if (field != null)
                    {
                        field.FieldValues.Add(new Metadatafield()
                        {
                            Description = "",
                            Value = stringValue
                        });
                    }
                    else
                    {
                        pm.MetadataFields.Add(new Metadatafields()
                        {
                            Key = description,
                            DisplayKey = description,
                            FieldType = type,
                            FieldName = jsonpath.First().jsonpath,
                            FieldValues = new List<Metadatafield>()
                            {
                                new Metadatafield()
                                {
                                    Description = "",
                                    Value = stringValue
                                }
                            }
                        });
                    }
                }
            }
            else
            {
                string key = GetKey(jsonpath);
                var f = pm.MetadataFields.FirstOrDefault(x => x.Key ==  key);
                if (f == null)
                {
                    f = new Metadatafields()
                    {
                        Key = key,
                        DisplayKey = key,
                        FieldType = type,
                        FieldName = "",
                        Header = GetHeader(jsonpath),
                        FieldValues = new List<Metadatafield>()
                        {
                            new Metadatafield()
                            {
                                Description = description==jsonpath.First().jsonpath ? "": description,
                                Value = stringValue
                            }
                        }
                    };
                    pm.MetadataFields.Add(f);
                }
                else
                {
                    f.FieldValues.Add(new Metadatafield()
                    {
                        Description = description == jsonpath.First().jsonpath ? "" : description,
                        Value = stringValue
                    });
                }
            }
        }

        private string GetHeader(List<JsonPathClass> jsonpath)
        {
            if (jsonpath.Count > 1)
            {
                string st = "";
                for (var i = 1; i < jsonpath.Count-1; i++)
                {
                    var VARIABLE = jsonpath[i];
                    if (!string.IsNullOrEmpty(st))
                        st += "/";
                    st += VARIABLE.jsonpath;
                }

                return st;
            }

            return "";
        }

        private string GetKey(List<JsonPathClass> jsonpath)
        {
            if (jsonpath.Count > 1)
            {
                string st = "";
                for (var i = 0; i < jsonpath.Count-1; i++)
                {
                    var VARIABLE = jsonpath[i];
                    if (!string.IsNullOrEmpty(st))
                        st += "/";
                    st += VARIABLE.jsonpath;
                }

                return st;
            }

            return WriteJsonPath(jsonpath);
        }


        // OLD:




        

        private ParsedMetadata CheckForFields(ParsedMetadata parsedMetadata, JToken token, string[] keys, int? arrayindex)
        {
            if (keys.Length <= 3)
            {
                return parsedMetadata;
            }
            var name = keys.LastOrDefault();
            if (name!="bytes")
                return parsedMetadata;

            if (keys[^2] == "k")
            {
         }



         











            var value = token.ToString().FromHex();

           

            if (keys.Length>2 && (keys[^2] == "k"))
            {
                lastKeys.RemoveAll(x => x.Depth >= keys.Length);

                lastKeys.Add(new LastKeysClass(){Depth = keys.Length, Key = value, Index = arrayindex??0});
                return parsedMetadata;
            }

            if (keys.Length > 2 && keys[^2] == "v" && string.IsNullOrEmpty(lastKeys.LastOrDefault()?.Key))
            {
                return parsedMetadata;
            }
            string key = string.Join('/', keys);



            if (keys.Length > 2 && (keys[^2] == "v" || keys[^2] == "array"))
            {
                if (keys.Length == 4)
                {
                    switch (lastKeys.LastOrDefault()?.Key.ToLower())
                    {
                        case "name":    
                            parsedMetadata.TokenName = value;
                            break;
                      /*  case "description":
                            parsedMetadata.Description = value;
                            break;*/
                        case "image":
                            parsedMetadata.PreviewImageSrc = value;
                            break;
                        case "mediatype":
                            parsedMetadata.PreviewImageMimeType = value;
                            break;
                        default:
                            parsedMetadata=AddField(parsedMetadata,lastKeys, value, GetMetadataFieldType(keys[^2]));
                            break;
                    }
                }
                else
                {
                    var found = false;
                    if (keys.Length > 4 && lastKeys.Count>1)
                    {
                        if (lastKeys.First().Key.ToLower() == "files" && lastKeys.Count==2)
                        {
                            if (lastKeys.Last().Index==0)
                                parsedMetadata.MetadataFiles.Add(new Metadatafiles(){});

                            switch (lastKeys.LastOrDefault()?.Key.ToLower())
                            {
                                case "name":
                                    parsedMetadata.MetadataFiles[parsedMetadata.MetadataFiles.Count-1].FileName = value;
                                    break;
                                case "src":
                                    parsedMetadata.MetadataFiles[parsedMetadata.MetadataFiles.Count - 1].FileSrc = value;
                                    break;
                                case "mediatype":
                                    parsedMetadata.MetadataFiles[parsedMetadata.MetadataFiles.Count - 1].MimeType = value;
                                    break;
                                default:
                                    parsedMetadata = AddFilesField(parsedMetadata, lastKeys, value, GetMetadataFieldType(keys[^2]));
                                    break;
                            }

                            found = true;

                        }
                        if (lastKeys.First().Key.ToLower() == "files" && lastKeys.Count > 2)
                        {

                            parsedMetadata = AddFilesField(parsedMetadata, lastKeys, value, GetMetadataFieldType(keys[^2]));
                            found = true;

                        }
                    }

                    if (!found)
                    {
                        parsedMetadata = AddField(parsedMetadata, lastKeys, value, GetMetadataFieldType(keys[^2]));
                    }
                   
                }
            }

            if (keys.Length > 2 && (keys[^2] == "list"))
            {
                parsedMetadata = AddField(parsedMetadata, lastKeys, value, GetMetadataFieldType(keys[^2]));
            }


            return parsedMetadata;
        }

        private MetadataFieldTypes GetMetadataFieldType(string v)
        {
            return v switch
            {
                "array" => MetadataFieldTypes.Array,
                "list" => MetadataFieldTypes.List,
                "integer" => MetadataFieldTypes.Integer,
                "int" => MetadataFieldTypes.Integer,
                "boolean" => MetadataFieldTypes.Boolean,
                "bool" => MetadataFieldTypes.Boolean,
                _ => MetadataFieldTypes.String
            };
        }

        private ParsedMetadata AddFilesField(ParsedMetadata parsedMetadata, List<LastKeysClass> list, string value,  MetadataFieldTypes previouskey)
        {
            string key1 = "";
            string key2 = "";
            for (var i = 1; i < list.Count - 1; i++)
            {
                var lastKeysClass = list[i];
                if (!string.IsNullOrEmpty(key1))
                    key1 += "/";
                key1 += lastKeysClass.Key;

                if (i > 1)
                {
                    if (!string.IsNullOrEmpty(key2))
                        key2 += "/";
                    key2 += lastKeysClass.Key;
                }
            }
            

            if (previouskey == MetadataFieldTypes.Array || previouskey== MetadataFieldTypes.List)
            {
                key1 += "/" + lastKeys.LastOrDefault()?.Key;
                if (!string.IsNullOrEmpty(key2))
                    key2 += "/";
                key2 += lastKeys.LastOrDefault()?.Key;
            }


            if (string.IsNullOrEmpty(key2))
                key2 = key1;


            var f = parsedMetadata.MetadataFiles.LastOrDefault()?.MetadataFields.FirstOrDefault(x => x.Key == key1);
            if (f == null)
            {
                f = new Metadatafields()
                {
                    Key = key1,
                    DisplayKey = key2,
                    FieldType = (previouskey == MetadataFieldTypes.Array || previouskey == MetadataFieldTypes.List) ? MetadataFieldTypes.Array : MetadataFieldTypes.String,
                    FieldName = lastKeys.LastOrDefault()?.Key,
                };
                f.FieldValues.Add(new Metadatafield()
                {
                    Description = (previouskey == MetadataFieldTypes.Array || previouskey == MetadataFieldTypes.List) ? "" : list.LastOrDefault()?.Key,
                    Value = value
                });
                parsedMetadata.MetadataFiles.LastOrDefault()?.MetadataFields.Add(f);
            }
            else
            {
                f.FieldValues.Add(new Metadatafield()
                {
                    Description = (previouskey == MetadataFieldTypes.Array || previouskey == MetadataFieldTypes.List) ? "": list.LastOrDefault()?.Key,
                    Value = value
                });
            }

            return parsedMetadata;
        }

        private ParsedMetadata AddField(ParsedMetadata parsedMetadata, List<LastKeysClass> list,  string value, MetadataFieldTypes previouskey)
        {
            string key1 = "";
            string key2 = "";
            for (var i = 1; i < list.Count - 1; i++)
            {
                var lastKeysClass = list[i];
                if (!string.IsNullOrEmpty(key1))
                    key1 += "/";
                key1 += lastKeysClass.Key;

                if (i > 1)
                {
                    if (!string.IsNullOrEmpty(key2))
                        key2 += "/";
                    key2 += lastKeysClass.Key;
                }
            }

            if (previouskey == MetadataFieldTypes.Array || previouskey == MetadataFieldTypes.List)
            {
                key1 += "/" + lastKeys.LastOrDefault()?.Key;
                if (!string.IsNullOrEmpty(key2))
                    key2 += "/";
                key2 += lastKeys.LastOrDefault()?.Key;
            }

            if (string.IsNullOrEmpty(key1) && string.IsNullOrEmpty(key2) && list.Count>1)
            {
                key2 = list[^2].Key;
                key1 = key2;
            }

            var f = parsedMetadata.MetadataFields.FirstOrDefault(x => x.Key == key1);
            if (f == null)
            {
                f = new Metadatafields()
                {
                   Key = key1,
                   DisplayKey = key2,
                   FieldType = (previouskey == MetadataFieldTypes.Array || previouskey == MetadataFieldTypes.List) ? MetadataFieldTypes.Array :MetadataFieldTypes.String,
                    FieldName = lastKeys.LastOrDefault()?.Key,
                };
                f.FieldValues.Add(new Metadatafield()
                {
                    Description = (previouskey == MetadataFieldTypes.Array || previouskey == MetadataFieldTypes.List) ? "" : list.LastOrDefault()?.Key,
                    Value = value
                });
                parsedMetadata.MetadataFields.Add(f);
            }
            else
            {
                f.FieldValues.Add(new Metadatafield()
                {
                    Description = (previouskey == MetadataFieldTypes.Array || previouskey == MetadataFieldTypes.List) ? "" : list.LastOrDefault()?.Key,
                    Value = value
                });
            }

            return parsedMetadata;
        }
    }
}
