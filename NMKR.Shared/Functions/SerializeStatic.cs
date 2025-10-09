using Newtonsoft.Json.Linq;
using System;
using System.Reflection;

namespace NMKR.Shared.Functions
{
    public class StaticSerialization
    {
        public static JObject Serialize(Type staticClass)
        {
            var props = staticClass.GetProperties(BindingFlags.Static | BindingFlags.Public);
            var json = new JObject();
            foreach (var p in props)
            {
                var value = p.GetValue(null);
                if (value == null || !p.CanWrite || !p.CanRead) continue;
                json[p.Name] = JToken.FromObject(value);
            }

            foreach (var t in staticClass.GetNestedTypes())
                json[t.Name] = Serialize(t);
            return json;
        }

        public static void Deserialize(Type staticClass, JObject json)
        {
            if (json == null) return;
            var props = staticClass.GetProperties(BindingFlags.Static | BindingFlags.Public);
            foreach (var p in props)
            {
                if (!json.ContainsKey(p.Name) || !p.CanWrite) continue;
                p.SetValue(null, Convert.ChangeType(json[p.Name], p.PropertyType));
            }
            foreach (var t in staticClass.GetNestedTypes())
            {
                if (!json.ContainsKey(t.Name)) continue;
                Deserialize(t, json[t.Name] as JObject);
            }
        }
    }
}
