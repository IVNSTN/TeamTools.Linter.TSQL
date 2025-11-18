using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.ExpressionEvaluator;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0161", "UNREACHABLE_CODE")]
    internal sealed class UnreachableCodeRule : AbstractRule
    {
        private readonly Action<TSqlFragment, TSqlTokenType> handleDelegate;

        public UnreachableCodeRule() : base()
        {
            handleDelegate = new Action<TSqlFragment, TSqlTokenType>((i, _) => HandleNodeError(i));
        }

        protected override void ValidateBatch(TSqlBatch node)
        {
            if (!ScalarExpressionEvaluator.IsBatchInteresting(node))
            {
                return;
            }

            node.Accept(new RecursiveQuitBlockVisitor(handleDelegate));
        }
    }
}
