using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TeamTools.TSQL.Linter.Routines
{
    public static class FormatMessageWildcardExtractor
    {
        private static readonly Regex WildcardPattern = new Regex(
            "((?<before>.*?)" +
                "(?<wildcard>(?<!%)%" +
                "(?<format>" +
                    "((?<prefix>[+#-])?)" +
                    "((?<size>[\\d]+)?))" +
                "(?<type>[sxoidu]))" +
            ")|(?<after>.+$)",
            RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public static MatchCollection ExtractElements(string src) => WildcardPattern.Matches(src);

        public static IEnumerable<string> ExtractWildcards(string src)
        {
            return WildcardPattern
              .Matches(src)
              .Select(m => m.Groups["wildcard"].Value)
              .Where(w => !string.IsNullOrEmpty(w));
        }
    }
}
