using System.Collections.Generic;
using Newtonsoft.Json;

namespace NMKR.Shared.Classes
{
    public class SmartContractParameters
    {
        public string type { get; set; }
        public string value { get; set; }
        public long intvalue { get; set; }
    }
    public interface ISmartContractFieldsInterface
    {

    }

    public class SmartContractDatumClass : ISmartContractFieldsInterface
    {
        [JsonProperty("fields", NullValueHandling = NullValueHandling.Ignore)]
        public List<ISmartContractFieldsInterface> Fields { get; set; } = new List<ISmartContractFieldsInterface>();
        [JsonProperty("constructor", NullValueHandling = NullValueHandling.Ignore)]
        public long? Constructor { get; set; }

        public SmartContractDatumClass(long? constructor, string pkh, SmartContractParameters[] parameters=null )
        {
            Constructor = constructor;
            if (!string.IsNullOrEmpty(pkh))
                Fields.Add(new SmartContractFieldsBytesClass(pkh));

            if (parameters == null) return;
            foreach (var parameter in parameters)
            {
                switch (parameter.type)
                {
                    case "int":
                        Fields.Add(new SmartContractFieldsIntsClass(parameter.intvalue));
                        break;
                    case "bytes":
                        Fields.Add(new SmartContractFieldsBytesClass(parameter.value));
                        break;
                }
            }
        }
    }


    public class SmartContractFieldsClass : ISmartContractFieldsInterface
    {
        [JsonProperty("fields", NullValueHandling = NullValueHandling.Ignore)]
        public List<ISmartContractFieldsInterface> Fields { get; set; } = new List<ISmartContractFieldsInterface>();

        [JsonProperty("constructor", NullValueHandling = NullValueHandling.Ignore)]
        public long? Constructor { get; set; }


        public SmartContractFieldsClass(long? constructor, ISmartContractFieldsInterface[] fields)
        {
            Constructor = constructor;
            Fields.AddRange(fields);
        }
        public SmartContractFieldsClass(long? constructor, ISmartContractFieldsInterface fields)
        {
            Constructor = constructor;
            Fields.Add(fields);
        }

        public SmartContractFieldsClass(long? constructor)
        {
            Constructor = constructor;
        }
    }


    public class SmartContractFieldsBytesClass : ISmartContractFieldsInterface
    {
        [JsonProperty("bytes", NullValueHandling = NullValueHandling.Ignore)]
        public string Bytes { get; set; }
        public SmartContractFieldsBytesClass(string bytes)
        {
            Bytes = bytes;
        }   
    }


    public class SmartContractFieldsIntsClass : ISmartContractFieldsInterface
    {
        [JsonProperty("int", NullValueHandling = NullValueHandling.Ignore)]
        public long Int { get; set; }
        public SmartContractFieldsIntsClass(long intvalue)
        {
            Int = intvalue;
        }
    }
    public class SmartContractFieldsListClass : ISmartContractFieldsInterface
    {
     [JsonProperty("list", NullValueHandling = NullValueHandling.Ignore)]
        public List<ISmartContractFieldsInterface> list { get; set; } = new List<ISmartContractFieldsInterface>();
    }
    public class SmartContractFieldsIntListClass : SmartContractFieldsClass
    {
        public SmartContractFieldsIntListClass(int contructor, int intvalue, ISmartContractFieldsInterface fields) : base(contructor,fields)
        {
            Fields.Insert(0, new SmartContractFieldsIntsClass(intvalue));
        }
    }

    public class SmartContractFieldsMapClass : ISmartContractFieldsInterface
    {
        [JsonProperty("map", NullValueHandling = NullValueHandling.Ignore)]
        public List<ISmartContractFieldsInterface> Map { get; set; } = new List<ISmartContractFieldsInterface>();

        public SmartContractFieldsMapClass(string keyBytes, ISmartContractFieldsInterface value)
        {
            Map.Add(new SmartContractKeyBytesValuesFieldsClass(keyBytes,value));
        }
        public SmartContractFieldsMapClass()
        {
           
        }
    }


    public class SmartContractFieldsArrayClass : ISmartContractFieldsInterface
    {
        [JsonProperty("array", NullValueHandling = NullValueHandling.Ignore)]
        public List<ISmartContractFieldsInterface> Array { get; set; } = new List<ISmartContractFieldsInterface>();
        public SmartContractFieldsArrayClass()
        {

        }
    }


    public class SmartContractKeyBytesValuesFieldsClass : ISmartContractFieldsInterface
    {
        [JsonProperty("k", NullValueHandling = NullValueHandling.Ignore)]
        public SmartContractFieldsBytesClass K { get; set; }
        [JsonProperty("v", NullValueHandling = NullValueHandling.Ignore)]
        public ISmartContractFieldsInterface V { get; set; }

        public SmartContractKeyBytesValuesFieldsClass(string keyBytes, ISmartContractFieldsInterface value)
        {
            K = new SmartContractFieldsBytesClass(keyBytes);
            V = value;
        }
    }

    public class SmartContractKeyValuesBytesFieldClass : ISmartContractFieldsInterface
    {
        [JsonProperty("k", NullValueHandling = NullValueHandling.Ignore)]
        public SmartContractFieldsBytesClass K { get; set; }
        [JsonProperty("v", NullValueHandling = NullValueHandling.Ignore)]
        public ISmartContractFieldsInterface V { get; set; }

        public SmartContractKeyValuesBytesFieldClass(string keyBytes, string value)
        {
            K = new SmartContractFieldsBytesClass(keyBytes);
            V = new SmartContractFieldsBytesClass(value);
        }
    }
    public class SmartContractKeyValuesIntFieldClass : ISmartContractFieldsInterface
    {
        [JsonProperty("k", NullValueHandling = NullValueHandling.Ignore)]
        public SmartContractFieldsBytesClass K { get; set; }
        [JsonProperty("v", NullValueHandling = NullValueHandling.Ignore)]
        public ISmartContractFieldsInterface V { get; set; }

        public SmartContractKeyValuesIntFieldClass(string keyBytes, long value)
        {
            K = new SmartContractFieldsBytesClass(keyBytes);
            V = new SmartContractFieldsIntsClass(value);
        }
    }
    public class SmartContractKeyValuesBooleanFieldClass : ISmartContractFieldsInterface
    {
        [JsonProperty("k", NullValueHandling = NullValueHandling.Ignore)]
        public SmartContractFieldsBytesClass K { get; set; }
        [JsonProperty("v", NullValueHandling = NullValueHandling.Ignore)]
        public ISmartContractFieldsInterface V { get; set; }

        public SmartContractKeyValuesBooleanFieldClass(string keyBytes, bool value)
        {
            K = new SmartContractFieldsBytesClass(keyBytes);
            V = new SmartContractFieldsClass(value ? 1 : 0, new ISmartContractFieldsInterface[] { });
        }
    }
    public class IntV : ISmartContractFieldsInterface
    {
        [JsonProperty("int", NullValueHandling = NullValueHandling.Ignore)]
        public long intvalue { get; set; }

        public IntV(long intValue)
        {
            intvalue = intValue;
        }
    }
    
}
