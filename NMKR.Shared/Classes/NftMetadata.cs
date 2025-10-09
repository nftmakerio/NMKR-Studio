using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace NMKR.Shared.Classes
{
    public sealed class NFTFile
    {
        /// <summary>
        /// File URL
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The file's media type (if available)
        /// </summary>
        public string MediaType { get; set; }

        /// <summary>
        /// The IPFS Hash
        /// </summary>
        public string Hash { get; set; }

        public string Name { get; set; }
    }
    public sealed class NFTInformation : NFTMetadata
    {
        /// <summary>
        /// The date + time when the NFT has been minted
        /// </summary>
        public DateTime MintingDateTimeUtc { get; set; }

        public NFTInformation()
        {

        }

        public NFTInformation(NFTMetadata metadata)
        {
            Name = metadata.Name;
            Description = metadata.Description;
            Metadata = metadata.Metadata;
            PolicyId = metadata.PolicyId;
            TokenName = metadata.TokenName;
            PreviewImage = metadata.PreviewImage;
            Files = metadata.Files;
        }
    }

    public sealed class NFTAttribute
    {
        /// <summary>
        /// Attribute key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Attribute value (only if not nested, else null)
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Whether the attribute is nested.
        /// </summary>
        public bool IsNested { get; set; }
    }


    public class NFTMetadata
        {
            /// <summary>
            /// NFT name
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Description
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            /// Raw JSON metadata
            /// </summary>
            public string Metadata { get; set; }

            /// <summary>
            /// Policy id
            /// </summary>
            public string PolicyId { get; set; }

            /// <summary>
            /// Token name
            /// </summary>
            public string TokenName { get; set; }

            /// <summary>
            /// NFT preview image
            /// </summary>
            public NFTFile PreviewImage { get; set; }

            /// <summary>
            /// NFT other files
            /// </summary>
            public List<NFTFile> Files { get; set; } = new();

            public Dictionary<string, string[]> FieldsArray = new Dictionary<string, string[]>();

            public Dictionary<string, string> Fields = new Dictionary<string, string>();



            private readonly string[] ignoreMetadataKeys = new string[] { "name", "image", "mediaType", "description", "files" };

            /// <summary>
            /// Gets a list of all non-default NFT attributes
            /// </summary>
            /// <returns>list of all non-default NFT attributes</returns>
            public List<NFTAttribute> GetNonNestedAttributes()
            {
                var attributes = new List<NFTAttribute>();

                var jsonObject = JObject.Parse(Metadata);

                foreach (var item in jsonObject)
                {
                    if (!ignoreMetadataKeys.Contains(item.Key.ToString()))
                    {
                        if (item.Value is JValue)
                        {
                            attributes.Add(new()
                            {
                                Key = item.Key,
                                Value = item.Value.ToString(),
                                IsNested = false
                            });
                        }
                        else
                        {
                            attributes.Add(new()
                            {
                                Key = item.Key,
                                Value = null,
                                IsNested = true
                            });
                        }
                    }
                }

                return attributes;
            }
        }
}
