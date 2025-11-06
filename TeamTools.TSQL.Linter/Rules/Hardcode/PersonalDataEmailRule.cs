using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Text.RegularExpressions;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("HD0257", "PERSONAL_DATA_EMAIL")]
    internal sealed class PersonalDataEmailRule : AbstractRule
    {
        // \w before @ is a simplification to avoid XML patterns false positive matches
        private readonly Regex emailRegex = new Regex(
            "(?<output>[a-zA-Z0-9.!#$%&’*+/=?^_`{|}~-]+[\\w]@[a-zA-Z0-9-]+(?:\\.[a-zA-Z0-9-]+)*)",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private readonly Regex commentedVariable = new Regex("^[\\W]+\\s*[@]", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public PersonalDataEmailRule() : base()
        {
        }

        public override void Visit(TSqlScript node)
        {
            var regexMatchVisitor = new StringRegexMatchVisitor(
                emailRegex,
                true,
                (token, value, fullString) =>
                {
                    // very likely a commented variable e.g. --@var which is valid email but not what we want
                    if (commentedVariable.IsMatch(value))
                    {
                        return;
                    }

                    HandleTokenError(node.ScriptTokenStream[token]);
                });
            regexMatchVisitor.DetectMatch(node);
        }
    }
}
