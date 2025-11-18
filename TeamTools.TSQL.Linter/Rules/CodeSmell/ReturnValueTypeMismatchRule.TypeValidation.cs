using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.TSQL.ExpressionEvaluator;
using TeamTools.TSQL.Linter.Properties;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    internal partial class ReturnValueTypeMismatchRule
    {
        private static bool AreCompatibleTypes(string expectedType, string returnedType)
        {
            return string.Equals(expectedType, returnedType, StringComparison.OrdinalIgnoreCase)
                || (ValueCanBeTreatedAs.TryGetValue(returnedType, out var compatibleTypes)
                    && compatibleTypes.Contains(expectedType));
        }

        private void ValidateReturnedType(string expectedType, ProcedureStatementBodyBase body)
        {
            if (string.IsNullOrEmpty(expectedType) || !sqlserverTypes.Contains(expectedType))
            {
                // could not determine type or type is unsupported
                return;
            }

            // Passing the whole programmability definition object
            // so parameters with they types can be detected
            // TODO : Migrate to ExpressionEvaluator once it supports all built-in types
            var ee = new ExpressionResultTypeEvaluator(body)
                .InjectKnownReturnTypes(knownFunctionReturnTypes);

            body.StatementList.AcceptChildren(new ReturnStatementDetector(returnedExpression => CheckExpressionTypeCompatibility(ee, (ScalarExpression)returnedExpression, expectedType)));
        }

        private void CheckExpressionTypeCompatibility(ExpressionResultTypeEvaluator evaluator, ScalarExpression returnedValue, string expectedValueType)
        {
            if (returnedValue is null)
            {
                return;
            }

            var returnedType = evaluator.GetExpressionType(returnedValue);

            if (string.IsNullOrEmpty(returnedType) || !sqlserverTypes.Contains(returnedType))
            {
                // could not determine type or type is unsupported
                return;
            }

            if (!AreCompatibleTypes(expectedValueType, returnedType))
            {
                // TODO : less string manufacturing
                string msg = string.Format(
                    Strings.ViolationDetails_ReturnValueTypeMismatchRule_UnexpectedReturnValueType,
                    expectedValueType.ToUpperInvariant(),
                    returnedType.ToUpperInvariant());

                HandleNodeError(returnedValue, msg);
            }
        }

        private sealed class ReturnStatementDetector : VisitorWithCallback
        {
            public ReturnStatementDetector(Action<TSqlFragment> callback) : base(callback)
            { }

            // If there are multiple RETURN statements we need to validate them all
            public override void Visit(ReturnStatement node)
            {
                if (node.Expression != null)
                {
                    Callback.Invoke(node.Expression);
                }
            }
        }
    }
}
