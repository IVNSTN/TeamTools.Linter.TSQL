using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0254", "NESTED_BEGIN_END_LEFT_MARGIN")]
    internal class NestedBeginEndAlignLeftMarginRule : AbstractRule
    {
        public NestedBeginEndAlignLeftMarginRule() : base()
        {
        }

        public override void Visit(TSqlBatch node)
        {
            var beginEndAlignVisitor = new BeginEndAlignVisitor(HandleNodeError);
            node.Accept(beginEndAlignVisitor);
        }

        private class BeginEndAlignVisitor : TSqlFragmentVisitor
        {
            private readonly IList<TSqlFragment> processedBlocks = new List<TSqlFragment>();
            private readonly Action<TSqlFragment> callback;
            private int leftMargin = 0;

            public BeginEndAlignVisitor(Action<TSqlFragment> callback)
            {
                this.callback = callback;
            }

            public override void Visit(BeginEndBlockStatement node)
            {
                if (processedBlocks.TryAddUnique(node))
                {
                    ValidateNodeRecursive(node);
                }
            }

            public override void Visit(TryCatchStatement node)
            {
                if (!processedBlocks.TryAddUnique(node))
                {
                    return;
                }

                int beginToken = node.FirstTokenIndex;
                while (node.ScriptTokenStream[beginToken].TokenType != TSqlTokenType.Begin)
                {
                    beginToken++;
                }

                ValidateNodeRecursive(node.TryStatements, beginToken);

                if (node.CatchStatements.Statements.Count == 0)
                {
                    // empty catch block
                    return;
                }

                beginToken = node.CatchStatements.FirstTokenIndex;
                while (!node.ScriptTokenStream[beginToken].Text.Equals("CATCH", StringComparison.OrdinalIgnoreCase))
                {
                    beginToken--;
                }

                while (node.ScriptTokenStream[beginToken].TokenType != TSqlTokenType.Begin)
                {
                    beginToken--;
                }

                ValidateNodeRecursive(node.CatchStatements, beginToken);
            }

            protected void ValidateRecursive(TSqlFragment node, int beginToken, int endToken)
            {
                int beginColumn = node.ScriptTokenStream[beginToken].Column;
                int endColumn = node.ScriptTokenStream[endToken].Column;

                // for BEGIN TRY column of BEGIN sometimes gives the end of the BEGIN, not the start
                if (beginToken > 0)
                {
                    var priorTokenText = node.ScriptTokenStream[beginToken - 1].Text.Split(Environment.NewLine);
                    int priorTokenLastPos = (priorTokenText.Length > 1 ? 0 : node.ScriptTokenStream[beginToken - 1].Column)
                        + priorTokenText[priorTokenText.Length - 1].Length;

                    if (priorTokenLastPos != beginColumn)
                    {
                        beginColumn = priorTokenLastPos == 0 ? 1 : priorTokenLastPos;
                    }
                }

                if (beginColumn <= leftMargin)
                {
                    callback(node);
                }
                else if (endColumn <= leftMargin)
                {
                    callback(node);
                }
                else
                {
                    // recursively
                    int oldLeftMargin = leftMargin;
                    leftMargin = beginColumn;
                    node.AcceptChildren(this);
                    leftMargin = oldLeftMargin;
                }
            }

            protected void GetBeginEndPosition(TSqlFragment node, out int beginToken, out int endToken, bool insideOut = false)
            {
                beginToken = node.FirstTokenIndex;
                endToken = node.LastTokenIndex;
                int delta = insideOut ? -1 : +1;

                while (node.ScriptTokenStream[beginToken].TokenType != TSqlTokenType.Begin)
                {
                    beginToken += delta;
                }

                while (node.ScriptTokenStream[endToken].TokenType != TSqlTokenType.End)
                {
                    endToken -= delta;
                }
            }

            protected void ValidateNodeRecursive(TSqlFragment node)
            {
                GetBeginEndPosition(node, out int beginToken, out int endToken, !(node is BeginEndBlockStatement));
                ValidateRecursive(node, beginToken, endToken);
            }

            protected void ValidateNodeRecursive(TSqlFragment node, int beginToken)
            {
                GetBeginEndPosition(node, out int _, out int endToken, !(node is BeginEndBlockStatement));
                ValidateRecursive(node, beginToken, endToken);
            }
        }
    }
}
