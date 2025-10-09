using System;
using Newtonsoft.Json;

namespace NMKR.Shared.Classes.IAMX
{
    public partial class IamxStatusResultClass
    {
        [JsonProperty("_id", NullValueHandling = NullValueHandling.Ignore)]
        public string _Id { get; set; }

        [JsonProperty("iamxReference", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? IamxReference { get; set; }

        [JsonProperty("KYCStatus", NullValueHandling = NullValueHandling.Ignore)]
        public string KycStatus { get; set; }

        [JsonProperty("person", NullValueHandling = NullValueHandling.Ignore)]
        public IamxStatusResultClassPerson Person { get; set; }

        [JsonProperty("address", NullValueHandling = NullValueHandling.Ignore)]
        public IamxStatusResultClassAddress[] Address { get; set; }

        [JsonProperty("email", NullValueHandling = NullValueHandling.Ignore)]
        public IamxStatusResultClassEmail[] Email { get; set; }

        [JsonProperty("mobilephone", NullValueHandling = NullValueHandling.Ignore)]
        public IamxStatusResultClassEmail[] Mobilephone { get; set; }

        [JsonProperty("UUID", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? Uuid { get; set; }
    }

    public partial class IamxStatusResultClassAddress
    {
        [JsonProperty("street", NullValueHandling = NullValueHandling.Ignore)]
        public string Street { get; set; }

        [JsonProperty("housenr", NullValueHandling = NullValueHandling.Ignore)]
        public string Housenr { get; set; }

        [JsonProperty("zip", NullValueHandling = NullValueHandling.Ignore)]
        public string Zip { get; set; }

        [JsonProperty("city", NullValueHandling = NullValueHandling.Ignore)]
        public string City { get; set; }

        [JsonProperty("country", NullValueHandling = NullValueHandling.Ignore)]
        public string Country { get; set; }

        [JsonProperty("country_iso", NullValueHandling = NullValueHandling.Ignore)]
        public string CountryIso { get; set; }

        [JsonProperty("verification_methods", NullValueHandling = NullValueHandling.Ignore)]
        public string VerificationMethods { get; set; }

        [JsonProperty("verification_level", NullValueHandling = NullValueHandling.Ignore)]
        public long? VerificationLevel { get; set; }

        [JsonProperty("verification_timestamp", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? VerificationTimestamp { get; set; }
    }

    public partial class IamxStatusResultClassEmail
    {
        [JsonProperty("email", NullValueHandling = NullValueHandling.Ignore)]
        public string EmailEmail { get; set; }

        [JsonProperty("verification_source", NullValueHandling = NullValueHandling.Ignore)]
        public string VerificationSource { get; set; }

        [JsonProperty("verification_methods", NullValueHandling = NullValueHandling.Ignore)]
        public string VerificationMethods { get; set; }

        [JsonProperty("verification_timestamp", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? VerificationTimestamp { get; set; }

        [JsonProperty("phonenr", NullValueHandling = NullValueHandling.Ignore)]
        public string Phonenr { get; set; }
    }

    public partial class IamxStatusResultClassPerson
    {
        [JsonProperty("issuer", NullValueHandling = NullValueHandling.Ignore)]
        public string Issuer { get; set; }

        [JsonProperty("issuing_authority", NullValueHandling = NullValueHandling.Ignore)]
        public string IssuingAuthority { get; set; }

        [JsonProperty("issuing_date", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? IssuingDate { get; set; }

        [JsonProperty("valid_until", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? ValidUntil { get; set; }

        [JsonProperty("id_number", NullValueHandling = NullValueHandling.Ignore)]
        public string IdNumber { get; set; }

        [JsonProperty("firstname", NullValueHandling = NullValueHandling.Ignore)]
        public string Firstname { get; set; }

        [JsonProperty("lastname", NullValueHandling = NullValueHandling.Ignore)]
        public string Lastname { get; set; }

        [JsonProperty("nationality_iso", NullValueHandling = NullValueHandling.Ignore)]
        public string NationalityIso { get; set; }

        [JsonProperty("birthdate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? Birthdate { get; set; }

        [JsonProperty("birthplace", NullValueHandling = NullValueHandling.Ignore)]
        public string Birthplace { get; set; }

        [JsonProperty("verification_source", NullValueHandling = NullValueHandling.Ignore)]
        public string VerificationSource { get; set; }

        [JsonProperty("verification_methods", NullValueHandling = NullValueHandling.Ignore)]
        public string VerificationMethods { get; set; }

        [JsonProperty("verification_level", NullValueHandling = NullValueHandling.Ignore)]
        public long? VerificationLevel { get; set; }

        [JsonProperty("verification_timestamp", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? VerificationTimestamp { get; set; }
    }
}
