using System.Linq;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.EntityFrameworkCore;

namespace NMKR.Shared.Classes.CustomerData
{
    public class LockedCustomerAssetsCheck
    {
        public bool LockedAssetsEnabled { get; set; }
        public Vestingoffer ActiveVestingOffer { get; set; }
        private int _customerid;
        private long totalused = 0;
        private long totalnfts = 0;
        private long totalprojects = 0;
        public LockedCustomerAssetsCheck(int customerid)
        {
            using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            _customerid = customerid;
            var globalSetting = GlobalFunctions.GetWebsiteSettingsBool(db, "enablevestingtokens");
            if (globalSetting == false)
            {
                LockedAssetsEnabled = false;
                return;
            }

            var customer = (from a in db.Customers
                where a.Id == customerid
                select a).AsNoTracking().FirstOrDefault();
            if (customer == null)
            {
                LockedAssetsEnabled = false;
                return;
            }

            if (customer.Donotneedtolocktokens == true)
            {
                LockedAssetsEnabled = false;
                return;
            }

            LockedAssetsEnabled = true;

            var lockedtokens = (from a in db.Lockedassets
                    .Include(a => a.Lockedassetstokens)
                where a.CustomerId == customerid
                select a).AsNoTracking().ToList();

            var lockedtokensSum=lockedtokens.Sum(x=>x.Lockedassetstokens.Sum(x1=>x1.Count));

            ActiveVestingOffer = (from a in db.Vestingoffers
                where a.Vesttokenquantity <= lockedtokensSum
                orderby a.Vesttokenquantity descending
                select a).FirstOrDefault();
            LoadData(db);
        }

        public void LoadData(EasynftprojectsContext db)
        {
            totalused = (from a in db.Nftprojects
                where a.CustomerId == _customerid
                select a).Sum(x => x.Usedstorage);
            totalprojects = (from a in db.Nftprojects
                where a.CustomerId == _customerid
                             && (a.Projecttype== "nft-project" || a.Projecttype == "ft-project") && a.State != "deleted"
                             select a).Count();
             totalnfts = (from a in db.Nftprojects
                where a.CustomerId == _customerid
                select a).Sum(x => x.Total1);
        }


        public bool IsFurtherUploadAllowed()
        {
            if (LockedAssetsEnabled == false)
                return true;

            if (ActiveVestingOffer == null)
                return false;

            // Check total used storage
            if (totalused < ActiveVestingOffer.Maxstorage)
                return false;

            // Check total files
            if (totalnfts < ActiveVestingOffer.Maxfiles)
                return false;

            return true;
        }

        public bool IsExtendedApiusageAllowed()
        {
            if (LockedAssetsEnabled == false)
                return true;

            return ActiveVestingOffer is {Extendedapienabled: true};
        }

        public string GetTotalStorageUsedString()
        {
            if (ActiveVestingOffer==null)
                return GlobalFunctions.GetPrettyFileSizeString(totalused);
            return GlobalFunctions.GetPrettyFileSizeString(totalused) + " of " + GlobalFunctions.GetPrettyFileSizeString(ActiveVestingOffer.Maxstorage);
        }

        public string GetTotalProjectsString()
        {
            return totalprojects.ToString("N0");
        }

        public string GetTotalNftsString()
        {
            if (ActiveVestingOffer == null)
                return totalnfts.ToString("N0");

            return totalnfts.ToString("N0") + " of " + ActiveVestingOffer.Maxfiles.ToString("N0");
        }


        public string GetMaxfilesizeString()
        {
            if (ActiveVestingOffer == null)
                return GlobalFunctions.GetPrettyFileSizeString(0);
            return GlobalFunctions.GetPrettyFileSizeString(ActiveVestingOffer.Maxfilesize);
        }
    }
}
