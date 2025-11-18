using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.TSQL.Linter.Interfaces;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    public abstract class BaseNamingConventionRule : AbstractRule
    {
        private static readonly IdentifierNotationResolver ConventionResolver;

        static BaseNamingConventionRule()
        {
            ConventionResolver = new IdentifierNotationResolver();
        }

        protected BaseNamingConventionRule() : base()
        {
        }

        protected void ValidateNamingNotation(TSqlFragment node, string name, NamingNotationKind expectedNotation)
        {
            if (!string.IsNullOrEmpty(name) && !IsValidName(name, expectedNotation))
            {
                DoReportConventionViolation(node, name, expectedNotation);
            }
        }

        private static bool IsValidName(string name, NamingNotationKind expectedNotation)
        {
            var notation = ConventionResolver.Resolve(name);

            return notation == NamingNotationKind.Unknown || notation == expectedNotation;
        }

        private static string GetValidName(string originalName, NamingNotationKind targetNotation)
            => IdentifierNotationResolver.ConvertTo(originalName, targetNotation);

        private void DoReportConventionViolation(TSqlFragment node, string currentName, NamingNotationKind expectedNotation)
        {
            HandleNodeError(node, string.Format("{0} vs {1}", currentName, GetValidName(currentName, expectedNotation)));
        }
    }
}
