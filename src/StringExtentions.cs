using IntegratedCalc.CommandLineIO;

using System.Collections.Generic;

namespace IntegratedCalc
{
    public static class StringExtentions
    {
        public static IEnumerable<string> GetClArguments(this string self, ClArgOptions options = ClArgOptions.RemoveWhitespaceEntries)
        {
            using var en = new ClArgParser(self, options);
            while (en.MoveNext())
                yield return en.Current;
        }
    }
}
