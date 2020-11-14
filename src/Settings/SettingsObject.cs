using IntegratedCalc.Commandline;

using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace IntegratedCalc.Settings
{
    public class SettingsObject
    {
        public Dictionary<string, Size> Sizes { get; set; } 
        public List<WebCommandData> WebCommands { get; set; } 
        public List<LaunchCommandData> LaunchCommands { get; set; }
        public Size StartupSize { get; set; }
        public Size WebbrowserSize { get; set; }
        public bool IsTopmost { get; set; }
        public WindowHelper.SnapOrigins StartupLocation { get; set; }
        public Color ErrorColor { get; set; }
        public Color InputColor { get; set; }
        public Color NotifyColor { get; set; }
        public Color ActionColor { get; set; }
        public Color BackgroundColor { get; set; }
        public Color ForegroundColor { get; set; }
        public Color LightBackgroundColor { get; set; }
        public Color DisabledForegroundColor { get; set; }

        public static SettingsObject Default => new SettingsObject
        {
            StartupSize = new Size(500, 300),
            WebbrowserSize = new Size(700, 800),
            IsTopmost = true,
            StartupLocation = WindowHelper.SnapOrigins.Bottom | WindowHelper.SnapOrigins.Left,
            Sizes = new Dictionary<string, Size>
            {
                { "small", new Size(400,300) },
                { "mid", new Size(500,700) },
                { "big", new Size(600,1000) }
            },
            WebCommands = new List<WebCommandData>
            {
                new WebCommandData(
                    "alpha",
                    "Queries the searchterm on WolframAlpha, in the integrated webbrowser.",
                    "The uri of the site is wolframalpha.com",
                    "https://www.wolframalpha.com/input/?i={0}"),
                new WebCommandData(
                    "web",
                    "Queries the searchterm on Ecosia, in the integrated webbrowser.",
                    "The uri of the site is ecosia.org",
                    "https://www.ecosia.org/search?q={0}"),
                new WebCommandData(
                    "uni",
                    "Queries the searchterm on Amp-What, in the integrated browser.",
                    "The uri of the site is amp-what.com",
                    "http://www.amp-what.com/unicode/search/{0}"),
                new WebCommandData(
                    "gif",
                    "Queries the searchterm on Giphy, in the integated browser.",
                    "The uri of the site is giphy.com",
                    "https://giphy.com/search/{0}"),
                new WebCommandData(
                    "trans",
                    "Queries the searchterm on Google Translate, in the integated browser.",
                    "The uri of the site is translate.google.com",
                    "https://translate.google.com/?text={0}&op=translate"),
            },
            LaunchCommands = new List<LaunchCommandData>
            {
                new LaunchCommandData(
                    "npp",
                    "Launches Notepad++.",
                    @"C:\Program Files (x86)\Notepad++\notepad++.exe"),
                new LaunchCommandData(
                    "code",
                    "Launches Visual Studio Code.",
                    @"C:\Program Files\Microsoft VS Code\Code.exe"),
                new LaunchCommandData(
                    "cmd",
                    "Launches the Windows Commandpromt.",
                    @"cmd.exe")
            },
            ErrorColor = Colors.Red,
            InputColor = Colors.LightBlue,
            NotifyColor = Colors.Orange,
            ActionColor = Colors.Green,
            BackgroundColor = Colors.Black,
            ForegroundColor = Colors.White,
            LightBackgroundColor = Colors.DarkSlateGray,
            DisabledForegroundColor = Colors.LightGray
        };
    }
}
