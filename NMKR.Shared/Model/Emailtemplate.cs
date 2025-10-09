namespace NMKR.Shared.Model;

public partial class Emailtemplate
{
    public int Id { get; set; }

    public string Templatename { get; set; }

    public string Language { get; set; }

    public string Textemail { get; set; }

    public string Htmlemail { get; set; }

    public string Emailsubject { get; set; }
}
