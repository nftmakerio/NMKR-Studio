namespace NMKR.Shared.Model;

public partial class Apikeyaccess
{
    public int Id { get; set; }

    public int ApikeyId { get; set; }

    public string Accessfrom { get; set; }

    public string State { get; set; }

    public string Description { get; set; }

    public int Order { get; set; }

    public virtual Apikey Apikey { get; set; }
}
