using System.Threading.Tasks;
using NMKR.Shared.Classes;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Mvc;

namespace NMKR.Api.Controllers.v2.Paymenttransactions.Processing
{
    public interface IProcessPaymentTransactionInterface
    {
        public Task<IActionResult> ProcessTransaction(EasynftprojectsContext db, string apikey, string remoteipaddress, ApiErrorResultClass result,
            Preparedpaymenttransaction preparedtransaction, object postparameter1);

    }
}
