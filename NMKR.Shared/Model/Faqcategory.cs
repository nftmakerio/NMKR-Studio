using System.Collections.Generic;

namespace NMKR.Shared.Model;

public partial class Faqcategory
{
    public int Id { get; set; }

    public string Categoryname { get; set; }

    public string Language { get; set; }

    public virtual ICollection<Faq> Faqs { get; set; } = new List<Faq>();
}
