using IntegratedCalc.Settings;

using org.mariuszgromada.math.mxparser;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace IntegratedCalc.CommandLineIO
{
    public partial class TextInOutHandler
    {
        readonly RichTextBox _output;
        readonly TextBox _input;
        string _latestInput = String.Empty;
        readonly List<string> _historyInput = new List<string>();
        readonly List<Constant> _historyResult = new List<Constant>();
        int _historyPosition = -1;
        readonly Dictionary<string, IHelpCommand> _commands = new Dictionary<string, IHelpCommand>();
        SettingsProvider<SettingsObject> _provider = SettingsManager.Get<SettingsObject>();

        public TextInOutHandler(RichTextBox output, TextBox input)
        {
            _output = output;
            _input = input;
            InitializeNativeCommands();
            InitializeWebCommands();
            InitializeLaunchCommands();
            PrintHeader();
        }

        public event EventHandler<EventArgs> Close;
        public event EventHandler<EventArgs> Collpse;
        public event EventHandler<Size> Resize;
        public event EventHandler<bool> Topmost;
        public event EventHandler<string> NavigateBrowser;
        public event EventHandler<EventArgs> ReloadSettings;

        #region Event invokers
        private void DoResize(double w, double h) => Resize(this, new Size(w, h));
        private void DoClose() => Close(this, null);
        private void DoCollapse() => Collpse(this, null);

        private void DoTopmost(bool isTopmost) => Topmost(this, isTopmost);

        private void DoNavigateBrowser(string uri) => NavigateBrowser(this, uri);

        private void DoReloadSettings()
        {
            ReloadSettings(this, null);
            _commands.Clear();
            InitializeNativeCommands();
            InitializeWebCommands();
            InitializeLaunchCommands();
        }
        #endregion

        private bool InterceptCommands()
        {
            string input = _input.Text.Trim();
            string cmdlet = input.GetClArguments().First().ToLowerInvariant();
            _historyInput.Add(input);
            if (_commands.TryGetValue(cmdlet, out IHelpCommand command))
            {
                PrintInputLn(input);
                return command.Execute(input.Remove(0, command.Cmdlet.Length).TrimStart());
            }
            return false;
        }

        private void EvalInput()
        {
            var expr = new org.mariuszgromada.math.mxparser.Expression(_input.Text, _historyResult.ToArray());
            PrintInput(expr.getExpressionString());
            double res = expr.calculate();
            if (!Double.IsNaN(res))
            {
                string constName = "e" + _historyResult.Count();
                PrintLn();
                PrintActionLn(constName + " = " + res);
                _historyResult.Add(new Constant(constName, res));
            }
            else
            {
                Print(" = " + res.ToString());
            }
            ResetInput();
        }

        private void ClearAll()
        {
            _historyResult.Clear();
            _historyInput.Clear();
            _historyPosition = -1;
            _output.Document.Blocks.Clear();
            _output.Document.Blocks.Add(new Paragraph());
            _input.Clear();
            PrintHeader();
        }

        #region Print
        private void PrintHeader()
        {
            InlineCollection inlines = (_output.Document.Blocks.LastBlock as Paragraph).Inlines;
            inlines.Add(new Run { FontSize = 14, Text = "© 2020 " });
            inlines.Add(new Run { FontSize = 14, Text = "ProphetLamb", Foreground = new SolidColorBrush(Colors.Aquamarine) });
            inlines.Add(new Run { FontSize = 14, Text = " - https://github.com/ProphetLamb \nLicenced under the MIT licence.\n\n" });
            inlines.Add(new Run
            {
                Text = "IntegratedCalculator is a calculator app that aims to be more accessible then the Windows Calculator or Cortana queries, while using a command line like input.\n\n" +
                "To solve expressions this applications uses the mXpaser libary (https://github.com/mariuszgromada/MathParser.org-mXparser) Copyright 2010 - 2020 Mariusz Gromada licenced under Simplified BSD Licence.\n\n" +
                "Features cmdlets to query webpages like\n* Ecosia - web\n* WolframAlpha - alpha\n* Giphy - gif\nthat can be created and edited by the user.\n\n"
            });
            _output.ScrollToEnd();
        }

        private void PrintInput(string text) => Print(">" + text, _provider.Current.InputColor);
        private void PrintInputLn(string text) => PrintInput(text + Environment.NewLine);
        private void PrintError(string text) => Print("E: " + text, _provider.Current.ErrorColor);
        private void PrintErrorLn(string text) => PrintError(text + Environment.NewLine);
        private void PrintNotify(string text) => Print(text, _provider.Current.NotifyColor);
        private void PrintNotifyLn(string text) => PrintNotify(text + Environment.NewLine);
        private void PrintAction(string text) => Print(text, _provider.Current.ActionColor);
        private void PrintActionLn(string text) => PrintAction(text + Environment.NewLine);
        private void PrintLn() => Print(Environment.NewLine);
        private void PrintLn(string text) => Print(text + Environment.NewLine);
        private void PrintLn(string text, Color foreground) => Print(text + Environment.NewLine, foreground);
        private void Print(string text) => (_output.Document.Blocks.LastBlock as Paragraph).Inlines.Add(text);
        private void Print(string text, Color foreground) => (_output.Document.Blocks.LastBlock as Paragraph).Inlines.Add(new Run { Foreground = new SolidColorBrush(foreground), Text = text });
        #endregion

        #region TextInput functionality
        public void InputKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    if (!String.IsNullOrWhiteSpace(_input.Text) && !InterceptCommands())
                        EvalInput();
                    else
                        ResetInput();
                    break;
                case Key.Up:
                    HistoryUp();
                    break;
                case Key.Down:
                    HistoryDown();
                    break;
            }
        }

        private void HistoryUp()
        {
            if (_historyPosition == -1)
            {
                // Backup current input
                _latestInput = _input.Text;
                // Set to latest
                _historyPosition = _historyInput.Count - 1;
            }
            else if (_historyPosition != 0)
            {
                // Move to next earlier
                _historyPosition--;
            }
            _input.Text = _historyPosition == -1 ? String.Empty : _historyInput[_historyPosition];
            _input.CaretIndex = _input.Text.Length;
        }

        private void HistoryDown()
        {
            if (_historyPosition != -1)
                _historyPosition++;
            if (IsInvalidHistoryPosition())
            {
                // Restore current input
                _input.Text = _latestInput;
                // Set to latest input
                _historyPosition = -1;
                _input.Text = _latestInput;
            }
            else
            {
                _input.Text = _historyInput[_historyPosition];
            }
            _input.CaretIndex = _input.Text.Length;
        }

        private bool IsInvalidHistoryPosition() => 0 > _historyPosition || _historyPosition >= _historyInput.Count;

        private void ResetInput()
        {
            _historyPosition = -1;
            _input.Clear();
            PrintLn();
            _output.ScrollToEnd();
        }
        #endregion

        #region Command list initialization
        private void InitializeNativeCommands()
        {
            _commands.Clear();
            AddCommand(new HelpCommand(this, "help",
                "Displays a general or specified help text for commands.\n\nType a command + help or help + command to obtain additional help information:\"size help\", \"help settings\".\nType \"help math\" for the help text of the math engine.\n\n" +
                "Hotkeys:\n* [Win]+[C] - Opens this window\n* [Esc] - Hides this window\n* [Cntr]-[W] - Closes the webbrowser, if open.",
                "As you just found out you can also type help help... amazing. Try it a few more times.",
                arg =>
            {
                if (arg.StartsWith("math"))
                {
                    arg = arg.Remove(0, 4).TrimStart();
                    if (arg.Length != 0)
                        PrintLn(mXparser.getHelp(arg));
                    else
                        PrintLn(mXparser.getHelp());
                }
                else if (arg.Length != 0)
                {
                    if (_commands.TryGetValue(arg, out var comm))
                    {
                        comm.PrintHelp();
                        return true;
                    }
                    PrintErrorLn("Command not found.");
                    return false;
                }
                else
                {
                    var web = _commands.Where(x => x.Value is WebCommand);
                    var launch = _commands.Where(x => x.Value is LaunchCommand);
                    var native = _commands.Where(x => !(web.Contains(x) || launch.Contains(x)));
                    PrintLn("\nNative commands:");
                    foreach (var kvp in native)
                    {
                        PrintNotify(kvp.Key + ": ");
                        PrintLn(kvp.Value.Helptext + '\n');
                    }
                    PrintLn("\nWeb commands:");
                    foreach (var kvp in web)
                    {
                        PrintNotify(kvp.Key + ": ");
                        PrintLn(kvp.Value.Helptext + '\n');
                    }
                    PrintLn("\nLaunch commands:");
                    foreach (var kvp in launch)
                    {
                        PrintNotify(kvp.Key + ": ");
                        PrintLn(kvp.Value.Helptext + "\nPath: " + (kvp.Value as LaunchCommand).Target + '\n');
                    }
                }
                return true;
            }));
            AddCommand(new NestedHelpCommand(this, "settings",
                "Allows limited modification of applications settings. It is adviced to edit settings.json directly instead.",
                "")
            {
                new HelpCommand(this, "open",
                "Opens the settings.json file with the associated default application.",
                "",
                _ => {
                    try
                    {
                        Process.Start("settings.json");
                        PrintActionLn("Open settings.json.");
                        return true;
                    }
                    catch (FileNotFoundException)
                    {
                        PrintErrorLn("Failed to open settings.json, it doesnt exist... yet.");
                        return false;
                    }
                }),
                new HelpCommand(this, "load",
                "Loads settings from the settings.json file on the device.",
                "",
                _ => {
                    _provider.GetAsync().ContinueWith((a) => DoReloadSettings()).ConfigureAwait(false);
                    PrintActionLn("Load settings.json");
                    return true;
                }),
                new HelpCommand(this, "save",
                "Writes the current setting to the settings.json file on the device.",
                "",
                _ => {
                    InitWindow.SaveSettingsAsync(_provider).ConfigureAwait(false);
                    PrintActionLn("Save settings.json");
                    return true;
                }),
                new HelpCommand(this, "topmost",
                "Writes the current setting to the settings.json file on the device.",
                "",
                arg => {
                    var words = arg.Split(' ');
                    if (words.Length < 2)
                    {
                        PrintErrorLn("No value for topmost provided. Valid are \"on\", and \"off\".");
                        return false;
                    }
                    if (words[1].Equals("on", StringComparison.InvariantCultureIgnoreCase))
                    {
                        DoTopmost(true);
                        PrintActionLn("Topmost enabled.");
                    }
                    else if (words[1].Equals("off", StringComparison.InvariantCultureIgnoreCase))
                    {
                        DoTopmost(false);
                        PrintActionLn("Topmost disabled.");
                    }
                    else
                    {
                        PrintErrorLn("Invalid value for topmost. Valid are \"on\", and \"off\".");
                        return false;
                    }
                    return true;
                }),

            });
            AddCommand(new HelpCommand(this, "size",
                "Sets the size of the window to a predefined or specified value.",
                "By default predefined sizes are \"size small\", \"size mid\", and \"size big\". These can be changed and added to in the settings.json.\nAlternatively a width and height value can be provided: \"size 500, 700\".",
                arg =>
                {
                    if (String.IsNullOrWhiteSpace(arg))
                    {
                        PrintErrorLn("No size provided.");
                        return false;
                    }
                    if (_provider.Current.Sizes.TryGetValue(arg, out var size)) // Named size
                    {
                        DoResize(size.Width, size.Height);
                        PrintActionLn(String.Format("Set size to (w,h)=({0},{1})", size.Width, size.Height));
                        return true;
                    }
                    else // Specified size
                    {
                        var sizeValues = arg.Split(new[] { " ", ",", ";" }, 2, StringSplitOptions.RemoveEmptyEntries);
                        if (sizeValues.Length == 2 && Double.TryParse(sizeValues[0].Trim(), out double w) && Double.TryParse(sizeValues[1].Trim(), out double h))
                        {
                            DoResize(w, h);
                            PrintActionLn(String.Format("Set size to (w,h)=({0},{1})", w, h));
                            return true;
                        }
                    }
                    PrintErrorLn("Invalid value for size.");
                    return false;
                }));
            AddCommand(new HelpCommand(this, "clear",
                "Clears the screen.",
                "",
                _ => { ClearAll(); return true; }));
            AddCommand(new HelpCommand(this, "exit",
                "Exits the application entirely.",
                "",
                _ => { DoClose(); return true; }));
            AddCommand(new HelpCommand(this, "close",
                "Minimizes the application to the background.",
                "",
                _ => { DoCollapse(); return true; }));
            AddCommand(new HelpCommand(this, "cpy",
                "Copies the last result or result of the expression to the clipboard.",
                "\"cpy e0\" copies the value of c0 to the clipboard.\n\"cpy\" copies the result of the last calulations to the clipboard.\n\"cpy 2+2\" copies the value \"4\" to the clipboard.",
                arg =>
                {
                    if (String.IsNullOrWhiteSpace(arg))
                    {
                        if (_historyInput.Count != 0)
                        {
                            Constant latest = _historyResult[_historyResult.Count - 1];
                            Clipboard.SetText(latest.getConstantValue().ToString());
                            PrintActionLn("Copied " + latest.getConstantValue());
                            return true;
                        }
                        return false;
                    }
                    else
                    {
                        var expr = new org.mariuszgromada.math.mxparser.Expression(arg, _historyResult.ToArray());
                        double res = expr.calculate();
                        PrintActionLn("Copied " + res);
                        return !Double.IsNaN(res);
                    }
                }));
            AddCommand(new NestedHelpCommand(this, "timer",
                "[Work In Progress] Counts down from a timespan to zero.",
                "", null)
            {
                new HelpCommand(this, "start",
                "Starts the timer to a specified time.",
                "",
                arg =>
                {
                    if (String.IsNullOrWhiteSpace(arg) || !TimeSpan.TryParse(arg, out var ts))
                        return false;
                    // TODO: Show notification with remaining time.
                    return true;
                }),
                new HelpCommand(this, "stop",
                "Stops the latest timer, or the timer specified, if it exisits.",
                "",
                arg =>
                {
                    // TODO: Stops the latest timer, or the timer specified, if it exisits
                    return true;
                })
            });
        }

        private void InitializeWebCommands()
        {
            foreach (WebCommandData data in _provider.Current.WebCommands)
            {
                AddCommand(new WebCommand(this, data));
            }
        }

        private void InitializeLaunchCommands()
        {
            foreach (LaunchCommandData data in _provider.Current.LaunchCommands)
            {
                AddCommand(new LaunchCommand(this, data));
            }
        }

        private bool AddCommand(IHelpCommand command)
        {
            string cmdlet = command.Cmdlet.Trim().ToLowerInvariant();
            if (_commands.ContainsKey(cmdlet))
                return false;
            _commands.Add(cmdlet, command);
            return true;
        }
        #endregion

        internal interface IHelpCommand : ICliCommand
        {
            void PrintHelp();
        }

        internal class HelpCommand : SimpleCommand, IHelpCommand
        {
            protected readonly TextInOutHandler _parent;

            public HelpCommand(TextInOutHandler parent, string cmdlet, string helptext, string documentation, Func<string, bool> eval = null) : base(cmdlet, helptext, documentation, eval)
            {
                _parent = parent;
            }

            public override bool Execute(string arg)
            {
                if (arg.Equals("help", StringComparison.InvariantCultureIgnoreCase))
                {
                    PrintHelp();
                    return true;
                }
                return base.Execute(arg);
            }

            public virtual void PrintHelp()
            {
                _parent.PrintNotify(Cmdlet + ": ");
                _parent.PrintLn(Helptext + '\n' + Documentation);
            }
        }

        internal class NestedHelpCommand : HelpCommand, ICollection<ICliCommand>
        {
            private Dictionary<string, ICliCommand> _subCommands = new Dictionary<string, ICliCommand>();

            public NestedHelpCommand(TextInOutHandler parent, string cmdlet, string helptext, string documentation, Func<string, bool> eval = null) : base(parent, cmdlet, helptext, documentation, eval)
            {
            }

            public int Count => _subCommands.Count;
            public bool IsReadOnly { get; } = false;

            public bool Add(ICliCommand item)
            {
                if (_subCommands.ContainsKey(item.Cmdlet))
                    return false;
                _subCommands.Add(item.Cmdlet, item);
                return true;
            }
            void ICollection<ICliCommand>.Add(ICliCommand item) => Add(item);
            public void Clear() => _subCommands.Clear();
            public bool Contains(ICliCommand item) => _subCommands.ContainsKey(item.Cmdlet);
            public void CopyTo(ICliCommand[] array, int arrayIndex) => _subCommands.Values.CopyTo(array, arrayIndex);

            public override bool Execute(string arg)
            {
                if (base.Execute(arg))
                    return true;
                string arg1, arg2;
                // Handle empty arg
                if (String.IsNullOrEmpty(arg))
                {
                    arg1 = String.Empty;
                    arg2 = String.Empty;
                }
                else
                {
                    var args = arg.GetClArguments(ClArgOptions.RemoveWhitespaceEntries | ClArgOptions.RemoveEncompassingQuotes);
                    arg1 = args.FirstOrDefault().ToLowerInvariant() ?? String.Empty;
                    arg2 = arg.Remove(0, arg1.Length).Trim();
                }
                
                // Check if subcommand with cmdlet exists
                if (!_subCommands.TryGetValue(arg1, out ICliCommand subCommand))
                {
                    _parent.PrintErrorLn("Invalid subcommand \"" + arg1 + "\".");
                    return true;
                }
                // Try to evaluate the found subcommand with the remaining arg
                return subCommand.Execute(arg2);
            }

            public IEnumerator<ICliCommand> GetEnumerator() => _subCommands.Values.GetEnumerator();
            public bool Remove(ICliCommand item) => _subCommands.Remove(item.Cmdlet);
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public override void PrintHelp()
            {
                base.PrintHelp();
                _parent.PrintNotify("Sub command list: ");
                _parent.PrintLn(String.Join(", ", _subCommands.Keys.ToArray()));
            }
        }

        internal class WebCommand : HelpCommand
        {
            public WebCommand(TextInOutHandler parent, WebCommandData data) : base(parent, data.Cmdlet, data.Helptext, data.Documentation, null)
            {
                UriFormat = data.UriFormat;
            }

            public string UriFormat { get; }

            public override bool Execute(string arg)
            {
                if (!base.Execute(arg))
                {
                    string uri = String.Format(UriFormat, HttpUtility.UrlEncode(arg));
                    _parent.DoNavigateBrowser(uri);
                    _parent.PrintActionLn("Open browser " + uri);
                }
                return true;
            }
        }

        internal class LaunchCommand : IHelpCommand
        {
            protected readonly TextInOutHandler _parent;

            public LaunchCommand(TextInOutHandler parent, LaunchCommandData data)
            {
                _parent = parent;
                Cmdlet = data.Cmdlet;
                Target = data.Target;
                Helptext = data.Helptext;
                ExecutableName = Path.GetFileName(data.Target);
            }

            public string Cmdlet { get; }
            public string Helptext { get; }
            public string Documentation => Target;
            public string Target { get; }
            public string ExecutableName { get; }

            public virtual bool Execute(string arg)
            {
                try
                {
                    Process.Start(Target, arg);
                    _parent.PrintActionLn("Launching " + ExecutableName);
                    return true;
                }
                catch (FileNotFoundException)
                {
                    _parent.PrintErrorLn("The file \"" + Target + "\" could not be found.");
                    return false;
                }
            }

            public virtual void PrintHelp()
            {
                _parent.PrintNotify(Cmdlet + ": ");
                _parent.PrintLn(Helptext + "\nFile: " + Target);
            }
        }
    }

    public class WebCommandData
    {
        public WebCommandData(string cmdlet, string helptext, string documentation, string uriFormat)
        {
            Cmdlet = cmdlet;
            Helptext = helptext;
            Documentation = documentation;
            UriFormat = uriFormat;
        }

        public string Cmdlet { get; }
        public string Helptext { get; }
        public string Documentation { get; }
        public string UriFormat { get; }
    }

    public class LaunchCommandData
    {
        public LaunchCommandData(string cmdlet, string helptext, string target)
        {
            Cmdlet = cmdlet;
            Helptext = helptext;
            Target = target;
        }

        public string Cmdlet { get; }
        public string Helptext { get; }
        public string Target { get; }
    }
}