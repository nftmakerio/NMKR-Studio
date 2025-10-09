using System;
using System.Collections.Generic;

namespace NMKR.Shared.Classes.RoyaltySplitAddresses
{
    public class EditSplitAddressClass : CreateSplitAddressClass
    {
        public int? Id { get; set; }
        public int CustomerId { get; set; }
    }
    public class CreateSplitAddressClass : CreateSplitAddressBaseClass
    {
        public string MainAddress { get; set; }
        public List<CreateSplits> Splits { get; set; } = new() { new() };
        public bool IsActive { get; set; }
    }
    public class CreateSplitAddressBaseClass
    {
        public string Comment { get; set; }
        public bool IsActive { get; set; }
        public long ThresholdInAda { get; set; }
    }
    public class CreateSplits
    {
        public string Address { get; set; }
        public int PercentageInt => (int)(Percentage * 100);
        public double Percentage { get; set; }
        public bool IsActive { get; set; }
        public DateTime? OptionalValidFromDate { get; set; }
        public DateTime? OptionalValidToDate { get; set; }
    }

    public class GetSplitAddressClass : CreateSplitAddressBaseClass
    {
        public string Address { get; set; }
        public DateTime Created { get; set; }
        public List<GetSplits> Splits { get; set; } = new() { new() };
        public long Lovelace { get; set; }
        public DateTime? Lastcheck { get; set; }
        public bool IsActice { get; set; }
    }

    public class GetSplits : CreateSplits
    {
        public DateTime Created { get; set; }
        public bool? IsMainReceiver { get; set; }
    }
}
