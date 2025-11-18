using Humanizer;
using System;
using System.Collections.Generic;
using TeamTools.TSQL.Linter.Interfaces;

namespace TeamTools.TSQL.Linter.Routines
{
    internal class IdentifierNotationResolver : INotationResolver
    {
        private static readonly List<char> NamePrefixes = new List<char>
        {
            TSqlDomainAttributes.VariablePrefixChar,
            TSqlDomainAttributes.TempTablePrefixChar,
        };

        public static string ConvertTo(string original, NamingNotationKind targetNotation)
        {
            switch (targetNotation)
            {
                case NamingNotationKind.CamelCase:
                    return original.Camelize();

                case NamingNotationKind.KebabCase:
                    return original.Kebaberize();

                case NamingNotationKind.PascalCase:
                    return original.Pascalize();

                case NamingNotationKind.SnakeLowerCase:
                    return original.Underscore().ToLowerInvariant();

                case NamingNotationKind.SnakeUpperCase:
                    return original.Underscore().ToUpperInvariant();

                default:
                    return original;
            }
        }

        public NamingNotationKind Resolve(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                return NamingNotationKind.Unknown;
            }

            identifier = CleanName(identifier);
            if (string.IsNullOrEmpty(identifier))
            {
                return NamingNotationKind.Unknown;
            }

            var parsed = identifier.Humanize();

            var pascalCaseVersion = parsed.Pascalize();
            if (identifier.Equals(pascalCaseVersion, StringComparison.Ordinal))
            {
                return NamingNotationKind.PascalCase;
            }

            var snakeCaseVersion = parsed.Underscore();
            if (identifier.Equals(snakeCaseVersion, StringComparison.Ordinal))
            {
                return NamingNotationKind.SnakeLowerCase;
            }

            if (identifier.Equals(snakeCaseVersion.ToUpperInvariant(), StringComparison.Ordinal))
            {
                return NamingNotationKind.SnakeUpperCase;
            }

            var camelCaseVersion = parsed.Camelize();
            if (camelCaseVersion.Equals(identifier, StringComparison.Ordinal))
            {
                return NamingNotationKind.CamelCase;
            }

            var kebabCaseVersion = parsed.Kebaberize();
            if (kebabCaseVersion.Equals(identifier, StringComparison.Ordinal))
            {
                return NamingNotationKind.KebabCase;
            }

            return NamingNotationKind.Unknown;
        }

        private static string CleanName(string name)
        {
            int prefixLength = GetNameStart(name);
            if (prefixLength == 0)
            {
                return name;
            }
            else
            {
                return name.Substring(prefixLength);
            }
        }

        private static int GetNameStart(string name)
        {
            int i = 0;
            int n = name.Length;
            while (i < n)
            {
                if (NamePrefixes.Contains(name[i]))
                {
                    i++;
                }
                else
                {
                    return i;
                }
            }

            return i;
        }
    }
}
