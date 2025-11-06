using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0284", "REDUNDANT_FUNCTION_CALL")]
    internal sealed class RedundantFunctionCallRule : AbstractRule
    {
        private static readonly Lazy<IDictionary<string, KeyValuePair<FuncArgScalar, int[]>>> MeaningfulFuncArgsInstance
            = new Lazy<IDictionary<string, KeyValuePair<FuncArgScalar, int[]>>>(() => InitMeaningfulFuncArgsInstance(), true);

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

        private static IDictionary<string, KeyValuePair<FuncArgScalar, int[]>> MeaningfulFuncArgs => MeaningfulFuncArgsInstance.Value;

        public override void Visit(TSqlBatch node)
        {
            var expressionEvaluator = new ScalarExpressionEvaluator(node);

            var violations = expressionEvaluator
                .Violations
                .OfType<RedundantFunctionCallViolation>()
                .Where(v => v.Source?.Node != null)
                .ToList();

            foreach (var v in violations)
            {
                HandleNodeError(v.Source.Node, v.Message);
            }
        }

        public override void Visit(FunctionCall node)
        {
            string funcName = node.FunctionName.Value;

            if (!MeaningfulFuncArgs.ContainsKey(funcName))
            {
                return;
            }

            FuncArgScalar meta = MeaningfulFuncArgs[funcName].Key;
            int[] args;

            if (MeaningfulFuncArgs[funcName].Value.Length == 1 && MeaningfulFuncArgs[funcName].Value[0] == -1)
            {
                args = Enumerable.Range(0, node.Parameters.Count).ToArray();
            }
            else
            {
                args = MeaningfulFuncArgs[funcName].Value;
            }

            if (args.Length == 0 || node.Parameters.Count == 0)
            {
                return;
            }

            int n = args.Length;

            for (int i = 0; i < n; i++)
            {
                if (args[i] >= 0 && args[i] < node.Parameters.Count)
                {
                    ValidateArgumentValue(node.Parameters[args[i]], meta, argNode =>
                        HandleNodeError(argNode, funcName));
                }
            }
        }

        private static IDictionary<string, KeyValuePair<FuncArgScalar, int[]>> InitMeaningfulFuncArgsInstance()
        {
            // TODO : consolidate all the metadata about known built-in functions
            return new SortedDictionary<string, KeyValuePair<FuncArgScalar, int[]>>(StringComparer.OrdinalIgnoreCase)
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

        private static KeyValuePair<FuncArgScalar, int[]> MakeFuncMeta(FuncArgScalar allowedElements, params int[] argPositions)
        {
            return new KeyValuePair<FuncArgScalar, int[]>(allowedElements, argPositions);
        }

        // TODO : refactoring
        private void ValidateArgumentValue(ScalarExpression param, FuncArgScalar allowedArgs, Action<TSqlFragment> callback)
        {
            var paramValue = DoGetParamValue(param);

            if (!allowedArgs.HasFlag(FuncArgScalar.AllowsNull) && paramValue is NullLiteral)
            {
                callback(paramValue);
                return;
            }

            if (!allowedArgs.HasFlag(FuncArgScalar.AllowsScalarVariable) && paramValue is VariableReference)
            {
                callback(paramValue);
                return;
            }

            if (!(paramValue is Literal ltrl))
            {
                return;
            }

            if (!allowedArgs.HasFlag(FuncArgScalar.AllowsLiteral))
            {
                callback(paramValue);
                return;
            }

            if (!allowedArgs.HasFlag(FuncArgScalar.AllowsEmptyString) && ltrl is StringLiteral str
                && str.Value == "")
            {
                callback(paramValue);
                return;
            }

            if (!allowedArgs.HasFlag(FuncArgScalar.AllowsZero) && ltrl is IntegerLiteral val
                && int.TryParse(val.Value, out int valVal) && valVal == 0)
            {
                callback(paramValue);
                return;
            }
        }
    }
}
