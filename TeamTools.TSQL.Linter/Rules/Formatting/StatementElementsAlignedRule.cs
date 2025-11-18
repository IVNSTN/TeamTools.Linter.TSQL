using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0219", "STATEMENT_ALIGN")]
    internal sealed class StatementElementsAlignedRule : AbstractRule
    {
        public StatementElementsAlignedRule() : base()
        {
        }

        public override void Visit(DeleteSpecification node)
        {
            if (node.StartLine == node.ScriptTokenStream[node.LastTokenIndex].Line)
            {
                // ignoring one-line statements
                return;
            }

            int statementOffset = node.StartColumn;

            CheckNodeOffset(statementOffset, node.FromClause);
            CheckNodeOffset(statementOffset, node.WhereClause);
        }

        public override void Visit(InsertSpecification node)
        {
            if (node.StartLine == node.ScriptTokenStream[node.LastTokenIndex].Line)
            {
                // ignoring one-line statements
                return;
            }

            int statementOffset = node.StartColumn;

            CheckNodeOffset(statementOffset, node.InsertSource);
        }

        public override void Visit(UpdateSpecification node)
        {
            if (node.StartLine == node.ScriptTokenStream[node.LastTokenIndex].Line)
            {
                // ignoring one-line statements
                return;
            }

            int statementOffset = node.StartColumn;

            CheckNodeOffset(statementOffset, node.FromClause);
            CheckNodeOffset(statementOffset, node.WhereClause);
        }

        public override void Visit(MergeSpecification node)
        {
            if (node.StartLine == node.ScriptTokenStream[node.LastTokenIndex].Line)
            {
                // ignoring one-line statements
                return;
            }

            int statementOffset = node.StartColumn;

            int usingIndex = node.TableReference.FirstTokenIndex;
            int start = node.Target.LastTokenIndex;

            while ((usingIndex > start) && !node.ScriptTokenStream[usingIndex].Text.Equals("USING", StringComparison.OrdinalIgnoreCase))
            {
                usingIndex--;
            }

            if (node.ScriptTokenStream[usingIndex].Text.Equals("USING", StringComparison.OrdinalIgnoreCase))
            {
                if (statementOffset != node.ScriptTokenStream[usingIndex].Column)
                {
                    HandleNodeError(node);
                }
            }

            for (int i = 0, n = node.ActionClauses.Count; i < n; i++)
            {
                var act = node.ActionClauses[i];
                int whenIndex = act.FirstTokenIndex;
                int startIndex = node.SearchCondition.LastTokenIndex;

                while ((whenIndex > startIndex) && (node.ScriptTokenStream[whenIndex].TokenType != TSqlTokenType.When))
                {
                    whenIndex--;
                }

                if (node.ScriptTokenStream[whenIndex].TokenType == TSqlTokenType.When
                && statementOffset != node.ScriptTokenStream[whenIndex].Column)
                {
                    HandleNodeError(node);
                }
            }
        }

        public override void Visit(QuerySpecification node)
        {
            if (node.StartLine == node.ScriptTokenStream[node.LastTokenIndex].Line)
            {
                // ignoring one-line statements
                return;
            }

            int statementOffset = node.StartColumn;

            CheckNodeOffset(statementOffset, node.FromClause);
            CheckNodeOffset(statementOffset, node.WhereClause);
            CheckNodeOffset(statementOffset, node.GroupByClause);
            CheckNodeOffset(statementOffset, node.HavingClause);
            CheckNodeOffset(statementOffset, node.OrderByClause);
        }

        private void CheckNodeOffset(int requiredOffset, TSqlFragment node)
        {
            if (node != null && requiredOffset != node.StartColumn)
            {
                HandleNodeError(node);
            }
        }
    }
}
