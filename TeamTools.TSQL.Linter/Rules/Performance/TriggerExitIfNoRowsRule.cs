using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("PF0140", "TRIGGER_EXIT_ON_NO_ROWS")]
    [TriggerRule]
    internal sealed class TriggerExitIfNoRowsRule : AbstractRule
    {
        private static readonly string RowCountVar = "@@ROWCOUNT";

        public TriggerExitIfNoRowsRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            // CREATE PROC/TRIGGER/FUNC must be the first statement in a batch
            var firstStmt = batch.Statements[0];
            if (firstStmt is TriggerStatementBody trg)
            {
                ValidateBody(trg);
            }
        }

        private static bool CheckPredicateContents(IfVisitor ifs)
        {
            if (!(ifs.FirstIF.Predicate is BooleanComparisonExpression predicate
            && predicate.FirstExpression is GlobalVariableExpression globalVar
            && predicate.SecondExpression is IntegerLiteral intValue
            && predicate.ComparisonType == BooleanComparisonType.Equals))
            {
                return false;
            }

            return string.Equals(RowCountVar, globalVar.Name, StringComparison.OrdinalIgnoreCase)
                && int.TryParse(intValue.Value, out int limit)
                && limit == 0;
        }

        // TODO : refactoring
        private void ValidateBody(TriggerStatementBody node)
        {
            if (node.TriggerObject.TriggerScope != TriggerScope.Normal)
            {
                // DDL trigger
                return;
            }

            var ifs = new IfVisitor();

            foreach (var st in node.StatementList.Statements)
            {
                st.Accept(ifs);
            }

            // statement order
            if (ifs.FirstIF is null || ifs.FirstIF.FirstTokenIndex > ifs.FirstStatement.FirstTokenIndex)
            {
                HandleNodeError(node);
                return;
            }

            if (!CheckPredicateContents(ifs))
            {
                HandleNodeError(ifs.FirstIF);
                return;
            }

            // then return
            ReturnVisitor ret = new ReturnVisitor();

            ifs.FirstIF.ThenStatement.Accept(ret);

            if (ret.FirstReturn is null || ret.FirstReturn.FirstTokenIndex > ret.FirstStatement.FirstTokenIndex)
            {
                HandleNodeError(node);
            }
        }

        private abstract class StmtVisitor : TSqlFragmentVisitor
        {
            public TSqlStatement FirstStatement { get; private set; }

            public override void Visit(TSqlStatement node)
            {
                if (node is BeginEndBlockStatement)
                {
                    return;
                }

                if (this.FirstStatement != null)
                {
                    return;
                }

                this.FirstStatement = node;
            }
        }

        private sealed class IfVisitor : StmtVisitor
        {
            public IfStatement FirstIF { get; private set; }

            public override void Visit(IfStatement node)
            {
                if (this.FirstIF is null)
                {
                    this.FirstIF = node;
                }
            }
        }

        private sealed class ReturnVisitor : StmtVisitor
        {
            public ReturnStatement FirstReturn { get; private set; }

            public override void Visit(ReturnStatement node)
            {
                if (this.FirstReturn is null)
                {
                    this.FirstReturn = node;
                }
            }
        }
    }
}
