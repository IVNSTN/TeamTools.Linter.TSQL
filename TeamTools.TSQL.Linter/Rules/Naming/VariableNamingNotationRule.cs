using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Interfaces;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("NM0205", "VAR_NAME_NOTATION_MIX")]
    internal sealed class VariableNamingNotationRule : AbstractRule
    {
        private static readonly Lazy<IDictionary<NamingNotationKind, string>> NamingNotationTitlesInstance
            = new Lazy<IDictionary<NamingNotationKind, string>>(() => InitNamingNotationTitlesInstance(), true);

        public VariableNamingNotationRule() : base()
        {
        }

        private static IDictionary<NamingNotationKind, string> NamingNotationTitles => NamingNotationTitlesInstance.Value;

        public override void Visit(TSqlScript node)
        {
            var notationVisitor = new VariableNotationVisitor(
                NamingNotationTitles,
                new IdentifierNotationResolver(),
                HandleNodeError);
            node.Accept(notationVisitor);
        }

        private static IDictionary<NamingNotationKind, string> InitNamingNotationTitlesInstance()
        {
            return new SortedDictionary<NamingNotationKind, string>
            {
                { NamingNotationKind.Unknown, "?" },
                { NamingNotationKind.SnakeLowerCase, "snake_case" },
                { NamingNotationKind.SnakeUpperCase, "UPPER_SNAKE_CASE" },
                { NamingNotationKind.CamelCase, "camelCase" },
                { NamingNotationKind.PascalCase, "PascalCase" },
                { NamingNotationKind.KebabCase, "kebab-case" },
            };
        }

        private static Regex MakeRegex(string pattern)
        {
            return new Regex(pattern, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.CultureInvariant);
        }

        private class VariableNotationVisitor : TSqlFragmentVisitor
        {
            private static readonly Regex IteratorName = MakeRegex(@"^@([a-z]{0,3}[0-9]*)$");
            private readonly IDictionary<NamingNotationKind, string> namingNotationTitles;
            private readonly IDictionary<NamingNotationKind, string> detectedNotations = new Dictionary<NamingNotationKind, string>();
            private readonly INotationResolver notationResolver;
            private readonly Action<TSqlFragment, string> callback;
            private NamingNotationKind firstNotation = NamingNotationKind.Unknown;

            public VariableNotationVisitor(
                IDictionary<NamingNotationKind, string> namingNotationTitles,
                INotationResolver notationResolver,
                Action<TSqlFragment, string> callback)
            {
                this.namingNotationTitles = namingNotationTitles;
                this.notationResolver = notationResolver;
                this.callback = callback;
            }

            public override void Visit(DeclareVariableElement node)
            {
                string varName = node.VariableName.Value;

                if (IteratorName.IsMatch(varName))
                {
                    // ignoring names like @i, @j and digit-only names
                    // unreadable names are handled by different rule
                    return;
                }

                NamingNotationKind notation = notationResolver.Resolve(varName);

                if (detectedNotations.ContainsKey(notation))
                {
                    return;
                }

                // registering new notation we just found
                detectedNotations[notation] = varName;

                if (detectedNotations.Count == 1)
                {
                    firstNotation = notation;
                    return;
                }

                if (detectedNotations.Count == 2)
                {
                    // we did not report anything until found second notation
                    // to show difference we must report the first one found too
                    ReportViolation(node, firstNotation);
                }

                // reporting if found something new
                ReportViolation(node, notation);
            }

            private void ReportViolation(TSqlFragment node, NamingNotationKind notation)
            {
                callback(node, string.Format("{0} [{1}]", detectedNotations[notation], namingNotationTitles[notation]));
            }
        }
    }
}
