using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0456", "USELESS_UNIT")]
    internal sealed partial class UselessUnitRule : AbstractRule
    {
        public UselessUnitRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            // CREATE PROC/TRIGGER/FUNC must be the first statement in a batch
            var firstStmt = batch.Statements[0];
            if (firstStmt is ProcedureStatementBody proc)
            {
                if (proc.MethodSpecifier != null)
                {
                    // external proc
                    return;
                }

                ValidateBody(proc.StatementList, proc);
            }
            else if (firstStmt is TriggerStatementBody trg)
            {
                if (trg.MethodSpecifier != null)
                {
                    // external trg
                    return;
                }

                ValidateBody(trg.StatementList, trg);
            }
            else if (firstStmt is FunctionStatementBody fn)
            {
                if (fn.MethodSpecifier != null)
                {
                    // external fn
                    return;
                }

                if (fn.ReturnType is SelectFunctionReturnType inlineTableFunc)
                {
                    // let's say that SELECT is good enough as a reason for a function to exist
                    return;
                }

                ValidateBody(fn.StatementList, fn);
            }
        }

        private static bool ContainsMeaningfulStatementInBody(TSqlStatement node)
        {
            var visitor = new BodyVisitor();
            node.AcceptChildren(visitor);
            return visitor.ContainsMeaningfulStatement;
        }

        private static bool IsSimpleExampleOfUselessUnit(TSqlStatement firstStmt)
        {
            if (firstStmt is PrintStatement)
            {
                return true;
            }

            if (firstStmt is ReturnStatement ret
            && (ret.Expression is null || ret.Expression is Literal))
            {
                return true;
            }

            if (firstStmt is ThrowStatement)
            {
                return true;
            }

            if (firstStmt is RaiseErrorStatement)
            {
                return true;
            }

            return false;
        }

        private void ValidateBody(StatementList body, TSqlStatement stmt)
        {
            // Expanding outermost and nested BEGIN-ENDs if any
            while (body != null && body.Statements.Count > 0
            && body.Statements[0] is BeginEndBlockStatement be)
            {
                body = be.StatementList;
            }

            if (body is null || body.Statements.Count == 0)
            {
                // no body
                HandleNodeError(stmt);
                return;
            }

            var firstStatement = body.Statements[0];

            // No need to instantiate and run visitor for simple cases.
            if (body.Statements.Count == 1 && IsSimpleExampleOfUselessUnit(firstStatement))
            {
                HandleNodeError(firstStatement);
                return;
            }

            if (ContainsMeaningfulStatementInBody(stmt))
            {
                return;
            }

            HandleNodeError(firstStatement);
        }
    }
}
