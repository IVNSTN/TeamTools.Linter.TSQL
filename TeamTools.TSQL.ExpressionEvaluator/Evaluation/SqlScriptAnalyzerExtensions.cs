using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace TeamTools.TSQL.ExpressionEvaluator.Evaluation
{
    public static class SqlScriptAnalyzerExtensions
    {
        public static bool CheckIfBlockBreaksBatch(this SqlScriptAnalyzer scriptAnalyzer, TSqlStatement node)
        {
            if (node is BeginEndBlockStatement be)
            {
                return scriptAnalyzer.HasItemBreakingBlock(be.StatementList.Statements);
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

        private static bool HasItemBreakingBlock(this SqlScriptAnalyzer scriptAnalyzer, IList<TSqlStatement> statements)
        {
            int n = statements.Count;
            for (int i = 0; i < n; i++)
            {
                if (scriptAnalyzer.CheckIfBlockBreaksBatch(statements[i]))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
