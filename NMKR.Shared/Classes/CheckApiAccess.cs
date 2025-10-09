using System;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using StackExchange.Redis;

namespace NMKR.Shared.Classes
{
    public static class CheckApiAccess
    {

        public static ApiErrorResultClass CheckApiKey(EasynftprojectsContext db, string apikey, string ipaddress, out Apikey resutapikey)
        {
            resutapikey = null;

            ApiErrorResultClass arc = new() {ErrorCode = 0, ErrorMessage = "", ResultState = ResultStates.Ok};

            if (string.IsNullOrEmpty(apikey))
            {
                arc.ResultState = ResultStates.Error;
                arc.ErrorMessage = "Apikey/Accesstoken is not correct";
                arc.ErrorCode = 1;
                return arc;
            }

            string hash = HashClass.GetHash(SHA256.Create(), apikey);


            var t = (from a in db.Apikeys
                where a.Apikeyhash == hash
                select a).AsNoTracking().FirstOrDefault();

            if (t == null)
            {
                arc.ResultState = ResultStates.Error;
                arc.ErrorMessage = "Apikey/Accesstoken is not correct";
                arc.ErrorCode = 1;
                return arc;
            }

            resutapikey = t;

            if (t.State == "revoked")
            {
                arc.ResultState = ResultStates.Error;
                arc.ErrorMessage = "Apikey is not correct";
                arc.ErrorCode = 1;
                return arc;
            }

            if (t.State == "deleted")
            {
                arc.ResultState = ResultStates.Error;
                arc.ErrorMessage = "Apikey is not correct";
                arc.ErrorCode = 1;
                return arc;
            }

            if (t.Expiration < DateTime.Now)
            {
                arc.ResultState = ResultStates.Error;
                arc.ErrorMessage = "Apikey is expired";
                arc.ErrorCode = 1;
                return arc;
            }

            // TODO: Check IP ADDRESSES 

            // Check IP Address rules

      /*      var access = (from a in db.Apikeyaccesses
                where a.ApikeyId == t.Id
                orderby a.Order
                select a).ToList();

            if (!access.Any()) return arc;
            var allowed = false;
            foreach (var c in from c in access let rangeB1 = IPAddressRange.Parse(c.Accessfrom) where rangeB1.Contains(ipaddress) select c)
            {
                allowed = c.IsActive switch
                {
                    "allowed" => true,
                    "forbidden" => false,
                    _ => allowed
                };

                break;
            }
      */
            // TODO: Change this
            var allowed = true;

            if (allowed) return arc;
            arc.ResultState = ResultStates.Error;
            arc.ErrorMessage = $"IP Address ({ipaddress}) is not allowed for this operation";
            arc.ErrorCode = 9;
            return arc;

        }


        public static ApiErrorResultClass CheckApiKey(EasynftprojectsContext db, string apikey, string ipaddress, int nftprojectid)
        {
            ApiErrorResultClass arc = CheckApiKey(db, apikey, ipaddress, out var t);
            
            if (arc.ResultState != ResultStates.Ok)
                return arc;

            var project = (from a in db.Nftprojects
                           where a.Id == nftprojectid && a.CustomerId==t.CustomerId
                           select a).FirstOrDefault();

            if (project==null)
            {
                arc.ResultState = ResultStates.Error;
                arc.ErrorMessage = "Project Id is not valid";
                arc.ErrorCode = 4;
                return arc;
            }

            if (project.CustomerId != t.CustomerId)
            {
                arc.ResultState = ResultStates.Error;
                arc.ErrorMessage = "Access to this Project denied";
                arc.ErrorCode = 2;
                return arc;
            }

            if (project.State == "notactive")
            {
                arc.ResultState = ResultStates.Error;
                arc.ErrorMessage = "NFT Project is not active";
                arc.ErrorCode = 5;
                return arc;
            }
        /*    if (project.State == "finished" && accesstype == AccessTypes.UploadNft)
            {
                arc.ResultState = ResultStates.Error;
                arc.ErrorMessage = "NFT Project is finished";
                arc.ErrorCode = 6;
                return arc;
            }*/
            if (project.State == "deleted")
            {
                arc.ResultState = ResultStates.Error;
                arc.ErrorMessage = "NFT Project is not active";
                arc.ErrorCode = 7;
                return arc;
            }
          
            return arc;
        }


        public static ApiErrorResultClass CheckApiKey(EasynftprojectsContext _db, string apikey, string ipaddress, string projectuid)
        {
            ApiErrorResultClass arc = CheckApiKey(_db, apikey, ipaddress, out var t);

            if (arc.ResultState != ResultStates.Ok)
                return arc;

            var project = (from a in _db.Nftprojects
                           where a.Uid == projectuid && a.CustomerId == t.CustomerId
                           select a).FirstOrDefault();

            if (project != null) return CheckApiKey(_db, apikey, ipaddress, project.Id);
            arc.ResultState = ResultStates.Error;
            arc.ErrorMessage = "Project UId is not valid or Project does not belong to this Apikey";
            arc.ErrorCode = 4;
            return arc;
        }

        public static Customer GetCustomer(IConnectionMultiplexer redis, EasynftprojectsContext db, string apikey, int? setCheckaddresscount=null)
        {
            if (apikey.StartsWith("token"))
            {
                IDatabase dbr = redis.GetDatabase();
                string customerid = dbr.StringGet("customerid_"+apikey);
                
                if (string.IsNullOrEmpty(customerid))
                    return null;

                var customer = (from a in db.Customers
                        .Include(a => a.Defaultsettings).AsSplitQuery()
                                where a.Id.ToString() == customerid
                    select a).FirstOrDefault();

                if (setCheckaddresscount != null && customer != null)
                {
                    customer.Checkaddresscount = (int) setCheckaddresscount;
                    db.SaveChangesAsync();
                }

                return customer;
            } 

            string hash = HashClass.GetHash(SHA256.Create(), apikey);
            var t = (from a in db.Apikeys
                    .Include(a => a.Customer)
                    .ThenInclude(a => a.Defaultsettings).AsSplitQuery()
                     where a.Apikeyhash == hash
                select a).FirstOrDefault();
            if (t == null)
                return null;

            if (setCheckaddresscount != null && t.Customer != null)
            {
                t.Customer.Checkaddresscount = (int)setCheckaddresscount;
                db.SaveChangesAsync();
            }

            return t.Customer;
        }
    }
}
