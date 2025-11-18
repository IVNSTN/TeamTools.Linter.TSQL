using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Diagnostics.CodeAnalysis;

namespace TeamTools.TSQL.ExpressionEvaluator.Evaluation
{
    [ExcludeFromCodeCoverage]
    public class WalkThroughProtector
    {
        public int LastAnalyzedToken { get; private set; } = 0;

        public void Run(TSqlFragment block)
        {
            if (block.FirstTokenIndex <= LastAnalyzedToken)
            {
                // kilroy waz here
                return;
            }

            // in case of nested calls strange things may happen
            LastAnalyzedToken = block.LastTokenIndex > LastAnalyzedToken ? block.LastTokenIndex : LastAnalyzedToken;
        }

        public void Run<T>(T block, Action<T> callback)
        where T : TSqlFragment
        {
            if (block.FirstTokenIndex <= LastAnalyzedToken)
            {
                // kilroy waz here
                return;
            }

            callback.Invoke(block);

            // in case of nested calls strange things may happen
            LastAnalyzedToken = block.LastTokenIndex > LastAnalyzedToken ? block.LastTokenIndex : LastAnalyzedToken;
        }

        public void Run<T>(TSqlFragment block, T pass, Action<T> callback)
        where T : TSqlFragment
        {
            if (block.FirstTokenIndex <= LastAnalyzedToken)
            {
                // kilroy waz here
                return;
            }

            callback.Invoke(pass);

            // in case of nested calls strange things may happen
            LastAnalyzedToken = block.LastTokenIndex > LastAnalyzedToken ? block.LastTokenIndex : LastAnalyzedToken;
        }
    }
}
