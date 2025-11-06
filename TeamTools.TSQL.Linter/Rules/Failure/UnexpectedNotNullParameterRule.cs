using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0918", "INVALID_NOT_NULL_PARAM")]
    [CompatibilityLevel(SqlVersion.Sql130)]
    public sealed class UnexpectedNotNullParameterRule : AbstractRule
    {
        public UnexpectedNotNullParameterRule() : base()
        {
        }

        public override void Visit(ProcedureStatementBody node)
        {
            if (node.Options.Any(opt => opt.OptionKind == ProcedureOptionKind.NativeCompilation))
            {
                // NOT NULL params are allowed in Natively compiled procs
                return;
            }

            ValidateParamsNullability(node.Parameters);
        }

        public override void Visit(FunctionStatementBody node)
        {
            if (node.ReturnType is SelectFunctionReturnType)
            {
                // NOT NULL params are allowed in inlined table-valued functions
                return;
            }

            ValidateParamsNullability(node.Parameters);
        }

        private void ValidateParamsNullability(IList<ProcedureParameter> parameters)
        {
            if (parameters.Count == 0)
            {
                return;
            }

            var notNullParams = parameters.Where(prm => prm.Nullable?.Nullable == false);

            foreach (var prm in notNullParams)
            {
                HandleNodeError(prm.Nullable, prm.VariableName.Value);
            }
        }
    }
}
