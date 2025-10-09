using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NMKR.Shared.Classes;
using NMKR.Shared.Model;
using Yoti.Auth;
using Yoti.Auth.DocScan;
using Yoti.Auth.DocScan.Session.Create;
using Yoti.Auth.DocScan.Session.Create.Check;
using Yoti.Auth.DocScan.Session.Create.Filter;
using Yoti.Auth.DocScan.Session.Create.Task;
using Yoti.Auth.DocScan.Session.Retrieve;
using Yoti.Auth.DocScan.Session.Retrieve.Resource;

namespace NMKR.Pro.Classes
{
    public class YotiClasses
    {

        private readonly DocScanClient _client;
        private readonly Uri _apiUrl;
       
        public YotiClasses()
        {
            _apiUrl = Yoti.Auth.Constants.Api.DefaultYotiDocsUrl;
            _client = GetDocScanClient(_apiUrl);
        }

      
        internal static DocScanClient GetDocScanClient(Uri apiUrl)
        {
            StreamReader privateKeyStream =
                System.IO.File.OpenText(AppDomain.CurrentDomain.BaseDirectory + "YotiSecurity.pem");
            var key = CryptoEngine.LoadRsaKey(privateKeyStream);

            string clientSdkId = GeneralConfigurationClass.YotaSdkId;

            return new(clientSdkId, key, new(), apiUrl);
        }

        public CreateSessionResult PrepareSession(string userid, string baseurl)
        {
            var sessionSpec = new SessionSpecificationBuilder()
                .WithClientSessionTokenTtl(1200)
                .WithResourcesTtl(90000)
                .WithUserTrackingId(userid)
                .WithRequestedCheck(
                    new RequestedDocumentAuthenticityCheckBuilder()
                        .Build())
                .WithRequestedCheck(
                    new RequestedWatchlistScreeningCheckBuilder()
                        .ForAdverseMedia()
                        .ForSanctions()
                        .Build()
                )
                .WithRequestedCheck(
                    new RequestedLivenessCheckBuilder()
                        .ForZoomLiveness()
                        .Build()
                )
                .WithRequestedCheck(
                    new RequestedFaceMatchCheckBuilder()
                        .WithManualCheckFallback()
                        .Build()
                )
                .WithRequestedTask(
                    new RequestedTextExtractionTaskBuilder()
                        .WithManualCheckFallback()
                        .WithChipDataDesired()
                        .Build()
                )
                .WithSdkConfig(
                    new SdkConfigBuilder()
                        .WithAllowsCameraAndUpload()
                        .WithPrimaryColour("#2d9fff")
                        .WithSecondaryColour("#FFFFFF")
                        .WithFontColour("#FFFFFF")
                        .WithLocale("en-GB")
                        .WithSuccessUrl($"{baseurl}idresult")
                        .WithErrorUrl($"{baseurl}idresult")
                        .WithPrivacyPolicyUrl($"https://www.nmkr.io/legal#privacy")
                        .WithAllowHandoff(true)
                        .Build()
                )
                /*       .WithRequiredDocument(
                           new RequiredSupplementaryDocumentBuilder()
                               .WithObjective(
                                   new ProofOfAddressObjectiveBuilder().Build())
                               .Build()
                       )*/
                .WithRequiredDocument(
                    new RequiredIdDocumentBuilder()
                        .WithFilter(
                            (new OrthogonalRestrictionsFilterBuilder())
                            .WithIncludedDocumentTypes(new() {"PASSPORT", "DRIVING_LICENCE", "NATIONAL_ID"})
                            .Build()
                        )
                        .Build()
                )

                .Build();


            CreateSessionResult createSessionResult = _client.CreateSession(sessionSpec);
            return createSessionResult;
        }

        public GetSessionResult GetSessionResult(string sessionid)
        {
            try
            {
                var sr = _client.GetSession(sessionid);
                return sr;
            }
            catch 
            {
                return null;
            }
        }

        public Uri GetApiUrl()
        {
            return _apiUrl;
        }

        public void SetKycState(EasynftprojectsContext db, AppSettings settings)
        {
            if (settings.UserId == null || settings.ShowKycAlert==false) return;

            var customer = (from a in db.Customers
                where a.Id == settings.UserId
                select a).FirstOrDefault();
            if (customer == null)
            {
                settings.ShowKycAlert = false;
                return;
            }

            if (customer.Showkycstate == false)
                settings.ShowKycAlert = false;


            if (!string.IsNullOrEmpty(customer.Kycaccesstoken) &&
                (customer.Kycstatus != "GREEN" && customer.Checkkycstate != "never"))
            {
                YotiClasses yc = new();
                var getSessionResult = yc.GetSessionResult(customer.Kycaccesstoken);
                if (getSessionResult != null)
                {
                    switch (getSessionResult.State)
                    {
                        case "COMPLETED" when getSessionResult.Checks.Any(checkResponse => checkResponse.Report.Recommendation.Value == "REJECT"):
                            customer.Kycstatus = "RED";
                            break;
                        case "COMPLETED":
                            customer.Kycstatus = "GREEN";
                            break;
                        case "EXPIRED":
                            customer.Kycstatus = "RED";
                            break;
                    }

                    db.SaveChanges();
                }

            }
            settings.KycState = customer.Kycstatus;
        }

        public async Task DownloadMediaData(GetSessionResult getSessionResult, string customerKycaccesstoken)
        {
            GetSessionResult sessionResult = await _client.GetSessionAsync(customerKycaccesstoken);

            // Returns all resources in the session
            ResourceContainer resources = sessionResult.Resources;

            // Returns a collection of ID Documents
            List<IdDocumentResourceResponse> idDocuments = resources.IdDocuments;

            foreach (IdDocumentResourceResponse idDocument in idDocuments)
            {
                // Gets the UUID of the document resource
                string id = idDocument.Id;

                // Returns the ID Document Type
                string documentType = idDocument.DocumentType;

                // Returns the ID Document country
                string issuingCountry = idDocument.IssuingCountry;

                // Returns the pages of an ID Document
                List<PageResponse> pages = idDocument.Pages;
                // Get the page media ids
                ArrayList pageMediaIds = new();
                foreach (PageResponse page in pages)
                {
                    if (page.Media != null && page.Media.Id != null)
                    {
                        pageMediaIds.Add(page.Media.Id);
                    }
                }

                // Returns document fields object
                DocumentFieldsResponse documentFields = idDocument.DocumentFields;
                //Get document fields media id
                string documentFieldsMediaId = null;
                if (documentFields != null)
                {
                    documentFieldsMediaId = documentFields.Media.Id;
                }
            }

        }
    }
}
