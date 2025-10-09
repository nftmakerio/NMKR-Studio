namespace NMKR.Shared.Model;

public partial class PreparedpaymenttransactionsNotification
{
    public int Id { get; set; }

    public int PreparedpaymenttransactionsId { get; set; }

    public string Notificationtype { get; set; }

    public string Notificationendpoint { get; set; }

    public string Secret { get; set; }

    public virtual Preparedpaymenttransaction Preparedpaymenttransactions { get; set; }
}
