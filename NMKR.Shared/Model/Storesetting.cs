using System.Collections.Generic;

namespace NMKR.Shared.Model;

public partial class Storesetting
{
    public int Id { get; set; }

    public string Settingsname { get; set; }

    public string Humanreadablesettingsname { get; set; }

    public bool Mandantory { get; set; }

    public string Settingstype { get; set; }

    public string Description { get; set; }

    public int Page { get; set; }

    public string Category { get; set; }

    public string Subcategory { get; set; }

    public string Listvalues { get; set; }

    public int Sortorder { get; set; }

    public int? Maxwidth { get; set; }

    public int? Maxheight { get; set; }

    public int? Maxlength { get; set; }

    public string Allowedfiletypes { get; set; }

    public virtual ICollection<Whitelabelstorecollection> Whitelabelstorecollections { get; set; } = new List<Whitelabelstorecollection>();

    public virtual ICollection<Whitelabelstoresetting> Whitelabelstoresettings { get; set; } = new List<Whitelabelstoresetting>();
}
