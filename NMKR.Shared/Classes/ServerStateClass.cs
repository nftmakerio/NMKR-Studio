using NMKR.Shared.Enums;
using System;

namespace NMKR.Shared.Classes
{
    public class ServerStateClass
    {
        public int ServerId { get; set; }
        public string ServerName { get; set; }
        public string ServerState { get; set; }
        public DateTime LastLifeSign { get; set; }
        public BackgroundTaskEnums ActualTask { get; set; }
        public string CardanoNodeVersion { get; set; }
        public string ServerVersion { get; set; }
        public string CardanoSlot { get; set; }
        public string CardanoEpoch { get; set; }
        public string CardanoSyncprogress { get; set; }
        public BackgroundTaskEnums[] BackgroundTasks { get; set; }
    }
}
