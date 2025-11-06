using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0141", "DATETIME2_SYSDATETIME")]
    internal sealed class Datetime2SysdatetimeRule : AbstractRule
    {
        private const string Datetime2Name = "DATETIME2";

        public Datetime2SysdatetimeRule() : base()
        {
        }

        public override void Visit(ColumnDefinition node) => DoValidate(node.DataType, node.DefaultConstraint);

        public override void Visit(DeclareVariableElement node) => DoValidate(node.DataType, node.Value);

        private static bool IsHandledDatatype(DataTypeReference dataType)
        {
            if (dataType?.Name is null)
            {
                return false;
            }

            return dataType.Name.BaseIdentifier.Value.Equals(Datetime2Name, StringComparison.OrdinalIgnoreCase);
        }

        private void DoValidate(DataTypeReference dataType, TSqlFragment node)
        {
            if (node is null || dataType?.Name is null || !IsHandledDatatype(dataType))
            {
                return;
            }

            TSqlViolationDetector.DetectFirst<GetdateVisitor>(node, HandleNodeError);
        }

        private class GetdateVisitor : TSqlViolationDetector
        {
            private const string GetdateFunctionName = "GETDATE";

            public override void Visit(PrimaryExpression node)
            {
                // some parser bug
                if (node.FirstTokenIndex < 0)
                {
                    return;
                }

                var token = node.ScriptTokenStream[node.FirstTokenIndex];

                if (token.TokenType == TSqlTokenType.Identifier)
                {
                    DetectFunctionCall(token.Text, node);
                }
            }

            public override void Visit(FunctionCall node) => DetectFunctionCall(node.FunctionName.Value, node);

            private void DetectFunctionCall(string call, TSqlFragment node)
            {
                if (call.Equals(GetdateFunctionName, StringComparison.OrdinalIgnoreCase))
                {
                    MarkDetected(node);
                }
            }
        }
    }
}
