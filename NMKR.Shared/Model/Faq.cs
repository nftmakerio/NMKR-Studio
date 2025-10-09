namespace NMKR.Shared.Model;

public partial class Faq
{
    public int Id { get; set; }

    public string Question { get; set; }

    public string Answer { get; set; }

    public string Language { get; set; }

    public string State { get; set; }

    public int FaqcategoryId { get; set; }

    public virtual Faqcategory Faqcategory { get; set; }
}
