using IntegratedCalc.Commandline;

using System.Collections.Generic;

namespace IntegratedCalc
{
    public static class StringExtentions
    {
        public static IEnumerable<string> CommandlineArguments(this string self, CommandLineArgumentParserOptions options = CommandLineArgumentParserOptions.RemoveWhitespaceEntries)
        {
            using var en = new CommandLineArgumentParser(self, options);
            while (en.MoveNext())
                yield return en.Current;
        }
    }
}
