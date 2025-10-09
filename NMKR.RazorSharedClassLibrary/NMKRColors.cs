namespace NMKR.RazorSharedClassLibrary
{
    public class NMKRColor
    {
        public string BackgroundColor { get; set; }
        public string ForegroundColor { get; set; }
    }

    public static class NMKRColors
    {
        public static NMKRColor PrimaryColor = new NMKRColor {BackgroundColor = "#11F250", ForegroundColor = "#FFFFFF"};
        public static NMKRColor Secondary = new NMKRColor { BackgroundColor = "#021205", ForegroundColor = "#FFFFFF" };
        public static NMKRColor Tertiary = new NMKRColor { BackgroundColor = "#0EBF3F", ForegroundColor = "#FFFFFF" };
        public static NMKRColor GrayDefault = new NMKRColor { BackgroundColor = "#A3A5A3", ForegroundColor = "#FFFFFF" };
        public static NMKRColor Info = new NMKRColor { BackgroundColor = "#302BFB", ForegroundColor = "#FFFFFF" };
        public static NMKRColor Success = new NMKRColor { BackgroundColor = "#0CA638", ForegroundColor = "#FFFFFF" };
        public static NMKRColor Warning = new NMKRColor { BackgroundColor = "#EB9D07", ForegroundColor = "#000000" };
        public static NMKRColor Error = new NMKRColor { BackgroundColor = "#FB2B50", ForegroundColor = "#FFFFFF" };
        public static NMKRColor Dark = new NMKRColor { BackgroundColor = "#021205", ForegroundColor = "#FFFFFF" };
    }
}
