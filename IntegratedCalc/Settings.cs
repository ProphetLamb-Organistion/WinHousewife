using IntegratedCalc.CommandLineIO;

using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace IntegratedCalc
{
    public class SettingsObject
    {
        public Dictionary<string, Size> Sizes { get; set; } = new Dictionary<string, Size>
        {
            { "small", new Size(400,300) },
            { "mid", new Size(500,700) },
            { "big", new Size(600,1000) }
        };
        public List<WebCommandData> WebCommands { get; set; } = new List<WebCommandData>
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
        };
        public List<LaunchCommandData> LaunchCommands { get; set; } = new List<LaunchCommandData>
        {
            new LaunchCommandData(
                "npp",
                "Launches Notepad++.",
                @"C:\Program Files (x86)\Notepad++\notepad++.exe"),
            new LaunchCommandData(
                "code",
                "Launches Visual Studio Code.",
                @"C:\Program Files\Microsoft VS Code\Code.exe")
        };
        public Size StartupSize = new Size(500, 300);
        public Size WebbrowserSize = new Size(700, 800);
        public bool IsTopmost = true;
        public WindowHelper.SnapOrigins StartupLocation = WindowHelper.SnapOrigins.Bottom | WindowHelper.SnapOrigins.Left;
        public Color ErrorColor { get; set; } = Colors.Red;
        public Color InputColor { get; set; } = Colors.LightBlue;
        public Color NotifyColor { get; set; } = Colors.Orange;
        public Color ActionColor { get; set; } = Colors.Green;
        public Color BackgroundColor { get; set; } = Colors.Black;
        public Color ForegroundColor { get; set; } = Colors.White;
        public Color LightBackgroundColor { get; set; } = Colors.DarkSlateGray;
        public Color DisabledForegroundColor { get; set; } = Colors.LightGray;
    }
}
