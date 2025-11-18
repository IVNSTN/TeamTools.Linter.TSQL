using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0143", "PROC_RETURN_REQUIRED_AFTER_CATCH")]
    internal sealed class ReturnValueFromProcIfTryCatchUsedRule : AbstractRule
    {
        public ReturnValueFromProcIfTryCatchUsedRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            // CREATE PROC/TRIGGER/FUNC must be the first statement in a batch
            var firstStmt = batch.Statements[0];
            if (firstStmt is ProcedureStatementBody proc)
            {
                DoValidate(proc);
            }
        }

        private static TSqlParserToken LocateCatchToken(TSqlFragment node, int catchStartTokenIndex)
        {
            for (int i = catchStartTokenIndex, n = node.ScriptTokenStream.Count; i < n; i++)
            {
                var token = node.ScriptTokenStream[i];
                if (token.TokenType == TSqlTokenType.Identifier
                && string.Equals(token.Text, "CATCH", StringComparison.OrdinalIgnoreCase))
                {
                    return token;
                }
            }

            return node.ScriptTokenStream[node.LastTokenIndex];
        }

        private void DoValidate(ProcedureStatementBody node)
        {
            var catchVisitor = new CatchThrowReturnVisitor();
            node.AcceptChildren(catchVisitor);
            if (catchVisitor.TryCatch is null)
            {
                return;
            }

            int catchStart = catchVisitor.TryCatch.CatchStatements.FirstTokenIndex;
            int catchEnd = catchVisitor.TryCatch.CatchStatements.LastTokenIndex;

            if (catchEnd == -1)
            {
                // Some parser bug/unexpected behavior: when CATCH is empty CatchStatements.LastTokenIndex is not initialized
                // TryStatements cannot be empty
                catchStart = catchVisitor.TryCatch.TryStatements.LastTokenIndex;
                catchEnd = catchStart;
            }

            if (catchVisitor.Return?.FirstTokenIndex >= catchStart)
            {
                // there is RETURN inside or after CATCH
                return;
            }

            if (catchVisitor.Throw != null)
            {
                // if error is reraised in catch then don't care about return value
                return;
            }

            HandleTokenError(LocateCatchToken(catchVisitor.TryCatch, catchEnd));
        }

        private sealed class CatchThrowReturnVisitor : TSqlFragmentVisitor
        {
            public TryCatchStatement TryCatch { get; private set; }

            public TSqlFragment Return { get; private set; }

            public TSqlFragment Throw { get; private set; }

            // the last one needed so overwriting is safe
            public override void Visit(TryCatchStatement node) => TryCatch = node;

            public override void Visit(ReturnStatement node) => Return = node;

            public override void Visit(ThrowStatement node) => DetectThrow(node);

            // TODO : ignore with low severity level?
            public override void Visit(RaiseErrorStatement node) => DetectThrow(node);

            private void DetectThrow(TSqlFragment node)
            {
                if (TryCatch is null)
                {
                    return;
                }

                // We are looking for THROW inside CATCH. If CATCH is empty then FirstTokenIndex = -1 and the THROW is not there for sure.
                if (false && TryCatch.CatchStatements.FirstTokenIndex == -1)
                {
                    return;
                }

                if (node.FirstTokenIndex >= TryCatch.CatchStatements.FirstTokenIndex && node.LastTokenIndex <= TryCatch.CatchStatements.LastTokenIndex)
                {
                    Throw = node;
                }
            }
        }
    }
}
