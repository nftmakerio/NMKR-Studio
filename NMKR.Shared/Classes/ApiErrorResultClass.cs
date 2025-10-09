namespace NMKR.Shared.Classes
{
    public enum ResultStates { Ok,Error};
    public class ApiErrorResultClass
    {
        public ResultStates ResultState { get; set; }
        public string ErrorMessage { get; set; }
        public int ErrorCode { get; set; }
        public string InnerErrorMessage { get; set; }
    }

    public class RejectedErrorResultClass
    {
        public ResultStates ResultState { get; set; }
        public string ErrorMessage { get; set; }
        public int ErrorCode { get; set; }
        public string RejectReason { get; set; }
        public string RejectParameter { get; set; }
    }


    public class ApiResultClass
    {
        public ApiErrorResultClass ApiError { get; set; }
        public int ActionResultStatuscode { get; set; }
        public object SuccessResultObject { get; set; }
    }
}
