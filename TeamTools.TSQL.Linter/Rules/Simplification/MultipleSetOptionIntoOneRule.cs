using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("SI0896", "MULTIPLE_SET_INTO_ONE")]
    internal sealed class MultipleSetOptionIntoOneRule : AbstractRule
    {
        public MultipleSetOptionIntoOneRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch batch) => batch.Accept(new SetVisitor(ViolationHandler));

        private sealed class SetVisitor : VisitorWithCallback
        {
            public SetVisitor(Action<TSqlFragment> callback) : base(callback)
            { }

            private TSqlFragment LastSetOnNode { get; set; }

            private TSqlFragment LastSetOffNode { get; set; }

            public override void Visit(TSqlBatch node) => Validate(node.Statements);

            public override void Visit(BeginEndBlockStatement node) => Validate(node.StatementList.Statements);

            public override void Visit(TryCatchStatement node) => Validate(node.TryStatements.Statements);

            private void Validate(IList<TSqlStatement> statements)
            {
                for (int i = 0, n = statements.Count; i < n; i++)
                {
                    if (statements[i] is PredicateSetStatement set)
                    {
                        HandleSet(set);
                    }
                    else
                    {
                        Reset();
                    }
                }
            }

            private void HandleSet(PredicateSetStatement node)
            {
                if (node.IsOn)
                {
                    if (LastSetOnNode != null)
                    {
                        Callback(node);
                    }

                    LastSetOnNode = node;
                }
                else
                {
                    if (LastSetOffNode != null)
                    {
                        Callback(node);
                    }

                    LastSetOffNode = node;
                }
            }

            private void Reset()
            {
                LastSetOnNode = null;
                LastSetOffNode = null;
            }
        }
    }
}
