using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Func specifics.
    /// </summary>
    internal partial class NativelyUnsupportedInstructionRule
    {
        private static readonly ICollection<FunctionOptionKind> UnsupportedFuncOptions = new List<FunctionOptionKind>
        {
            FunctionOptionKind.Encryption,
            FunctionOptionKind.Inline,
        };

        public override void Visit(FunctionStatementBody node)
        {
            if (!node.Options.Any(opt => opt.OptionKind == FunctionOptionKind.NativeCompilation))
            {
                return;
            }

            if (node.ReturnType is TableValuedFunctionReturnType)
            {
                HandleNodeError(node, "TVF function");
            }

            node.Options
                .Where(opt => UnsupportedFuncOptions.Contains(opt.OptionKind))
                .ToList()
                .ForEach(opt => HandleNodeError(opt, opt.OptionKind.ToString().ToUpperInvariant()));

            DoValidateStatements(node, node.StatementList, !(node.ReturnType is SelectFunctionReturnType));
        }
    }
}
