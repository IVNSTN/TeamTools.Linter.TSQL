using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0912", "WINDOW_MISSING_ORDERBY")]
    internal sealed class WindowFunctionRequiresOrderByRule : AbstractRule
    {
        private static readonly Lazy<ICollection<string>> WindowFunctionsRequiringOrderByInstance
            = new Lazy<ICollection<string>>(() => InitWindowFunctionsRequiringOrderByInstance(), true);

        public WindowFunctionRequiresOrderByRule() : base()
        {
        }

        private static ICollection<string> WindowFunctionsRequiringOrderBy => WindowFunctionsRequiringOrderByInstance.Value;

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

        private static ICollection<string> InitWindowFunctionsRequiringOrderByInstance()
        {
            return new SortedSet<string>(StringComparer.OrdinalIgnoreCase)
            {
               "ROW_NUMBER",
               "RANK",
               "DENSE_RANK",
               "NTILE",
               "LEAD",
               "LAG",
            };
        }
    }
}
