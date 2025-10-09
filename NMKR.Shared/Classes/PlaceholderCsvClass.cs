using System;
using System.Collections.Generic;
using System.Linq;
using NMKR.Shared.Model;

namespace NMKR.Shared.Classes
{
    public class NftNames
    {
        public string Tokenname { get; set; }
        public string Displayname { get; set; }
    }
    public class PlaceholderCsvClass
    {
        private readonly HashSet<string> _fieldnames = new();
        private readonly string _csv = null;
        private readonly Nftproject _project;
        public PlaceholderCsvClass(Nftproject project)
        {
            _project = project;
        }

        public bool HasPlaceholder
        {
            get { return _csv != ""; }
        }

        public string GetCsv()
        {
            if (_csv != null)
                return _csv;



            _fieldnames.Add("Filename");

            foreach (var projectNft in _project.Nfts)
            {
                foreach (var metadatum in projectNft.Metadata)
                {
                    _fieldnames.Add(metadatum.Placeholdername);
                }

                foreach (var nft2 in projectNft.InverseMainnft)
                {
                    foreach (var pl2 in nft2.Metadata)
                    {
                        _fieldnames.Add(pl2.Placeholdername);
                    }
                }
            }


            string csv = "";
            foreach (var fieldname in _fieldnames)
            {
                if (!string.IsNullOrEmpty(csv))
                    csv += ",";
                csv += fieldname;
            }

            csv += Environment.NewLine;
            
            foreach (var nft in _project.Nfts)
            {
                csv += string.IsNullOrEmpty(nft.Detaildata) ? nft.Name : nft.Detaildata;

                foreach (var fieldname in _fieldnames)
                {
                    csv += ",";
                    var t = nft.Metadata.FirstOrDefault(x => x.Placeholdername == fieldname);
                    if (t != null)
                        csv += t.Placeholdervalue;
                }
                csv += Environment.NewLine;

                foreach (var nft1 in nft.InverseMainnft)
                {
                    csv += string.IsNullOrEmpty(nft1.Detaildata) ? nft1.Name  : nft1.Detaildata;
                    foreach (var fieldname in _fieldnames)
                    {
                        csv += ",";
                        var t = nft1.Metadata.FirstOrDefault(x => x.Placeholdername == fieldname);
                        if (t != null)
                            csv += t.Placeholdervalue;
                    }


                }


                csv += Environment.NewLine;
            }
            return csv;
        }

    }
}
