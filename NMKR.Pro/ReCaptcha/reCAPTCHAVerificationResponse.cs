using System;
using System.Text.Json.Serialization;

namespace NMKR.Pro.ReCaptcha
{
    public class reCAPTCHAVerificationResponse
    {
        public bool Success { get; set; }

        // timestamp of the challenge load (ISO format yyyy-MM-dd'T'HH:mm:ssZZ)
        [JsonPropertyName("challenge_ts")]
        public DateTimeOffset ChallengeTimestamp { get; set; }

        // the hostname of the site where the reCAPTCHA was solved
        public string Hostname { get; set; }

        [JsonPropertyName("error-codes")]
        public string[] ErrorCodes { get; set; } = Array.Empty<string>();
    }
}
