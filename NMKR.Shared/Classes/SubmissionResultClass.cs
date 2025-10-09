namespace NMKR.Shared.Classes
{
    public class SubmissionResultClass
    {
        public bool Success { get; set; }
        public string TxHash { get; set; }
        public string ErrorMessage { get; set; }
        public BuildTransactionClass Buildtransaction { get; set; }

        public SubmissionResultClass()
        {
            Success = false;
            ErrorMessage = "Unknown Error";
        }
    }
}
