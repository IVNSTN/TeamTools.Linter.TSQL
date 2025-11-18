using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Variable assignment validation for ImplicitConversionImpossibleRule.
    /// </summary>
    internal partial class ImpossibleVisitor
    {
        // Variable default value must be compatible with variable type
        public override void Visit(DeclareVariableElement node)
        {
            if (node.Value is null)
            {
                return;
            }

            ValidateCanConvertAtoB(node.Value, typeEvaluator.GetExpressionType(node));
        }

        // Checking if we can assign expression result type to variable of it's own type
        public override void Visit(SetVariableStatement node)
        {
            var variableType = typeEvaluator.GetExpressionType(node.Variable);
            if (string.IsNullOrEmpty(variableType))
            {
                return;
            }

            if (node.Expression is null)
            {
                ValidateCanConvertAtoB(node.Variable, variableType);
            }
            else
            {
                ValidateCanConvertAtoB(node.Expression, variableType);
            }
        }

        // Checking if we can assign expression result type to variable of it's own type
        public override void Visit(SelectSetVariable node)
        {
            var variableType = typeEvaluator.GetExpressionType(node.Variable);
            if (string.IsNullOrEmpty(variableType))
            {
                return;
            }

            ValidateCanConvertAtoB(node.Expression, variableType);
        }
    }
}
