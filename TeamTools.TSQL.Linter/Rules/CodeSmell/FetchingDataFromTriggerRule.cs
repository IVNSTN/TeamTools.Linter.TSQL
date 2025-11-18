using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0163", "OUTPUT_FROM_TRIGGER")]
    [TriggerRule]
    internal sealed class FetchingDataFromTriggerRule : AbstractRule
    {
        private readonly FetchingDataVisitor fetchDetector;

        public FetchingDataFromTriggerRule() : base()
        {
            fetchDetector = new FetchingDataVisitor(ViolationHandler);
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            // CREATE PROC/TRIGGER/FUNC must be the first statement in a batch
            var firstStmt = batch.Statements[0];
            if (firstStmt is TriggerStatementBody trg)
            {
                DoValidate(trg.StatementList);
            }
        }

        private void DoValidate(StatementList node) => node?.AcceptChildren(fetchDetector);

        private class FetchingDataVisitor : VisitorWithCallback
        {
            private const int MaxInfoSeverity = 10;
            private readonly List<TSqlFragment> ignoredStatements = new List<TSqlFragment>();

            public FetchingDataVisitor(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(PrintStatement node) => Callback(node);

            // without INTO
            public override void Visit(OutputClause node) => Callback(node);

            public override void Visit(CursorDefinition node) => ignoredStatements.Add(node.Select);

            public override void Visit(RaiseErrorStatement node)
            {
                if (!(node.SecondParameter is IntegerLiteral intArg))
                {
                    return;
                }

                if (int.TryParse(intArg.Value, out int severity) && severity > MaxInfoSeverity)
                {
                    return;
                }

                Callback(node);
            }

            public override void Visit(SelectStatement node)
            {
                if (ignoredStatements.Contains(node))
                {
                    return;
                }

                // not a DML or scalar expression
                if (null != node.Into)
                {
                    return;
                }

                var qs = node.QueryExpression.GetQuerySpecification();

                if (qs is null)
                {
                    return;
                }

                if (qs.SelectElements.OfType<SelectSetVariable>().Any())
                {
                    return;
                }

                Callback(node);
            }
        }
    }
}
