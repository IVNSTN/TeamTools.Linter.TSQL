using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Func specifics.
    /// </summary>
    internal partial class NativelyUnsupportedInstructionRule
    {
        private static readonly Dictionary<FunctionOptionKind, string> UnsupportedFuncOptions = new Dictionary<FunctionOptionKind, string>
        {
            { FunctionOptionKind.Encryption, "ENCRYPTION" },
            { FunctionOptionKind.Inline, "INLINE" },
        };

        private void DoValidate(FunctionStatementBody node)
        {
            if (!node.Options.HasOption(FunctionOptionKind.NativeCompilation))
            {
                return;
            }

            if (node.ReturnType is TableValuedFunctionReturnType)
            {
                HandleNodeError(node, "TVF function");
            }

            int n = node.Options.Count;
            for (int i = 0; i < n; i++)
            {
                var opt = node.Options[i];
                if (UnsupportedFuncOptions.TryGetValue(opt.OptionKind, out var optionName))
                {
                    HandleNodeError(opt, optionName);
                }
            }

            DoValidateStatements(node, node.StatementList, !(node.ReturnType is SelectFunctionReturnType));
        }
    }
}
