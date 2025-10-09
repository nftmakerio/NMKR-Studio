using System.Collections.Generic;

namespace NMKR.Shared.Classes
{
    public class CsvValues
    {
        public string Value { get; set; }
        public int Id { get; set; }
    }
    public class SavePlaceHolderCsvClass
    {
        public string Name { get; set; }
        public List<CsvValues> Values = new();

        public SavePlaceHolderCsvClass(string name)
        {
            Name = name;
        }
    }
}
