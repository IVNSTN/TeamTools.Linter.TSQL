using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines
{
    internal class RecursiveQuitBlockVisitor : TSqlFragmentVisitor
    {
        private const int MaxInfoSeverity = 10;
        private readonly QuitBlockParserState state;
        private readonly IDictionary<int, bool> checkedNodes;
        private readonly Action<TSqlFragment, TSqlTokenType> callback;
        private readonly bool conditionalEnter = false;
        private readonly bool insideTryBlock = false;

        public RecursiveQuitBlockVisitor(
            IDictionary<int, bool> checkedNodes,
            Action<TSqlFragment, TSqlTokenType> callback,
            QuitBlockParserState state,
            bool conditionalEnter = false,
            bool insideTryBlock = false)
        {
            this.state = state;
            this.conditionalEnter = conditionalEnter;
            this.insideTryBlock = insideTryBlock;
            this.checkedNodes = checkedNodes;
            this.callback = callback;
        }

        public override void Visit(IfStatement node)
        {
            GoRecursive(node.ThenStatement, true);
            GoRecursive(node.ElseStatement, true);
            // TODO : if Predicate is always false then NextIsUnreachable = true
        }

        public override void Visit(WhileStatement node)
        {
            GoRecursive(node, true);
        }

        public override void Visit(BeginEndBlockStatement node)
        {
            GoRecursive(node, conditionalEnter);
        }

        public override void Visit(TryCatchStatement node)
        {
            // TODO: from Try RETURN exits total batch, TROW and RAISERROR - only try block
            // try is always a conditional block - we reach to the end only if no error occured
            GoRecursive(node.TryStatements, true, true);
            // catch is always a conditional block - we enter it if error occured
            GoRecursive(node.CatchStatements, true, false);
        }

        public override void Visit(ThrowStatement node)
        {
            if (node.FirstTokenIndex <= state.LastCheckedTokenIndex)
            {
                return;
            }

            state.LastCheckedTokenIndex = node.LastTokenIndex;

            state.NextIsUnreachable = true;
            state.BreakCommandType = TSqlTokenType.Raiserror;
            state.AllNextAreUnreachable = !insideTryBlock && !conditionalEnter;
        }

        public override void Visit(ContinueStatement node)
        {
            if (node.FirstTokenIndex <= state.LastCheckedTokenIndex)
            {
                return;
            }

            state.LastCheckedTokenIndex = node.LastTokenIndex;

            state.NextIsUnreachable = true;
            state.BreakCommandType = TSqlTokenType.Continue;
        }

        public override void Visit(BreakStatement node)
        {
            if (node.FirstTokenIndex <= state.LastCheckedTokenIndex)
            {
                return;
            }

            state.LastCheckedTokenIndex = node.LastTokenIndex;

            state.NextIsUnreachable = true;
            state.BreakCommandType = TSqlTokenType.Break;
        }

        public override void Visit(RaiseErrorStatement node)
        {
            if (node.FirstTokenIndex <= state.LastCheckedTokenIndex)
            {
                return;
            }

            state.LastCheckedTokenIndex = node.LastTokenIndex;

            if (!((node.SecondParameter is IntegerLiteral litr) && int.TryParse(litr.Value, out int severity)))
            {
                severity = MaxInfoSeverity + 1;
            }

            state.NextIsUnreachable = insideTryBlock && (severity > MaxInfoSeverity);
            state.BreakCommandType = TSqlTokenType.Raiserror;
        }

        public override void Visit(ReturnStatement node)
        {
            if (node.FirstTokenIndex <= state.LastCheckedTokenIndex)
            {
                return;
            }

            state.LastCheckedTokenIndex = node.LastTokenIndex;

            state.NextIsUnreachable = true;
            state.BreakCommandType = TSqlTokenType.Return;
            state.AllNextAreUnreachable = !conditionalEnter;
        }

        public override void Visit(GoToStatement node)
        {
            if (node.FirstTokenIndex <= state.LastCheckedTokenIndex)
            {
                return;
            }

            state.LastCheckedTokenIndex = node.LastTokenIndex;

            state.NextIsUnreachable = true;
            state.BreakCommandType = TSqlTokenType.GoTo;
            state.AllNextAreUnreachable = !conditionalEnter;
        }

        public override void Visit(LabelStatement node)
        {
            if (node.FirstTokenIndex <= state.LastCheckedTokenIndex)
            {
                return;
            }

            state.LastCheckedTokenIndex = node.LastTokenIndex;

            // goto can get here no matter how
            state.NextIsUnreachable = false;
            state.BreakCommandType = TSqlTokenType.None;
            state.AllNextAreUnreachable = false;
        }

        public override void Visit(TSqlFragment node)
        {
            if (node.FirstTokenIndex <= state.LastCheckedTokenIndex)
            {
                return;
            }

            if (!state.NextIsUnreachable)
            {
                return;
            }

            if (node is LabelStatement)
            {
                // goto labels can be reached from anywhere
                return;
            }

            callback?.Invoke(node, state.BreakCommandType);

            // reporting only once
            state.NextIsUnreachable = false;
            state.BreakCommandType = TSqlTokenType.None;
            state.LastCheckedTokenIndex = node.LastTokenIndex;
        }

        protected void GoRecursive(TSqlFragment node, bool enteringConditionally, bool enteringTryBlock = false)
        {
            if (null == node)
            {
                return;
            }

            if (node.FirstTokenIndex <= state.LastCheckedTokenIndex)
            {
                return;
            }

            state.LastCheckedTokenIndex = node.FirstTokenIndex; // LastTokenIndex would kill recursion
            if (node is StatementList)
            {
                state.LastCheckedTokenIndex--; // to let the very first statement in the list be analyzed
            }

            var blockVisitor = new RecursiveQuitBlockVisitor(
                checkedNodes,
                callback,
                state,
                enteringConditionally,
                enteringTryBlock);

            if (node is StatementList)
            {
                node.AcceptChildren(blockVisitor);
            }
            else
            {
                node.Accept(blockVisitor);
            }

            if (node.LastTokenIndex > state.LastCheckedTokenIndex)
            {
                state.LastCheckedTokenIndex = node.LastTokenIndex;
            }

            // if we left conditional block then the rest of statements can be reached
            // when condition is false
            if ((enteringConditionally || enteringTryBlock) && !state.AllNextAreUnreachable)
            {
                state.NextIsUnreachable = false;
                state.BreakCommandType = TSqlTokenType.None;
            }
        }
    }
}
