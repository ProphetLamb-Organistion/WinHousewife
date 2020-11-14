using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;

namespace IntegratedCalc.Commandline
{
    public interface ICommand
    {
        public string Cmdlet { get; }
        public string Helptext { get; }
        public string Documentation { get; }
        public bool Execute(string arg);
        void PrintHelp();
    }

    public abstract class CommandBase : ICommand
    {
        private readonly Func<string, bool> _func;

        protected CommandBase(string cmdlet, string helptext, string documentation, Func<string, bool> eval)
        {
            Cmdlet = cmdlet.Trim().ToLowerInvariant();
            Helptext = helptext;
            Documentation = documentation;
            _func = eval;
        }

        public virtual string Cmdlet { get; }

        public virtual string Helptext { get; }

        public virtual string Documentation { get; }

        public virtual bool Execute(string arg)
        {
            if (_func != null)
                return _func(arg);
            return false;
        }

        public abstract void PrintHelp();
    }

    internal class SimpleCommand : CommandBase
    {
        protected readonly TextInOutHandler _parent;

        public SimpleCommand(TextInOutHandler parent, string cmdlet, string helptext, string documentation, Func<string, bool> eval = null) : base(cmdlet, helptext, documentation, eval)
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

        public override void PrintHelp()
        {
            _parent.PrintNotify(Cmdlet + ": ");
            _parent.PrintLn(Helptext + '\n' + Documentation);
        }
    }

    internal class NestedCommand : SimpleCommand, ICollection<ICommand>
    {
        private Dictionary<string, ICommand> _subCommands = new Dictionary<string, ICommand>();

        public NestedCommand(TextInOutHandler parent, string cmdlet, string helptext, string documentation, Func<string, bool> eval = null) : base(parent, cmdlet, helptext, documentation, eval)
        {
        }

        public int Count => _subCommands.Count;
        public bool IsReadOnly { get; } = false;

        public bool Add(ICommand item)
        {
            if (_subCommands.ContainsKey(item.Cmdlet))
                return false;
            _subCommands.Add(item.Cmdlet, item);
            return true;
        }
        void ICollection<ICommand>.Add(ICommand item) => Add(item);
        public void Clear() => _subCommands.Clear();
        public bool Contains(ICommand item) => _subCommands.ContainsKey(item.Cmdlet);
        public void CopyTo(ICommand[] array, int arrayIndex) => _subCommands.Values.CopyTo(array, arrayIndex);

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
                var args = arg.CommandlineArguments(CommandLineArgumentParserOptions.RemoveWhitespaceEntries | CommandLineArgumentParserOptions.RemoveEncompassingQuotes);
                arg1 = args.FirstOrDefault().ToLowerInvariant() ?? String.Empty;
                arg2 = arg.Remove(0, arg1.Length).Trim();
            }

            // Check if subcommand with cmdlet exists
            if (!_subCommands.TryGetValue(arg1, out ICommand subCommand))
            {
                _parent.PrintErrorLn("Invalid subcommand \"" + arg1 + "\".");
                return true;
            }
            // Try to evaluate the found subcommand with the remaining arg
            return subCommand.Execute(arg2);
        }

        public IEnumerator<ICommand> GetEnumerator() => _subCommands.Values.GetEnumerator();
        public bool Remove(ICommand item) => _subCommands.Remove(item.Cmdlet);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override void PrintHelp()
        {
            base.PrintHelp();
            _parent.PrintNotify("Sub command list: ");
            _parent.PrintLn(String.Join(", ", _subCommands.Keys.ToArray()));
        }
    }

    internal class WebCommand : SimpleCommand
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

    internal class LaunchCommand : ICommand
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

