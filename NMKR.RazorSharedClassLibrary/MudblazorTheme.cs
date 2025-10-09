using MudBlazor;

namespace NMKR.RazorSharedClassLibrary
{
    public class MudblazorTheme
    {
        public MudTheme Theme = new();

        public MudblazorTheme()
        {
            Theme.PaletteLight = new()
            {
               // AppbarBackground = "#F0F0F0",
                AppbarBackground = "#000000",
                Black = "#021205",
             //Black= "#6c6c6c",
                White = "#FFFFFF",

                Tertiary = "#11F250",
                TertiaryContrastText = "#021205",
                TertiaryLighten = "#DAFFE8",
                TertiaryDarken = "#0FD948",

                Primary = "#11F250",
                PrimaryContrastText = "#021205",
                PrimaryDarken = "#11F250",
                PrimaryLighten = "#0FD948",
               // PrimaryLighten = "#6c6c6c",


                Secondary = "#021205",
                SecondaryContrastText = "#FFFFFF",
                SecondaryDarken = "#000000",
                SecondaryLighten = "#0A852D",
              
                GrayDefault = "#A3A5A3",
                GrayLight = "#E0E0E0",
                GrayLighter = "#F4F5F5",
                GrayDark = "#7A7A7A",
                GrayDarker = "#424242",
                Info = "#302BFB",
                InfoContrastText = "#EAF1FF",
                InfoDarken = "#302BFB",
                InfoLighten = "#EAF1FF",
                Success = "#11F250",
                SuccessContrastText = "#021205",
                SuccessDarken = "#11F250",
                SuccessLighten = "#DAFFE8",
                Warning = "#EB9D07",
                WarningContrastText = "#FFFBE9",
                WarningDarken = "#EB9D07",
                WarningLighten = "#FFFBE9",
                Error = "#FB2B50",
                ErrorContrastText = "#FFEAEE",
                ErrorDarken = "#FB2B50",
                ErrorLighten = "#FFEAEE",
                TextPrimary = "#021205",
                TextSecondary = "#525252",
                TextDisabled = "#7A7A7A",
                LinesDefault = "#0000001e",
                LinesInputs = "#bdbdbdff",
                DrawerBackground = "#FFFFFF",
                DrawerText = "#021205",
                DrawerIcon = "#021205",
                AppbarText = "#FFFFFF",
                Divider = "#E0E0E0",
                DividerLight = "#F4F5F5",
                TableLines = "#E0E0E0",
                TableStriped = "#F4F5F5",
                TableHover = "#F4F5F5",
                Dark = "#021205",
              // Dark= "#6c6c6c",
                DarkContrastText = "#FFFFFF",
                DarkDarken = "#000000",
                DarkLighten = "0D2812",
                Background = "#F4F5F5",
              //  BackgroundGrey = "#F4F5F5",
                Surface = "#FFFFFF",
                ActionDefault = "#0EBF3F",
                ActionDisabled = "#525252",
                ActionDisabledBackground = "#E0E0E0",
                OverlayDark = "rgba(33,33,33,0.55)",
                OverlayLight = "rgba(255,255,255,0.5)",
            };


            Theme.Typography = new()
            {
                H1 = new H1Typography()
                {
                    FontFamily = new[] { "Runda" },
                    FontSize = "2.625rem",
                    FontWeight = "700",
                    LineHeight = "1.3",
                    LetterSpacing = "0em"
                },

                H2 = new H2Typography()
                {
                    FontFamily = new[] { "Runda" },
                    FontSize = "2.125rem",
                    FontWeight = "700",
                    LineHeight = "1.27",
                    LetterSpacing = "0em"
                },

                H3 = new H3Typography()
                {
                    FontFamily = new[] { "Runda" },
                    FontSize = "1.75rem",
                    FontWeight = "700",
                    LineHeight = "1.29",
                    LetterSpacing = "0em"
                },

                H4 = new H4Typography()
                {
                    FontFamily = new[] { "Inter" },
                    FontSize = "1.375rem",
                    FontWeight = "700",
                    LineHeight = "1.39",
                    LetterSpacing = "0em"
                },

                H5 = new H5Typography()
                {
                    FontFamily = new[] { "Runda" },
                    FontSize = "1.125rem",
                    FontWeight = "700",
                    LineHeight = "1.45",
                    LetterSpacing = "0em"
                },

                H6 = new H6Typography()
                {
                    FontFamily = new[] { "Runda" },
                    FontSize = "0.875rem",
                    FontWeight = "700",
                    LineHeight = "1.53",
                    LetterSpacing = ".01em"
                },

                Body1 = new Body1Typography()
                {
                    FontFamily = new[] { "Runda" },
                    FontSize = "0.875rem",
                    FontWeight = "500",
                    LineHeight = "1.45",
                    LetterSpacing = "0em"
                },

                Body2 = new Body2Typography()
                {
                    FontFamily = new[] { "Runda" },
                    FontSize = "0.688rem",
                    FontWeight = "500",
                    LineHeight = "1.58",
                    LetterSpacing = "0em"
                },

                Default = new DefaultTypography()
                {
                    FontFamily = new[] { "Runda" },
                    FontSize = "1rem",
                    FontWeight = "500",
                    LineHeight = "1.45",
                    LetterSpacing = "0em"
                },

                Subtitle1 = new Subtitle1Typography()
                {
                    FontFamily = new[] { "Runda" },
                    FontSize = "1rem",
                    FontWeight = "500",
                    LineHeight = "1.45",
                    LetterSpacing = "0em"
                },

                Button = new ButtonTypography()
                {
                    FontFamily = new[] { "Runda" },
                    FontSize = "0.875rem",
                    FontWeight = "700",
                    LineHeight = "1.2",
                    LetterSpacing = ".08em"
                }
            };


        }


    }
}
