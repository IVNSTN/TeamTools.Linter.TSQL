using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Linq;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// EXEC statement validation for ImplicitConversionImpossibleRule.
    /// </summary>
    internal partial class ImpossibleVisitor
    {
        // EXECUTE return value type is INT
        public override void Visit(ExecuteSpecification node)
        {
            if (node.Variable is null)
            {
                return;
            }

            ValidateCanConvertAtoB("INT", node.Variable);
        }

        // TODO : refactoring and optimization needed
        // sp_executesql parameters defined must be compatible with passed params
        // implementation is very similar to SpExecuteParamDefinitionMatchRule
        public override void Visit(ExecutableProcedureReference node)
        {
            var declaredParameters = SpExecuteParameterExtractor.ExtractDeclaredParameters(parser, node);
            if (declaredParameters is null || declaredParameters.Count == 0)
            {
                return;
            }

            // TODO : less linq
            // first two params are scipt itself and input param definition
            var passedArguments = node.Parameters.Skip(2).ToArray();
            bool namedCall = passedArguments.Any(p => p.Variable != null);
            // same number of declared and passed params is checked by separate rule
            int n = declaredParameters.Count > passedArguments.Length && !namedCall ? passedArguments.Length : declaredParameters.Count;

            for (int paramIndex = 0; paramIndex < n; paramIndex++)
            {
                var decl = declaredParameters[paramIndex];
                string paramType = typeEvaluator.GetExpressionType(decl);
                if (string.IsNullOrEmpty(paramType))
                {
                    continue;
                }

                ExecuteParameter arg;
                if (namedCall)
                {
                    string paramName = decl.VariableName.Value;
                    arg = passedArguments
                        .FirstOrDefault(p => p.Variable?.Name.Equals(paramName, StringComparison.OrdinalIgnoreCase) ?? false);
                }
                else
                {
                    arg = passedArguments[paramIndex];
                }

                if (arg is null || arg.ParameterValue is null)
                {
                    continue;
                }

                ValidateCanConvertAtoB(arg.ParameterValue, paramType);
            }
        }
    }
}
