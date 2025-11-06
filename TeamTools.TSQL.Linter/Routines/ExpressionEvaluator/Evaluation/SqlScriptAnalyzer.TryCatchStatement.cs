using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.Evaluation
{
    /// <summary>
    /// TRY-CATCH blocks processing.
    /// </summary>
    public partial class SqlScriptAnalyzer
    {
        // TRY-CATCH blocks are like IF-ELSE
        // any assignements in try or catch should be reset afterwards
        // because we don't know how it ended up: did we reach any assignment in
        // TRY block, did we fall into CATCH block.
        public override void Visit(TryCatchStatement node)
        {
            walkThrough.Run(node.TryStatements, () =>
            {
                node.TryStatements.Accept(this);
                conditionHandler.ResetValueEstimatesAfterConditionalBlock(node.TryStatements);
            });

            walkThrough.Run(node.CatchStatements, () =>
            {
                node.CatchStatements.Accept(this);
                conditionHandler.ResetValueEstimatesAfterConditionalBlock(node.CatchStatements);
            });
        }
    }
}
