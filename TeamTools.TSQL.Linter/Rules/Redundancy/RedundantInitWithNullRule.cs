using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0798", "REDUNDANT_INIT_NULL")]
    internal sealed class RedundantInitWithNullRule : AbstractRule
    {
        public RedundantInitWithNullRule() : base()
        {
        }

        // Explicit - to ignore ProcedureParameter declarations
        // having NULL as default is fine for them
        public override void ExplicitVisit(DeclareVariableElement node)
        {
            if (node.Value is null)
            {
                // no init value
                return;
            }

            if (node.Value.ExtractScalarExpression() is NullLiteral)
            {
                HandleNodeError(node.Value, node.VariableName.Value);
            }
        }
    }
}
