using System;
using NMKR.Shared.Enums;

namespace NMKR.Shared.Classes
{
    public class BackgroundServerTasksClass
    {
        public BackgroundTaskEnums Task { get; set; }
        public int? ActualProjectId { get; set; }
        public DateTime LastlifeSign { get; set; }
    }
}
