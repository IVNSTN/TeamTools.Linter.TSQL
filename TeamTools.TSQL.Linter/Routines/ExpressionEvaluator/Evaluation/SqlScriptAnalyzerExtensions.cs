using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Linq;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.Evaluation
{
    public static class SqlScriptAnalyzerExtensions
    {
        public static bool CheckIfBlockBreaksBatch(this SqlScriptAnalyzer scriptAnalyzer, TSqlStatement node)
        {
            if (node is BeginEndBlockStatement be)
            {
                return be.StatementList.Statements.Any(st => scriptAnalyzer.CheckIfBlockBreaksBatch(st));
            }

            if (node is ThrowStatement)
            {
                return true;
            }

            if (node is ReturnStatement)
            {
                return true;
            }

            return false;
        }

        public static bool CheckIfBlockIsNestedIf(this SqlScriptAnalyzer scriptAnalyzer, TSqlStatement node)
        {
            if (node is BeginEndBlockStatement be)
            {
                if (be.StatementList.Statements.Count > 1)
                {
                    return false;
                }

                return scriptAnalyzer.CheckIfBlockIsNestedIf(be.StatementList.Statements[0]);
            }

            return node is IfStatement;
        }
    }
}
