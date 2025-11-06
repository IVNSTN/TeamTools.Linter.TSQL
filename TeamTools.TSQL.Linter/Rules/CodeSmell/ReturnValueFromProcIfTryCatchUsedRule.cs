using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0143", "PROC_RETURN_REQUIRED_AFTER_CATCH")]
    internal sealed class ReturnValueFromProcIfTryCatchUsedRule : AbstractRule
    {
        public ReturnValueFromProcIfTryCatchUsedRule() : base()
        {
        }

        public override void Visit(ProcedureStatementBody node)
        {
            var catchVisitor = new CatchVisitor();
            node.AcceptChildren(catchVisitor);
            if (catchVisitor.LastDetectedNode is null)
            {
                return;
            }

            var returnVisitor = new ReturnVisitor();
            node.AcceptChildren(returnVisitor);
            if (returnVisitor.Detected && returnVisitor.TokenIndex > catchVisitor.LastDetectedNode.FirstTokenIndex)
            {
                return;
            }

            var throwVisitor = new ThrowVisitor();
            catchVisitor.LastDetectedNode.AcceptChildren(throwVisitor);

            // if error is reraised then don't care about return value
            if (throwVisitor.Detected)
            {
                return;
            }

            HandleTokenError(LocateCatchToken(catchVisitor.LastDetectedNode));
        }

        private static TSqlParserToken LocateCatchToken(TSqlFragment node)
        {
            int i = node.LastTokenIndex;
            int n = node.ScriptTokenStream.Count;

            while (i < n)
            {
                if (node.ScriptTokenStream[i].TokenType == TSqlTokenType.Identifier
                && string.Equals(node.ScriptTokenStream[i].Text, "CATCH", StringComparison.OrdinalIgnoreCase))
                {
                    return node.ScriptTokenStream[i];
                }

                i++;
            }

            return node.ScriptTokenStream[node.LastTokenIndex];
        }

        private class CatchVisitor : TSqlViolationDetector
        {
            // the last one needed so overwriting is safe
            public override void Visit(TryCatchStatement node) => MarkDetected(node.CatchStatements);
        }

        private class ReturnVisitor : TSqlViolationDetector
        {
            public int TokenIndex { get; private set; } = 0;

            public override void Visit(ReturnStatement node)
            {
                MarkDetected(node);
                if (node.LastTokenIndex > TokenIndex)
                {
                    TokenIndex = node.LastTokenIndex;
                }
            }
        }

        private class ThrowVisitor : TSqlViolationDetector
        {
            public override void Visit(ThrowStatement node) => MarkDetected(node);

            public override void Visit(RaiseErrorStatement node) => MarkDetected(node);
        }
    }
}
