using System.Collections.Generic;

namespace NMKR.Shared.Model;

public class SearchResult
{

    public string Title { get; set; }

    public string Description { get; set; }

    public string Link { get; set; }


    public class Section
    {
        public string sectionTitle;
        public string sectionText;

        public Section(string Title, string Text)
        {
            sectionTitle = Title;
            sectionText = Text;
        }
    }

    public List<Section> sectionList { get; set; }

}
