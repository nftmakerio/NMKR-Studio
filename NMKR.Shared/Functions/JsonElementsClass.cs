using Newtonsoft.Json.Linq;

namespace NMKR.Shared.Functions
{
    public static class JsonElementsClass
    {
        public static string Get(string jsonContent, string elementToFind)
        {
            if (string.IsNullOrEmpty(jsonContent))
                return jsonContent;

            JObject jsonObj = JObject.Parse(jsonContent);

            JToken foundValue = FindElement(jsonObj, elementToFind);

            if (foundValue != null)
            {
                return foundValue.ToString();
            }

            return string.Empty;
        }
        private static JToken FindElement(JToken token, string elementToFind)
        {
            if (token.Type == JTokenType.Object)
            {
                JObject obj = (JObject)token;

                // Durch jedes Property im Objekt iterieren
                foreach (JProperty prop in obj.Properties())
                {
                    // Wenn das Property den gesuchten Schlüssel enthält
                    if (prop.Name == elementToFind)
                    {
                        return prop.Value;
                    }

                    // Wenn das Property ein weiteres Objekt enthält, in diesem rekursiv suchen
                    JToken nestedResult = FindElement(prop.Value, elementToFind);
                    if (nestedResult != null)
                    {
                        return nestedResult;
                    }
                }
            }
            else if (token.Type == JTokenType.Array)
            {
                JArray array = token as JArray;

                if (array != null)
                {
                    // Durch jedes Element im Array iterieren
                    foreach (JToken arrayElement in array)
                    {
                        // In jedem Element rekursiv nach dem gesuchten Schlüssel suchen
                        JToken nestedResult = FindElement(arrayElement, elementToFind);
                        if (nestedResult != null)
                        {
                            return nestedResult;
                        }
                    }
                }
            }

            return null;
        }

    public static string Delete(string jsonContent, string elementToDelete)
        {
            if (string.IsNullOrEmpty(jsonContent))
                return jsonContent;

            JObject jsonObj = JObject.Parse(jsonContent);
            bool elementFound = DeleteElement(jsonObj, elementToDelete);

            if (elementFound)
            {
                return jsonObj.ToString();
            }

            return jsonContent;
        }
        private static bool DeleteElement(JToken token, string elementToDelete)
        {
            if (token.Type == JTokenType.Object)
            {
                JObject obj = (JObject)token;

                // Durch jedes Property im Objekt iterieren
                foreach (JProperty prop in obj.Properties())
                {
                    // Wenn das Property den zu löschenden Schlüssel enthält
                    if (prop.Name == elementToDelete)
                    {
                        prop.Remove();
                        return true;
                    }

                    // Wenn das Property ein weiteres Objekt enthält, in diesem rekursiv suchen
                    if (DeleteElement(prop.Value, elementToDelete))
                    {
                        return true;
                    }
                }
            }
            else if (token.Type == JTokenType.Array)
            {
                JArray array = token as JArray;

                if (array != null)
                {
                    // Durch jedes Element im Array iterieren
                    for (int i = 0; i < array.Count; i++)
                    {
                        // In jedem Element rekursiv nach dem zu löschenden Schlüssel suchen
                        if (DeleteElement(array[i], elementToDelete))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
