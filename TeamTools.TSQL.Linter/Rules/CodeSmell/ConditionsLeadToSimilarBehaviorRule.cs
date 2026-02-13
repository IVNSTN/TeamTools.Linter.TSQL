using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0844", "CONDITIONS_SAME_DECISIONS")]
    internal sealed class ConditionsLeadToSimilarBehaviorRule : AbstractRule
    {
        public ConditionsLeadToSimilarBehaviorRule() : base()
        {
        }

        public override void Visit(SimpleCaseExpression node)
        {
            var flows = new List<ScalarExpression>(node.WhenClauses.Count + 1);
            for (int i = node.WhenClauses.Count - 1; i >= 0; i--)
            {
                flows.Add(node.WhenClauses[i].ThenExpression);
            }

            if (node.ElseExpression != null)
            {
                flows.Add(node.ElseExpression);
            }

            DetectDups(flows);
        }

        // Searched Case and Simple Case are very similar however don't have common ancestor
        public override void Visit(SearchedCaseExpression node)
        {
            var flows = new List<ScalarExpression>(node.WhenClauses.Count + 1);
            for (int i = node.WhenClauses.Count - 1; i >= 0; i--)
            {
                flows.Add(node.WhenClauses[i].ThenExpression);
            }

            if (node.ElseExpression != null)
            {
                flows.Add(node.ElseExpression);
            }

            DetectDups(flows);
        }

        public override void Visit(IfStatement node)
        {
            var flows = new List<TSqlStatement>();

            while (node != null)
            {
                flows.Add(node.ThenStatement);
                if (node.ElseStatement is IfStatement elseIf)
                {
                    node = elseIf;
                }
                else
                {
                    if (node.ElseStatement != null)
                    {
                        flows.Add(node.ElseStatement);
                    }

                    node = null;
                }
            }

            DetectDups(flows);
        }

        public override void Visit(IIfCall node)
        {
            var flows = new List<ScalarExpression>(2);
            flows.Add(node.ThenExpression);
            flows.Add(node.ElseExpression);

            DetectDups(flows);
        }

        // Expanding flow expression/statement list
        private static TSqlFragment ExpandFragment(TSqlFragment node)
        {
            while (node is ParenthesisExpression pe)
            {
                node = pe.Expression;
            }

            while (node is BeginEndBlockStatement be)
            {
                if (be.StatementList.Statements.Count == 1)
                {
                    node = be.StatementList.Statements[0];
                }
                else
                {
                    node = be.StatementList;
                }
            }

            return node;
        }

        private void DetectDups<T>(IList<T> flows)
        where T : TSqlFragment
        {
            if (flows is null || flows.Count < 2)
            {
                return;
            }

            string priorFlowCode = null;

            for (int i = flows.Count - 1; i >= 0; i--)
            {
                TSqlFragment flow = ExpandFragment(flows[i]);

                string flowCode = flow.GetFragmentCleanedText();
                if (priorFlowCode is null)
                {
                    priorFlowCode = flowCode;
                }
                else if (!string.Equals(priorFlowCode, flowCode, StringComparison.OrdinalIgnoreCase))
                {
                    // found something different - no violation
                    return;
                }
            }

            HandleNodeError(flows[0]);
        }
    }
}
