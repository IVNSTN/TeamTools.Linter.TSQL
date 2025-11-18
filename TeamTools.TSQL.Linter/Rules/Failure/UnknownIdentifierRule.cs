using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0127", "UNKNOWN_IDENTIFIER")]
    internal sealed class UnknownIdentifierRule : AbstractRule, ISqlServerMetadataConsumer
    {
        private HashSet<string> functionsWithMagicLiterals;

        public UnknownIdentifierRule() : base()
        {
        }

        // TODO : save DatePart parameter index and validate
        // that only this parameter has "magic name" argument value
        // and this is one of well known names
        public void LoadMetadata(SqlServerMetadata data)
        {
            var dateFunctions = data.Functions
                .Where(f => f.Value.ParamDefinition != null
                    && f.Value.ParamDefinition.Any(p => p.Value.Equals(TSqlDomainAttributes.DateTimePartEnum, StringComparison.OrdinalIgnoreCase)))
                .Select(f => f.Key);

            functionsWithMagicLiterals = new HashSet<string>(dateFunctions, StringComparer.OrdinalIgnoreCase);
        }

        protected override void ValidateBatch(TSqlBatch node)
        {
            Debug.Assert(functionsWithMagicLiterals.Count > 0, "functionsWithMagicLiterals not loaded");

            var visitor = new UknownIdentifierCatcher(functionsWithMagicLiterals, ViolationHandler);
            node.AcceptChildren(visitor);
        }

        private sealed class UknownIdentifierCatcher : TSqlFragmentVisitor
        {
            private readonly ColumnReferenceVisitor colVisitor = new ColumnReferenceVisitor();
            private readonly HashSet<string> funcs;
            private readonly Action<TSqlFragment> callback;

            public UknownIdentifierCatcher(HashSet<string> funcs, Action<TSqlFragment> callback)
            {
                this.funcs = funcs;
                this.callback = callback;
            }

            public override void Visit(QueryExpression node) => node.AcceptChildren(colVisitor);

            public override void Visit(IndexStatement node) => node.AcceptChildren(colVisitor);

            public override void Visit(IndexDefinition node) => node.AcceptChildren(colVisitor);

            public override void Visit(TableDefinition node) => node.AcceptChildren(colVisitor);

            public override void Visit(DataModificationSpecification node) => node.AcceptChildren(colVisitor);

            public override void Visit(OutputIntoClause node) => node.AcceptChildren(colVisitor);

            public override void Visit(ReceiveStatement node) => node.AcceptChildren(colVisitor);

            public override void Visit(CreateStatisticsStatement node) => node.AcceptChildren(colVisitor);

            public override void Visit(FunctionCall node)
            {
                if (!funcs.Contains(node.FunctionName.Value) || node.Parameters.Count == 0)
                {
                    return;
                }

                node.AcceptChildren(colVisitor);
            }

            public override void Visit(ColumnReferenceExpression node)
            {
                // these are supposed to happen later containing clauses mentioned above
                if (colVisitor.Columns.Contains(node))
                {
                    return;
                }

                callback(node);
            }
        }

        private sealed class ColumnReferenceVisitor : TSqlFragmentVisitor
        {
            public HashSet<TSqlFragment> Columns { get; } = new HashSet<TSqlFragment>();

            public override void Visit(ColumnReferenceExpression node) => Columns.Add(node);
        }
    }
}
