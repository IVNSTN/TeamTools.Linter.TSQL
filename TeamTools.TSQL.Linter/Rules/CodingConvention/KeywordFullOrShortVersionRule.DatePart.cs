using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Diagnostics;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// DD/DAY, Y/YYYY/YEAR spelling.
    /// </summary>
    internal partial class KeywordFullOrShortVersionRule
    {
        public override void Visit(FunctionCall node)
        {
            Debug.Assert(dateFunctions.Count > 0, "dateFunctions not loaded");

            string functionName = node.FunctionName.Value;
            if (string.IsNullOrEmpty(functionName) || !dateFunctions.Contains(functionName))
            {
                return;
            }

            if ((node.Parameters?.Count ?? 0) == 0)
            {
                return;
            }

            string datePartname = ExtractDatePartName(node.Parameters[0]);
            if (string.IsNullOrEmpty(datePartname) || !dateParts.ContainsKey(datePartname))
            {
                return;
            }

            HandleNodeError(node.Parameters[0], dateParts[datePartname]);
        }

        private static string ExtractDatePartName(ScalarExpression node)
        {
            if (node is Literal l)
            {
                return l.Value;
            }

            if (node is ColumnReferenceExpression r // parser takes them as "columns"
            && r.MultiPartIdentifier?.Identifiers.Count == 1)
            {
                return r.MultiPartIdentifier.Identifiers[0].Value;
            }

            return default;
        }
    }
}
