using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0912", "WINDOW_MISSING_ORDERBY")]
    internal sealed class WindowFunctionRequiresOrderByRule : AbstractRule
    {
        private static readonly HashSet<string> WindowFunctionsRequiringOrderBy = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ROW_NUMBER",
            "RANK",
            "DENSE_RANK",
            "NTILE",
            "LEAD",
            "LAG",
        };

        public WindowFunctionRequiresOrderByRule() : base()
        {
        }

        public override void Visit(FunctionCall node)
        {
            if (node.OverClause is null)
            {
                return;
            }

            if (!(node.OverClause.OrderByClause is null))
            {
                return;
            }

            string funcName = node.FunctionName.Value;

            if (!WindowFunctionsRequiringOrderBy.Contains(funcName))
            {
                return;
            }

            HandleNodeError(node);
        }
    }
}
