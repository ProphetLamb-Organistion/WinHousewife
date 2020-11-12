using System;

namespace IntegratedCalc.CommandLineIO
{
    public interface ICliCommand
    {
        public string Cmdlet { get; }
        public string Helptext { get; }
        public string Documentation { get; }
        public bool Execute(string arg);
    }

    public class SimpleCommand : ICliCommand
    {
        private readonly Func<string, bool> _func;

        public SimpleCommand(string cmdlet, string helptext, string documentation, Func<string, bool> eval)
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
    }
}
