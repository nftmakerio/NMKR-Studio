using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace NMKR.Shared.Classes.Koios
{
    public partial class KoiosOldRoyaltyTokenClass
    {
        [JsonProperty("777")]
        public The777 The777 { get; set; }
    }

    public partial class The777
    {
        [JsonProperty("pct", NullValueHandling = NullValueHandling.Ignore)]
        public string Pct { get; set; }

        [JsonProperty("rate", NullValueHandling = NullValueHandling.Ignore)]
        public string Rate { get; set; }

        
        [JsonProperty("addr", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(ArrayToSingleConverter))]
        public string Addr { get; set; }
    }

    public class ArrayToSingleConverter : JsonConverter<string>
    {
        public override void WriteJson(JsonWriter writer, string value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override string ReadJson(JsonReader reader, Type objectType, string existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartArray)
            {
                return serializer.Deserialize<string>(reader);
            }

            List<string> list = new List<string>();

            reader.Read();

            while (reader.TokenType != JsonToken.EndArray)
            {
                list.Add(serializer.Deserialize<string>(reader));

                reader.Read();
            }

            return string.Join("", list);
        }
    }
}