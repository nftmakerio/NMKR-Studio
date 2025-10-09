using System.Text.RegularExpressions;
using NMKR.Shared.Classes;

namespace NMKR.Shared.Functions
{
    public static class IpfsHelper
    {
        private static readonly string GATEWAY_BASE = GeneralConfigurationClass.IPFSGateway;
        private static readonly Regex CHECK_REGEX = new("^[a-z0-9A-Z:\\/]+$", RegexOptions.Compiled);

        /// <summary>
        /// Converts the specified IPFS hash or url to a https:// url pointing to the respective file.
        /// Performs some sanitizing and checks for unclean URLs
        /// </summary>
        /// <param name="ipfsHashOrUrl">IPFS hash or url</param>
        /// <returns>https:// url pointing to the respective file</returns>
        public static string ToIpfsGatewayUrl(string ipfsHashOrUrl)
        {
            if (string.IsNullOrEmpty(ipfsHashOrUrl))
            {
                return null;
            }

            if (!CHECK_REGEX.IsMatch(ipfsHashOrUrl))
            {
                return null;
            }

            if (ipfsHashOrUrl.StartsWith(GATEWAY_BASE))
            {
                return ipfsHashOrUrl;
            }

            // Trim the url in different manners to extract the hash
            if (ipfsHashOrUrl.StartsWith("ipfs://") && ipfsHashOrUrl.Length >= 7)
            {
                ipfsHashOrUrl = ipfsHashOrUrl[7..];
            }

            if (ipfsHashOrUrl.StartsWith("ipfs") && ipfsHashOrUrl.Length >= 5)
            {
                ipfsHashOrUrl = ipfsHashOrUrl[5..];
            }

            if (ipfsHashOrUrl.StartsWith("/") && ipfsHashOrUrl.Length >= 1)
            {
                ipfsHashOrUrl = ipfsHashOrUrl[1..];
            }

            return GATEWAY_BASE + $"{ipfsHashOrUrl}";
        }
    }
}