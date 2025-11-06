using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0289", "ODBC_SYNTAX")]
    internal sealed class OdbcSyntaxRule : AbstractRule
    {
        public OdbcSyntaxRule() : base()
        {
        }

        public override void Visit(OdbcConvertSpecification node)
        {
            HandleNodeError(node);
        }

        public override void Visit(OdbcFunctionCall node)
        {
            HandleNodeError(node);
        }

        public override void Visit(OdbcLiteral node)
        {
            HandleNodeError(node);
        }

        public override void Visit(OdbcQualifiedJoinTableReference node)
        {
            HandleNodeError(node);
        }
    }
}
