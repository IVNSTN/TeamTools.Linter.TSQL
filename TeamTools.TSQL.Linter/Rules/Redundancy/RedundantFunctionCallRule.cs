using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.ExpressionEvaluator;
using TeamTools.TSQL.ExpressionEvaluator.Violations;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0284", "REDUNDANT_FUNCTION_CALL")]
    internal sealed class RedundantFunctionCallRule : ScriptAnalysisServiceConsumingRule
    {
        private static readonly Lazy<Dictionary<string, Tuple<FuncArgScalar, int[]>>> MeaningfulFuncArgsInstance
            = new Lazy<Dictionary<string, Tuple<FuncArgScalar, int[]>>>(() => InitMeaningfulFuncArgsInstance(), true);

        public RedundantFunctionCallRule() : base()
        {
        }

        [Flags]
        private enum FuncArgScalar
        {
            NotAllowed = 0,
            AllowsNull = 1,
            AllowsZero = 2,
            AllowsEmptyString = 4,
            AllowsLiteral = 8,
            AllowsScalarVariable = 16,
            ScalarVarOrNotEmptyLiteral = AllowsScalarVariable | AllowsLiteral,
        }

        private static Dictionary<string, Tuple<FuncArgScalar, int[]>> MeaningfulFuncArgs => MeaningfulFuncArgsInstance.Value;

        // TODO : avoid double-visiting
        public override void Visit(FunctionCall node)
        {
            string funcName = node.FunctionName.Value;

            if (!MeaningfulFuncArgs.TryGetValue(funcName, out var funcArgs))
            {
                return;
            }

            FuncArgScalar meta = funcArgs.Item1;
            int[] args;

            if (funcArgs.Item2.Length == 1 && funcArgs.Item2[0] == -1)
            {
                args = Enumerable.Range(0, node.Parameters.Count).ToArray();
            }
            else
            {
                args = funcArgs.Item2;
            }

            if (args.Length == 0 || node.Parameters.Count == 0)
            {
                return;
            }

            int paramCount = node.Parameters.Count;
            int n = args.Length;
            for (int i = 0; i < n; i++)
            {
                var arg = args[i];
                if (arg >= 0 && arg < paramCount)
                {
                    ValidateArgumentValue(node.Parameters[arg], meta, funcName, ViolationHandlerWithMessage);
                }
            }
        }

        protected override void ValidateBatch(TSqlBatch node)
        {
            if (!ScalarExpressionEvaluator.IsBatchInteresting(node))
            {
                return;
            }

            var expressionEvaluator = GetService<ScalarExpressionEvaluator>(node);

            int n = expressionEvaluator.Violations.Count;
            for (int i = 0; i < n; i++)
            {
                var v = expressionEvaluator.Violations[i];
                if (v.Source?.Node != null
                && v is RedundantFunctionCallViolation)
                {
                    HandleNodeError(v.Source.Node, v.Message);
                }
            }

            // to catch partitioning info
            node.Accept(this);
        }

        private static Dictionary<string, Tuple<FuncArgScalar, int[]>> InitMeaningfulFuncArgsInstance()
        {
            // TODO : consolidate all the metadata about known built-in functions
            return new Dictionary<string, Tuple<FuncArgScalar, int[]>>(StringComparer.OrdinalIgnoreCase)
            {
                { "DATEADD", MakeFuncMeta(FuncArgScalar.ScalarVarOrNotEmptyLiteral, 1) },
                { "DATEFROMPARTS", MakeFuncMeta(FuncArgScalar.ScalarVarOrNotEmptyLiteral, -1) },
                { "DATETIMEFROMPARTS", MakeFuncMeta(FuncArgScalar.AllowsZero | FuncArgScalar.ScalarVarOrNotEmptyLiteral, -1) },
                { "SMALLDATETIMEFROMPARTS", MakeFuncMeta(FuncArgScalar.AllowsZero | FuncArgScalar.ScalarVarOrNotEmptyLiteral, -1) },
                { "TIMEFROMPARTS", MakeFuncMeta(FuncArgScalar.AllowsZero | FuncArgScalar.ScalarVarOrNotEmptyLiteral, -1) },
                { "ISDATE", MakeFuncMeta(FuncArgScalar.AllowsScalarVariable, 0) },
                { "AVG", MakeFuncMeta(FuncArgScalar.NotAllowed, 0) },
                { "MAX", MakeFuncMeta(FuncArgScalar.NotAllowed, 0) },
                { "MIN", MakeFuncMeta(FuncArgScalar.NotAllowed, 0) },
                { "SUM", MakeFuncMeta(FuncArgScalar.ScalarVarOrNotEmptyLiteral, 0) },
                { "PARSE", MakeFuncMeta(FuncArgScalar.ScalarVarOrNotEmptyLiteral, 0) },
                { "TRY_PARSE", MakeFuncMeta(FuncArgScalar.ScalarVarOrNotEmptyLiteral, 0) },
                { "CEILING", MakeFuncMeta(FuncArgScalar.AllowsScalarVariable, 0) },
                { "FLOOR", MakeFuncMeta(FuncArgScalar.AllowsScalarVariable, 0) },
                { "ROUND", MakeFuncMeta(FuncArgScalar.AllowsScalarVariable, 0) },
                { "PATINDEX", MakeFuncMeta(FuncArgScalar.ScalarVarOrNotEmptyLiteral, 0) },
                { "STRING_SPLIT", MakeFuncMeta(FuncArgScalar.ScalarVarOrNotEmptyLiteral, 0, 1) },
                { "OPENJSON", MakeFuncMeta(FuncArgScalar.ScalarVarOrNotEmptyLiteral, 0) },
                { "HASHBYTES", MakeFuncMeta(FuncArgScalar.ScalarVarOrNotEmptyLiteral, -1) },
            };
        }

        private static ScalarExpression DoGetParamValue(ScalarExpression expr)
        {
            while (expr is ParenthesisExpression p)
            {
                expr = p.Expression;
            }

            return expr;
        }

        private static Tuple<FuncArgScalar, int[]> MakeFuncMeta(FuncArgScalar allowedElements, params int[] argPositions)
        {
            return new Tuple<FuncArgScalar, int[]>(allowedElements, argPositions);
        }

        // TODO : refactoring
        private static void ValidateArgumentValue(ScalarExpression param, FuncArgScalar allowedArgs, string funcName, Action<TSqlFragment, string> callback)
        {
            var paramValue = DoGetParamValue(param);

            if (!allowedArgs.HasFlag(FuncArgScalar.AllowsNull) && paramValue is NullLiteral)
            {
                callback(paramValue, funcName);
                return;
            }

            if (!allowedArgs.HasFlag(FuncArgScalar.AllowsScalarVariable) && paramValue is VariableReference)
            {
                callback(paramValue, funcName);
                return;
            }

            if (!(paramValue is Literal ltrl))
            {
                return;
            }

            if (!allowedArgs.HasFlag(FuncArgScalar.AllowsLiteral))
            {
                callback(paramValue, funcName);
                return;
            }

            if (!allowedArgs.HasFlag(FuncArgScalar.AllowsEmptyString) && ltrl is StringLiteral str
                && string.IsNullOrEmpty(str.Value))
            {
                callback(paramValue, funcName);
                return;
            }

            if (!allowedArgs.HasFlag(FuncArgScalar.AllowsZero) && ltrl is IntegerLiteral val
                && int.TryParse(val.Value, out int valVal) && valVal == 0)
            {
                callback(paramValue, funcName);
                return;
            }
        }
    }
}
