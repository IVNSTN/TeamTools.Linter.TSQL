using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    internal partial class UselessUnitRule
    {
        // No need in examples for each and every statement kind
        [ExcludeFromCodeCoverage]
        private sealed class BodyVisitor : TSqlFragmentVisitor
        {
            private const int MinFunctionCalls = 2;
            private int funcCallCount = 0;

            public BodyVisitor()
            { }

            public bool ContainsMeaningfulStatement { get; private set; }

            public override void Visit(TSqlFragment node)
            {
            }

            // Generic visitor for all kind of statements except
            // excluded by ExplicitVisit
            public override void Visit(TSqlStatement node)
            {
                ContainsMeaningfulStatement = true;
            }

            public override void Visit(ScalarSubquery node)
            {
                ContainsMeaningfulStatement = true;
            }

            // Let's say that CASE is complex enough
            public override void Visit(SearchedCaseExpression node)
            {
                if (ContainsMeaningfulStatement)
                {
                    return;
                }

                ContainsMeaningfulStatement = node.WhenClauses.Count > 1;
            }

            public override void Visit(SimpleCaseExpression node)
            {
                if (ContainsMeaningfulStatement)
                {
                    return;
                }

                ContainsMeaningfulStatement = node.WhenClauses.Count > 1;
            }

            // Complex expressions
            public override void Visit(IIfCall node)
            {
                ContainsMeaningfulStatement = true;
            }

            public override void Visit(BooleanBinaryExpression node)
            {
                ContainsMeaningfulStatement = true;
            }

            public override void Visit(FunctionCall node)
            {
                if (ContainsMeaningfulStatement)
                {
                    return;
                }

                ContainsMeaningfulStatement = ++funcCallCount >= MinFunctionCalls;
            }

            // Ignoring code blocks themselves but going deeper
            public override void ExplicitVisit(TryCatchStatement node) => node.AcceptChildren(this);

            public override void ExplicitVisit(BeginEndBlockStatement node) => node.AcceptChildren(this);

            public override void ExplicitVisit(WhileStatement node) => node.AcceptChildren(this);

            // Declare may contain scalar subqueries
            public override void ExplicitVisit(DeclareVariableStatement node) => node.AcceptChildren(this);

            // Ignoring statements which cannot be reasonable purpose for a unit to exist
            public override void ExplicitVisit(PredicateSetStatement node) { }

            public override void ExplicitVisit(SetIdentityInsertStatement node) { }

            public override void ExplicitVisit(SetOffsetsStatement node) { }

            public override void ExplicitVisit(SetStatisticsStatement node) { }

            public override void ExplicitVisit(SetCommandStatement node) { }

            public override void ExplicitVisit(PrintStatement node) { }

            public override void ExplicitVisit(ThrowStatement node) { }

            public override void ExplicitVisit(RaiseErrorStatement node) { }

            public override void ExplicitVisit(DeclareTableVariableStatement node) { }

            public override void ExplicitVisit(DeclareCursorStatement node) { }

            public override void ExplicitVisit(GoToStatement node) { }

            public override void ExplicitVisit(LabelStatement node) { }

            public override void ExplicitVisit(BeginTransactionStatement node) { }

            public override void ExplicitVisit(CommitTransactionStatement node) { }

            public override void ExplicitVisit(RollbackTransactionStatement node) { }

            public override void ExplicitVisit(SaveTransactionStatement node) { }

            // Return expression may be complex: function call, query
            public override void ExplicitVisit(ReturnStatement node) => node.Expression?.Accept(this);
        }
    }
}
