using Humanizer;
using System;
using TeamTools.TSQL.Linter.Interfaces;

namespace TeamTools.TSQL.Linter.Routines
{
    internal class IdentifierNotationResolver : INotationResolver
    {
        private static readonly char[] TrimmedPrefixes = new char[]
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

            identifier = identifier.TrimStart(TrimmedPrefixes);
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
    }
}
