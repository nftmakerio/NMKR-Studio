using NMKR.Shared.Classes.Koios;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMKR.Shared.DbSyncModel;
using Microsoft.EntityFrameworkCore;

namespace NMKR.Shared.Functions.DbSync
{
    public static class DbSyncFunctions
    {
        public static async Task<KoiosAssetListClass> GetPolicyidAndAssetnameFromFingerprintAsync(string fingerprint)
        {
            await using DbSyncContext db = new(GlobalFunctions.optionsBuilderDbSync.Options);

            var asset = await (from a in db.MultiAssets
                where a.Fingerprint == fingerprint
                               select a).AsNoTracking().FirstOrDefaultAsync();

            if (asset != null)
            {
                KoiosAssetListClass res = new KoiosAssetListClass()
                {
                    PolicyId = BitConverter.ToString(asset.Policy).Replace("-", "").ToLower() ,
                    AssetName= Encoding.UTF8.GetString(asset.Name).ToHex(),
                    Fingerprint=fingerprint
                };

                return res;
            }


            return null;
        }
    }
}
