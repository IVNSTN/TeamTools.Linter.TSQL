using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0950", "FUNCTION_ENDS_WITH_RETURN")]
    internal sealed class FunctionEndsWithReturnRule : AbstractRule
    {
        public FunctionEndsWithReturnRule() : base()
        {
        }

        public override void Visit(FunctionStatementBody node)
        {
            if (node.ReturnType is SelectFunctionReturnType)
            {
                // inline functions do not have StatementList
                return;
            }

            if (node.StatementList is null)
            {
                // external functions do not have body
                return;
            }

            if (EndsWithReturn(node.StatementList.Statements))
            {
                return;
            }

            HandleNodeError(node.StatementList.Statements.Last());
        }

        private static TSqlStatement GetLastStatement(TSqlStatement stmt)
            => stmt is BeginEndBlockStatement block
                ? GetLastStatement(block.StatementList.Statements)
                : stmt;

        private static TSqlStatement GetLastStatement(IList<TSqlStatement> statements)
            => GetLastStatement(statements.Last());

        private static bool EndsWithReturn(IList<TSqlStatement> statements)
            => EndsWithReturn(GetLastStatement(statements));

        private static bool AllPathsEndWithReturn(IfStatement node)
            => node.ElseStatement != null
                && EndsWithReturn(GetLastStatement(node.ThenStatement))
                && EndsWithReturn(GetLastStatement(node.ElseStatement));

        private static bool EndsWithReturn(TSqlStatement statement)
        {
            if (statement is ReturnStatement)
            {
                return true;
            }

            if (statement is IfStatement conditional)
            {
                return AllPathsEndWithReturn(conditional);
            }

            return false;
        }
    }
}
