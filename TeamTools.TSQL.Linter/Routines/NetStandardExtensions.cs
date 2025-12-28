using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace TeamTools.TSQL.Linter.Routines
{
    [ExcludeFromCodeCoverage]
    public static class NetStandardExtensions
    {
#if NETSTANDARD
        private static readonly Regex NewLineRegex = new Regex(Environment.NewLine, RegexOptions.Compiled);

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
            if (string.Equals(delim, Environment.NewLine))
            {
                return NewLineRegex.Split(str);
            }

            return Regex.Split(str, Regex.Escape(delim), RegexOptions.CultureInvariant);
        }

        public static bool Contains(this string str, char c)
        {
            return str.IndexOf(c) >= 0;
        }

        public static TVal GetValueOrDefault<TKey, TVal>(this IDictionary<TKey, TVal> dict, TKey key, TVal def)
        {
            if (dict.TryGetValue(key, out TVal value))
            {
                return value;
            }

            return def;
        }
#endif

        public static int LineCount(this string str)
        {
            // just \n is fine here - we are not doing split, just counting
            // no matter whether \r preceides it or not
            const char LineBreakChar = '\n';
            // the string itself takes at least 1 line for sure
            // TODO : shouldn't it return 0 for empty string or null?
            int count = 1;
            int n = str.Length;
            for (int i = n - 1; i >= 0; i--)
            {
                if (str[i].Equals(LineBreakChar))
                {
                    count++;
                }
            }

            return count;
        }

        public static bool IsUpperCase(this string value)
        {
            foreach (var c in value)
            {
                if (char.IsLetter(c) && !char.IsUpper(c))
                {
                    return false;
                }
            }

            return true;
        }

        public static string Left(this string value, int length)
        {
            return value.Substring(0, Math.Min(value.Length, length));
        }

        [Obsolete("Use native bool Add/TryAdd functions of SortedSet, HashSet, etc")]
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
