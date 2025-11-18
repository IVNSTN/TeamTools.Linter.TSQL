using System.Text.RegularExpressions;

namespace TeamTools.TSQL.ExpressionEvaluator.Routines
{
    public static class FormatMessageWildcardExtractor
    {
        private static readonly Regex WildcardPattern = new Regex(
            "((?<before>.*?)" +
                "(?<wildcard>(?<!%)%" +
                "(?<format>" +
                    "((?<prefix>[+#-])?)" +
                    "((?<size>[\\d]+)?))" +
                "(?<type>[sxoiduSXOIDU]))" +
            ")|(?<after>.+$)",
            RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);

        public static MatchCollection ExtractElements(string src) => WildcardPattern.Matches(src);

        public static int CountWildcards(string src)
        {
            int count = 0;

            var matches = ExtractElements(src);
            int n = matches.Count;
            for (int i = 0; i < n; i++)
            {
                if (!string.IsNullOrEmpty(matches[i].Groups["wildcard"].Value))
                {
                    count++;
                }
            }

            return count;
        }
    }
}
