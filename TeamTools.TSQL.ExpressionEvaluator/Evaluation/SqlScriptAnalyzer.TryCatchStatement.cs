using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;

namespace TeamTools.TSQL.ExpressionEvaluator.Evaluation
{
    /// <summary>
    /// TRY-CATCH blocks processing.
    /// </summary>
    public partial class SqlScriptAnalyzer
    {
        private Action<StatementList> evalTryCatch;

        // TRY-CATCH blocks are like IF-ELSE
        // any assignements in try or catch should be reset afterwards
        // because we don't know how it ended up: did we reach any assignment in
        // TRY block, did we fall into CATCH block.
        public override void Visit(TryCatchStatement node)
        {
            if (evalTryCatch is null)
            {
                evalTryCatch = new Action<StatementList>(nd =>
                {
                    nd.AcceptChildren(this);
                    conditionHandler.ResetValueEstimatesAfterConditionalBlock(nd);
                });
            }

            walkThrough.Run(node.TryStatements, evalTryCatch);

            if (node.CatchStatements?.Statements?.Count > 0)
            {
                walkThrough.Run(node.CatchStatements, evalTryCatch);
            }
        }
    }
}
