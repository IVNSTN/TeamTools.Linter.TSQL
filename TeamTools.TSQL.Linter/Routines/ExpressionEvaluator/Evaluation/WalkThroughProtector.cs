using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Diagnostics.CodeAnalysis;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.Evaluation
{
    [ExcludeFromCodeCoverage]
    public class WalkThroughProtector
    {
        public int LastAnalyzedToken { get; private set; } = 0;

        public void Run(TSqlFragment block, Action callback)
        {
            if (block.FirstTokenIndex < LastAnalyzedToken)
            {
                // kilroy waz here
                return;
            }

            callback.Invoke();

            // in case of nested calls strange things may happen
            LastAnalyzedToken = block.LastTokenIndex > LastAnalyzedToken ? block.LastTokenIndex : LastAnalyzedToken;
        }
    }
}
