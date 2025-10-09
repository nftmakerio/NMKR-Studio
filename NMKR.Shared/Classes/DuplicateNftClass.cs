using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.EntityFrameworkCore;

namespace NMKR.Shared.Classes
{
    public delegate void DuplicateNftProgress(int value, int max);
    public class DuplicateNftClass
    {
        public event DuplicateNftProgress ProgressEvent;

        private int _countDuplicates = 1;
        private string _tokennameprefix = "";
        private string _tokennamesuffix = "";
        private string _displaynameprefix = "";
        private string _displaynamesuffix = "";
        private int _startingNumber = 1;
        private int _leadingZeros = 4;

        public event Action OnChange;
        private void NotifyStateChanged() => OnChange?.Invoke();

        public DuplicateNftClass()
        {
            UpdateSample();
        }
        public int CountDuplicates
        {
            get { return _countDuplicates;}
            set
            {
                _countDuplicates = value;
                UpdateSample();
            }
        }
        public string Tokennameprefix
        {
            get { return _tokennameprefix;}
            set
            {
                _tokennameprefix = GlobalFunctions.FilterTokenname(value);
                UpdateSample();
            }
        }
        public string Tokennamesuffix
        {
            get { return _tokennamesuffix; }
            set
            {
                _tokennamesuffix = GlobalFunctions.FilterTokenname(value);
                UpdateSample();
            }
        }

        public string Displaynameprefix
        {
            get { return _displaynameprefix; }
            set
            {
                _displaynameprefix = value;
                UpdateSample();
            }
        }
        public string Displaynamesuffix
        {
            get { return _displaynamesuffix; }
            set
            {
                _displaynamesuffix = value;
                UpdateSample();
            }
        }

        public int StartingNumber
        {
            get { return _startingNumber; }
            set
            {
                _startingNumber = value;
                UpdateSample();
            }
        }
        public int LeadingZeros
        {
            get { return _leadingZeros; }
            set
            {
                _leadingZeros = value;
                UpdateSample();
            }
        }
        private string _tokennameSample = "";

        public string TokennameSample => _tokennameSample;

        private string _displaySample = "";
        public string DisplaySample => _displaySample;
        public bool SetDisplayName { get; set; } = false;

        private void UpdateSample()
        {
            if (_countDuplicates == 1)
            {
                _tokennameSample = GetTokenName(0);
                _displaySample = GetDisplayName(0);
            }
            else
            {
                _tokennameSample = " from: " + GetTokenName(0) + " to: " + GetTokenName(_countDuplicates-1);
                _displaySample = " from: " + GetDisplayName(0) + " to: " + GetDisplayName(_countDuplicates-1);
            }

            NotifyStateChanged();
        }

        public string GetTokenName(int i)
        {
           return _tokennameprefix + (_startingNumber+i).ToString("D" + _leadingZeros) + _tokennamesuffix;
        }
        public string GetDisplayName(int i)
        {
            return _displaynameprefix + (_startingNumber + i).ToString("D" + _leadingZeros) + _displaynamesuffix;
        }

        public async Task StartDuplicating(EasynftprojectsContext db, string nftuid)
        {
            var nft = await (from a in db.Nfts
                    .Include(a => a.InverseMainnft)
                    .ThenInclude(a => a.Metadata)
                    .AsSplitQuery()
                    .Include(a => a.Metadata)
                    .AsSplitQuery()
                    .Include(a => a.Nftproject)
                where a.Uid == nftuid 
                select a).AsNoTracking().FirstOrDefaultAsync();

            var nfts = await(from a in db.Nfts
                where a.NftprojectId == nft.NftprojectId && a.State != "deleted" && a.MainnftId == null
                select a).AsNoTracking().ToListAsync();
            db.ChangeTracker.AutoDetectChangesEnabled = false;
            for (int i = 0; i < CountDuplicates; i++)
            {
                ProgressEvent?.Invoke(i,CountDuplicates);
                var tokenname = GetTokenName(i);
                var displayname = SetDisplayName ? GetDisplayName(i) : null;

                // Skip if exists
                if (nfts.Exists(x => x.Name == tokenname))
                    continue;


                var nftnew = await InsertNewNft(db, nft, tokenname, displayname, nft.Nftproject.Policyid, nft.Nftproject.Tokennameprefix ,null);
                await Insertmetadata(db, nft.Metadata, nftnew.Id);

                foreach (var nft2 in nft.InverseMainnft)
                {
                    var subnft = await InsertNewNft(db, nft2, tokenname, displayname, nft.Nftproject.Policyid, nft.Nftproject.Tokennameprefix, nftnew.Id);
                    await Insertmetadata(db, nft2.Metadata, subnft.Id);
                }
                nfts.Add(nftnew);
            }
            db.ChangeTracker.AutoDetectChangesEnabled = true;
        }
        private async Task Insertmetadata(EasynftprojectsContext db, ICollection<Metadata> nftMetadata, int newid)
        {
            foreach (var metadatum in nftMetadata)
            {
                Metadata meta = new() { NftId = newid, Placeholdername = metadatum.Placeholdername, Placeholdervalue = metadatum.Placeholdervalue };
                await db.Metadata.AddAsync(meta);
            }
            await db.SaveChangesAsync();
        }

        private async Task<Nft> InsertNewNft(EasynftprojectsContext db, Nft nft, string tokenname, string displayname, string policyid,string tokennameprefix, int? mainNftId)
        {
            var n1 = new Nft
            {
                NftprojectId = nft.NftprojectId,
                Buildtransaction = null,
                Burncount = 0,
                Checkpolicyid = false,
                Created = DateTime.Now,
                Detaildata = nft.Detaildata,
                Errorcount = 0,
                Filename = "",
                Filesize = 0, // Duplicated files will not count
                Fingerprint = null,
                Initialminttxhash = null,
                Ipfshash = nft.Ipfshash,
                Isroyaltytoken = false,
                Markedaserror = null,
                Metadataoverride = nft.Metadataoverride,
                Metadataoverridecip68 = nft.Metadataoverridecip68,
                Mimetype = nft.Mimetype,
                Mintingfees = null,
                Lastpolicycheck = null,
                Minted = false,
                Mintingfeespaymentaddress = null,
                Mintingfeestransactionid = null,
                MetadatatemplateId = nft.MetadatatemplateId,
                Uploadedtonftstorage = nft.Uploadedtonftstorage,
                Title = nft.Title,
                Transactionid = null,
                Testmarker = 0,
                Soldcount = 0,
                Soldby = null,
                Series = nft.Series,
                Selldate = null,
                Reserveduntil = null,
                Reservedcount = 0,
                Price = nft.Price,
                Policyid = policyid,
                Receiveraddress = null,
                MainnftId = mainNftId,
                Uid = Guid.NewGuid().ToString(),
                State = "free",
                Name = tokenname,
                Displayname = displayname,
                Id = 0,
                
                Multiplier = nft.Multiplier,
                Uploadsource = nft.Uploadsource + " (Duplicated)",
                Iagonid = nft.Iagonid,
                Iagonuploadresult = nft.Iagonuploadresult,
                Solanacollectionnft = nft.Solanacollectionnft,
                Solanatokenhash = "",
                Pricesolana = nft.Pricesolana,
            };
            n1.Assetid = GlobalFunctions.GetAssetId(policyid, tokennameprefix, n1.Name);
            n1.Assetname = null; 
            await db.Nfts.AddAsync(n1);
            await db.SaveChangesAsync();
            return n1;
        }
    }
}
