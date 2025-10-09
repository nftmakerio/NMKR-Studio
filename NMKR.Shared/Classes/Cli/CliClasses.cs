namespace NMKR.Shared.Classes.Cli
{
    public class CommandFiles
    {
        public string FileName { get; set; }
        public string Content { get; set; }
    }
    public class CliCommand
    {
        public string Command { get; set; }
        public CommandFiles[] InFiles { get; set; }
        public CommandFiles[] OutFiles { get; set; }
    }

    public class RemoteCallCardanoCliResultClass
    {
        public string Result { get; set; }
        public string ErrorMessage { get; set; }
        public CommandFiles[] OutFiles { get; set; }
        public string Log { get; set; }
    }
}
