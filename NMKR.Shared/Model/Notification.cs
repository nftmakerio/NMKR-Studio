namespace NMKR.Shared.Model;

public partial class Notification
{
    public int Id { get; set; }

    public int NftprojectId { get; set; }

    public string Notificationtype { get; set; }

    public string Address { get; set; }

    public string State { get; set; }

    public string Secret { get; set; }

    public virtual Nftproject Nftproject { get; set; }
}
