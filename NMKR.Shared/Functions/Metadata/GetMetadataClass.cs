using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using NMKR.Shared.Classes;
using NMKR.Shared.Model;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NMKR.Shared.Functions.Metadata
{
    public class GetMetadataClass
    {
        private readonly List<Nft> _internalnfts = new();
        private MetadataResultClass _metadataresult=new MetadataResultClass();
       
       
        public GetMetadataClass(NftIdWithMintingAddressClass nft, bool takeAlsoPremintedNfts = false, EasynftprojectsContext db = null) : this(new[] { nft }, takeAlsoPremintedNfts, db)
        {
        }
        public GetMetadataClass(int nftid, string mintingaddress, bool takeAlsoPremintedNfts = false, EasynftprojectsContext db = null) : this(new[] { new NftIdWithMintingAddressClass(nftid,mintingaddress) }, takeAlsoPremintedNfts, db)
        {
        }

        public GetMetadataClass(NftIdWithMintingAddressClass[] nfts,  bool takeAlsoPremintedNfts = false,
            EasynftprojectsContext db = null)
        {
            _nfts = nfts;
            _takeAlsoPremintedNfts = takeAlsoPremintedNfts;
            _db = db;

        }

        private NftIdWithMintingAddressClass[] _nfts;
        private bool _takeAlsoPremintedNfts;
        private EasynftprojectsContext _db;

        public MetadataResultClass MetadataResult()
        {
            var res = Task.Run(async () => await MetadataResultAsync());
            return res.Result;
        }

        public async Task<MetadataResultClass> MetadataResultAsync()
        {
            if (_nfts==null || !_nfts.Any())
                return _metadataresult;

            bool releasedb = false;
            if (_db == null)
            {
                _db = new(GlobalFunctions.optionsBuilder.Options);
                releasedb = true;
            }

            foreach (var nftid in _nfts)
            {
                var nft = await (from a in _db.Nfts
                        .Include(a => a.Nftproject)
                        .AsSplitQuery()
                        .Include(a => a.InverseMainnft)
                        .ThenInclude(a => a.Metadata)
                        .AsSplitQuery()
                        .Include(a => a.Metadata)
                        .AsSplitQuery()
                           where a.Id == nftid.NftId
                           select a).AsNoTracking().FirstOrDefaultAsync();

                _internalnfts.Add(nft);
            }

            if (_nfts.Length == 1 && _internalnfts.First().Nftproject.Cip68 &&
                !string.IsNullOrEmpty(_internalnfts.First().Metadataoverridecip68))
            {
                _metadataresult.MetadataCip68 = _internalnfts.First().Metadataoverridecip68;
                _metadataresult.SourceType = MetadataSourceTypes.Cip68;
                if (releasedb)
                    await _db.Database.CloseConnectionAsync();
                return _metadataresult;
            }


            if (!_internalnfts.First().Nftproject.Cip68)
            {
                _metadataresult = CreateMetadataNew(_internalnfts.ToArray(), _internalnfts.First().Nftproject.Metadata,
                    _takeAlsoPremintedNfts);
            }


            if (_internalnfts.First().Nftproject.Cip68)
            {
                _metadataresult = CreateMetadataNew(_internalnfts.ToArray(), _internalnfts.First().Nftproject.Metadata,
                    _takeAlsoPremintedNfts);
                try
                {
                    if (_metadataresult.MetadataCip25.Contains("\"721\""))
                    {
                        var chm = new ParseCip25Metadata();
                        var x = chm.ParseMetadata(_metadataresult.MetadataCip25);
                        _metadataresult.MetadataCip68 = x.ToCip68Metadata(
                            ConsoleCommand.GetCip68Extrafield(_internalnfts.First().Nftproject, _nfts[0].MintingAddress));
                        _metadataresult.SourceType = MetadataSourceTypes.Cip25;
                    }
                    else
                    {
                        _metadataresult.Error = "Error in Metadata";
                    }
                }
                catch (Exception e)
                {
                    _metadataresult.Error = e.Message;
                }
            }


            if (releasedb)
                await _db.Database.CloseConnectionAsync();
            return _metadataresult;
        }

        private MetadataResultClass CreateMetadataNew(Nft[] nft, string metadatatemplate, bool takeAlsoPremintedNfts = false)
        {
            if (!nft.Any())
                return new MetadataResultClass();

            // When takeall == true - take all nft - even those which are already minted
            Nft[] nft1Unminted = nft;
            if (takeAlsoPremintedNfts == false)
            {
                nft1Unminted = nft.Where(x => x.InstockpremintedaddressId == null && x.Fingerprint == null).ToArray();
                if (!nft1Unminted.Any())
                    return new MetadataResultClass();
            }

            try
            {
                string version = "1.0";
                if (metadatatemplate.Contains("\"version\": \"2"))
                    version = "2.0";

                List<CreateMetadataClass> meta = new();
                foreach (var n1 in nft1Unminted)
                {
                    CreateMetadataClass cmc = new();
                    cmc.Template = !string.IsNullOrEmpty(n1.Metadataoverride) ? n1.Metadataoverride : metadatatemplate;

                    // Remove the Formatting
                    var obj = JsonConvert.DeserializeObject(cmc.Template);
                    cmc.Template = JsonConvert.SerializeObject(obj, Formatting.None);

                    if (!string.IsNullOrEmpty(n1.Metadataoverride))
                    {
                        cmc.FinalTemplate = n1.Metadataoverride;
                        cmc.FinalTemplate = ChangeDefaultPlaceholder(cmc.FinalTemplate, n1, n1, version);
                        cmc.FinalTemplate = LookForSubfilePlaceholder(cmc.FinalTemplate, n1.InverseMainnft.ToList());
                        cmc.FinalTemplate = LookForPlaceholder(cmc.FinalTemplate, n1);
                    }
                    else
                    {
                        // Extract the Files Section
                        string searchfor = "\"files\":[";
                        var ux = cmc.Template.IndexOf(searchfor, StringComparison.OrdinalIgnoreCase);
                        if (ux != -1)
                        {
                            string meta1 = cmc.Template.Substring(0, ux);
                            string mx = cmc.Template.Substring(ux);
                            var tx = StringExtensions.FindClosingBracketIndex(mx, '[', ']');
                            string meta2 = mx.Substring(tx + 1);
                            cmc.Filessection = mx.Substring(0 + searchfor.Length, tx - searchfor.Length);
                            cmc.TemplateWithoutFiles = meta1 + "*filessection*" + meta2;
                        }
                        else
                        {
                            cmc.Filessection = "";
                            cmc.TemplateWithoutFiles = cmc.Template;
                        }

                        // Fill the Placeholder
                        cmc.TemplateWithoutFiles = ChangeDefaultPlaceholder(cmc.TemplateWithoutFiles, n1, n1, version);
                        cmc.TemplateWithoutFiles = LookForPlaceholder(cmc.TemplateWithoutFiles, n1);
                        cmc.CompleteFilesSection = "";

                        if (!string.IsNullOrEmpty(cmc.Filessection))
                        {
                            // Add the first file (if it is the only one)
                            if (!n1.InverseMainnft.Any())
                            {
                                var f1 = cmc.Filessection;
                                f1 = ChangeDefaultPlaceholder(f1, n1, n1,version);
                                f1 = LookForPlaceholder(f1, n1);
                                cmc.Files.Add(f1);
                                if (!string.IsNullOrEmpty(cmc.CompleteFilesSection))
                                    cmc.CompleteFilesSection += ",";
                                cmc.CompleteFilesSection += f1;
                            }


                            // Create the different File Sections
                            foreach (var n2 in n1.InverseMainnft)
                            {
                                var f1 = cmc.Filessection;
                                f1 = ChangeDefaultPlaceholder(f1, n2, n1,version);
                                f1 = LookForPlaceholder(f1, n2);
                                cmc.Files.Add(f1);
                                if (!string.IsNullOrEmpty(cmc.CompleteFilesSection))
                                    cmc.CompleteFilesSection += ",";
                                cmc.CompleteFilesSection += f1;
                            }


                            // Remove duplicates in files section
                            string files = "";
                            try
                            {
                                files = "{\"files\":[" + cmc.CompleteFilesSection + "]}";
                                files = RemoveDuplicatesInFilesSection(files);
                                if (!string.IsNullOrEmpty(files))
                                {
                                    files = files.Substring(1, files.Length - 2);
                                }
                            }
                            catch
                            {
                                files = "";
                            }

                            if (string.IsNullOrEmpty(files))
                            {
                                files = "\"files\":[" + cmc.CompleteFilesSection + "]";
                            }

                            // Add Files Array to Template
                            cmc.FinalTemplate = cmc.TemplateWithoutFiles.Replace("*filessection*",
                                files);
                        }
                        else
                        {
                            cmc.FinalTemplate = cmc.TemplateWithoutFiles;
                        }


                        // The the Solana Reference
                    /*    if (nft.First().Nftproject.Integratesolanacollectionaddressinmetadata == true)
                        {
                            string s = $"""
                                       "solanaCollectionAddress": "{nft.First().Nftproject.Solanapublickey}",
                                       """;

                            cmc.FinalTemplate=cmc.FinalTemplate.Replace("\"files\":",s+"\"files\":");
                        }*/
                    }

                    meta.Add(cmc);
                }

                // Combine all Tokens
                string st = meta.First().FinalTemplate;
                JObject o1 = JObject.Parse(st);

                for (int i = 1; i < meta.Count(); i++)
                {
                    JObject o2 = JObject.Parse(meta[i].FinalTemplate);
                    o1.Merge(o2, new()
                    {
                        MergeArrayHandling = MergeArrayHandling.Union
                    });
                }

                st = o1.ToString();
                return new MetadataResultClass() {MetadataCip25 = st, SourceType = MetadataSourceTypes.Cip25};
            }
            catch (Exception)
            {
                return new MetadataResultClass();
            }
        }

        private string RemoveDuplicatesInFilesSection(string json)
        {
            // Get your JObject as you've already shown
            var obj = JObject.Parse(json);

            // Use LINQ to create a List<JToken> of unique values based on ID
            // In this case the first occurence of the ID will be kept, repeats are removed
            var unique = obj["files"].GroupBy(x => x["src"]).Select(x => x.First()).ToList();

            // Iterate backwards over the JObject to remove any duplicate keys
            for (int i = obj["files"].Count() - 1; i >= 0; i--)
            {
                var token = obj["files"][i];
                if (!unique.Contains(token))
                {
                    token.Remove();
                }
            }

            // Re-serialize into JSON
            var result = JsonConvert.SerializeObject(obj);
            return result;
        }

        private string LookForSubfilePlaceholder(string s, List<Nft> subfiles)
        {
            for (int i = 0; i < subfiles.Count; i++)
            {
                s = s.Replace("<ipfs_link_subfile_" + (i + 1) + ">", "ipfs://" + subfiles[i].Ipfshash);
                s = s.Replace("<iagon_link_subfile_" + (i + 1) + ">", "iagon://" + subfiles[i].Iagonid);
                s = s.Replace("<gateway_link_subfile_" + (i + 1) + ">", GeneralConfigurationClass.IPFSGateway + subfiles[i].Ipfshash);
                s = s.Replace("<mime_type_subfile_" + (i + 1) + ">", subfiles[i].Mimetype);
                s = s.ReplaceWithArray("<display_name_subfile_" + (i + 1) + ">", EscapeForJson(subfiles[i].Displayname));
               // s = s.Replace("<display_name_subfile_" + (i + 1) + ">", subfiles[i].Displayname);
                s = s.Replace("<asset_name_subfile_" + (i + 1) + ">", subfiles[i].Name);
                s = s.Replace("<nft_name_subfile_" + (i + 1) + ">", subfiles[i].Name);
                s=s.ReplaceWithArray("<detail_data_subfile_" + (i + 1) + ">", EscapeForJson(subfiles[i].Detaildata));
               // s = s.Replace("<detail_data_subfile_" + (i + 1) + ">", subfiles[i].Detaildata);
            }
            return s;
        }
        public static string EscapeForJson(string s)
        {
            if (string.IsNullOrEmpty(s))
                return "";

            if (s.Trim().StartsWith("[") || s.Trim().StartsWith("{"))
                return s;

            var quoted = JsonEncodedText.Encode(s);
            return quoted.ToString();
        }
        private string ChangeDefaultPlaceholder(string s, Nft nft1, Nft mainNft, string version)
        {
            string name = GlobalFunctions.FilterTokenname((mainNft.Nftproject.Tokennameprefix ?? "") + nft1.Name);
            /*name += mainNft.Nftproject.Tokennameprefix.Replace(" ", "");
        name += nft1.Name.Replace(" ", "");
            */
            s = s.ReplaceWithArray("<project_description>", EscapeForJson(mainNft.Nftproject.Description));
            s =s.ReplaceWithArray("<series_description>", EscapeForJson(mainNft.Nftproject.Description));
            //s = s.Replace("<series_description>", EscapeForJson(mainNft.Nftproject.Description.Truncate(64)));
            s = s.ReplaceWithArray("<series_name>", EscapeForJson(mainNft.Nftproject.Projectname));
            s = s.ReplaceWithArray("<project_name>", EscapeForJson(mainNft.Nftproject.Projectname));
            //  s = s.Replace("<series_name>", EscapeForJson(mainNft.Nftproject.Projectname.Truncate(64)));
            s = s.Replace("<nft_name>", EscapeForJson(nft1.Name.Truncate(64)));
            s = s.Replace("<ipfs_link>", "ipfs://" + nft1.Ipfshash);
            s = s.Replace("<iagon_link>", "iagon://" + nft1.Iagonid);
            s = s.Replace("<gateway_link>", GeneralConfigurationClass.IPFSGateway + nft1.Ipfshash);
            s = s.Replace("<policy_name>", EscapeForJson(name));
            s = s.Replace("<tokenname_prefix>", EscapeForJson(mainNft.Nftproject.Tokennameprefix.Truncate(64)));
            s = s.Replace("<policy_id>", mainNft.Nftproject.Policyid);
            s = s.Replace("<asset_name>",
                version.StartsWith("2") ? EscapeForJson(name.Truncate(64)).ToHex() : EscapeForJson(name.Truncate(64)));
            s = s.Replace("<asset_name_hex>", EscapeForJson(name.Truncate(64)).ToHex());
            s = s.Replace("<asset_name_ascii>", EscapeForJson(name.Truncate(64)));
            s = s.Replace("<mime_type>", nft1.Mimetype);

            s = s.ReplaceWithArray("<description>", EscapeForJson(nft1.Detaildata));
            s = s.ReplaceWithArray("<detail_data>", EscapeForJson(nft1.Detaildata));
            // s = s.Replace("<description>", EscapeForJson(nft1.Detaildata.Truncate(64)));
          //  s = s.Replace("<detail_data>", EscapeForJson(nft1.Detaildata.Truncate(64)));
            s = s.Replace("<display_name>", EscapeForJson(string.IsNullOrEmpty(mainNft.Displayname) ? name.Truncate(64) : mainNft.Displayname.Truncate(64)));
            s = s.Replace("<version>", "1.0");

            if (mainNft.Nftproject.Integratesolanacollectionaddressinmetadata == true)
            {
                s = s.Replace("<solana_collection_address>", mainNft.Nftproject.Solanapublickey);
            }


            return s;
        }

      

        private string LookForPlaceholder(string s, Nft nft1)
        {
            // Add Placeholder
            int i = 0;
            do
            {
                string st = s.Between("<", ">");
                if (string.IsNullOrEmpty(st))
                    break;

                // Not more than 100 Placeholder - and to prevent a endless loop
                i++;
                if (i > 100)
                    break;

                foreach (var metadatum in nft1.Metadata)
                {
                    if (metadatum.Placeholdername.Trim().ToLower() != st.Trim().ToLower()) continue;
                    metadatum.Placeholdervalue = metadatum.Placeholdervalue.Trim();
                    if (metadatum.Placeholdervalue.StartsWith("[") && metadatum.Placeholdervalue.EndsWith("]"))
                        s = s.ReplaceInsensitive("\"<" + st + ">\"", EscapeForJson(metadatum.Placeholdervalue));
                    else
                        s = s.ReplaceWithArrayInsensitive("<" + st + ">", EscapeForJson(metadatum.Placeholdervalue));
                        //s = s.ReplaceInsensitive("<" + st + ">", EscapeForJson(metadatum.Placeholdervalue));
                    break;
                }
                s = s.Replace("<" + st + ">", "");
            } while (true);

            return s;
        }



    }
}
