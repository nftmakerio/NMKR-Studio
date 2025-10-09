namespace NMKR.Shared.Model;

public partial class Ip2locationDb11
{
    public uint IpFrom { get; set; }

    public uint IpTo { get; set; }

    public string CountryCode { get; set; }

    public string CountryName { get; set; }

    public string RegionName { get; set; }

    public string CityName { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public string ZipCode { get; set; }

    public string TimeZone { get; set; }
}
