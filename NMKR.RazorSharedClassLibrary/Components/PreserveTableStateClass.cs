using MudBlazor;

namespace NMKR.RazorSharedClassLibrary.Components
{
    public class PreserveTableStateClass
    {
        public PreserveTableStateClass()
        {
            WasInitialized = false;
        }
        public int? ReferenceTableId { get; set; }
        public int PageNumber{ get; set; }
        public int EntriesPerPage{ get; set; }
        public string Search{ get; set; }
        public string Parameter1 { get; set; }
        public string SortOrder { get; set; }
        public SortDirection Direction { get; set; }
        public bool WasInitialized { get; set; }

        public void SetState(TableState state, string searchString, string parameter1, int? referenceId=null)
        {
            PageNumber = state.Page;
            EntriesPerPage = state.PageSize;
            Search = searchString;
            Parameter1 = parameter1;
            SortOrder = state.SortLabel;
            Direction = state.SortDirection;
            ReferenceTableId = referenceId;
            WasInitialized = true;
        }
    }
}
