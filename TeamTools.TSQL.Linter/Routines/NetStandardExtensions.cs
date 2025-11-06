using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;

namespace TeamTools.TSQL.Linter.Routines
{
    [ExcludeFromCodeCoverage]
    public static class NetStandardExtensions
    {
        private const char LineBreakChar = '\r';

#if NetStandard
        public static bool TryAdd<TKey, TVal>(this IDictionary<TKey, TVal> dict, TKey key, TVal val)
        {
            if (dict.ContainsKey(key))
            {
                return false;
            }

            dict.Add(key, val);
            return true;
        }

        public static string[] Split(this string str, string delim)
        {
            return Regex.Split(str, Regex.Escape(delim), RegexOptions.CultureInvariant);
        }

        public static IEnumerable<string> Select(this MatchCollection matches, Func<Match, string> lambda)
        {
            int n = matches.Count;
            for (int i = 0; i < n; i++)
            {
                var res = lambda(matches[i]);
                if (res != null)
                {
                    yield return res;
                }
            }
        }

        public static IEnumerable<T> Select<T>(this MatchCollection matches, Func<Match, T> lambda)
        {
            int n = matches.Count;
            for (int i = 0; i < n; i++)
            {
                yield return lambda(matches[i]);
            }
        }
#endif

        public static IEnumerable<TElement> TakeLast<TElement>(this IEnumerable<TElement> arr, int count)
        {
            return arr.Reverse().Take(count).Reverse();
        }

        public static TVal GetValueOrDefault<TKey, TVal>(this IDictionary<TKey, TVal> dict, TKey key, TVal def)
        {
            if (dict.ContainsKey(key))
            {
                return dict[key];
            }

            return def;
        }

        public static string FirstCharToLower(this string str)
        {
            var chars = str.ToCharArray();
            chars[0] = char.ToLowerInvariant(chars[0]);
            return new string(chars);
        }

        public static string FirstCharToUpper(this string str)
        {
            var chars = str.ToCharArray();
            chars[0] = char.ToUpperInvariant(chars[0]);
            return new string(chars);
        }

        public static int LineCount(this string str)
        {
            // just \r is fine here - we are not doing split, just counting
            // no matter whether \n follows it or not
            return str.Count(s => s.Equals(LineBreakChar)) + 1;
        }

        public static int DelimiterCount(this string str, char delim)
        {
            int delimCount = 0;
            int length = str.Length;
            for (int i = length - 1; i >= 0; i--)
            {
                if (str[i] == delim)
                {
                    delimCount++;
                }
            }

            return delimCount;
        }

        public static bool In<T>(this T value, params T[] values)
        {
            return values.Contains(value);
        }

        public static string Left(this string value, int length)
        {
            return value.Substring(0, Math.Min(value.Length, length));
        }

        public static bool TryAddUnique<T>(this ICollection<T> target, T value)
        {
            if (target.Contains(value))
            {
                return false;
            }
            else
            {
                target.Add(value);
                return true;
            }
        }
    }
}
