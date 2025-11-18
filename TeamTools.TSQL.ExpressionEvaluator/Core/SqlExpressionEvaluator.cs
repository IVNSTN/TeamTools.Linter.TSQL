using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.TSQL.ExpressionEvaluator.Evaluation;
using TeamTools.TSQL.ExpressionEvaluator.Interfaces;
using TeamTools.TSQL.ExpressionEvaluator.Interfaces.OperatorHandlers;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Core
{
    public class SqlExpressionEvaluator : IExpressionEvaluator
    {
        private readonly IVariableEvaluator varEval;
        private readonly ISqlFunctionRegistry funcReg;
        private readonly ISqlTypeConverter converter;
        private readonly ISqlTypeResolver typeResolver;
        private readonly ILiteralValueFactory literalFactory;
        private readonly IConditionalFlowHandler conditionHander;
        private readonly IViolationRegistrar violations;

        public SqlExpressionEvaluator(
            IVariableEvaluator variableEvaluator,
            ISqlFunctionRegistry functionRegistry,
            ISqlTypeConverter converter,
            ISqlTypeResolver typeResolver,
            ILiteralValueFactory literalFactory,
            IViolationRegistrar violations,
            Func<SqlExpressionEvaluator, IConditionalFlowHandler> makeConditionHandler)
        {
            varEval = variableEvaluator ?? throw new ArgumentNullException(nameof(variableEvaluator));
            funcReg = functionRegistry ?? throw new ArgumentNullException(nameof(functionRegistry));
            this.converter = converter ?? throw new ArgumentNullException(nameof(converter));
            this.typeResolver = typeResolver ?? throw new ArgumentNullException(nameof(typeResolver));
            this.literalFactory = literalFactory ?? throw new ArgumentNullException(nameof(literalFactory));
            this.violations = violations ?? throw new ArgumentNullException(nameof(violations));

            conditionHander = makeConditionHandler?.Invoke(this);
        }

        public IVariableEvaluator VariableEvaluator => varEval;

        public ISqlFunctionRegistry FunctionRegistry => funcReg;

        public ISqlTypeConverter Converter => converter;

        public ISqlTypeResolver TypeResolver => typeResolver;

        public ILiteralValueFactory LiteralValueFactory => literalFactory;

        public IConditionalFlowHandler ConditionHander => conditionHander;

        public IViolationRegistrar Violations => violations;

        public SqlValue EvaluateExpression(ScalarExpression expr)
        {
            while (expr is ParenthesisExpression pe)
            {
                expr = pe.Expression;
            }

            if (expr is UnaryExpression un)
            {
                if (un.UnaryExpressionType == UnaryExpressionType.BitwiseNot)
                {
                    // what would negation mean in non-boolean expression?
                    return default;
                }

                var ev = EvaluateExpression(un.Expression);
                if (ev is null)
                {
                    return default;
                }

                // TODO : support redundant multiple unary expressions e.g. --1,  + + ''
                if (un.UnaryExpressionType == UnaryExpressionType.Positive)
                {
                    // positive number is just a number
                    return ev;
                }
                else if (ev.GetTypeHandler() is IReverseValueSignHandler rs)
                {
                    return rs.ReverseSign(ev);
                }
                else
                {
                    // type handler does not support "-"
                    return default;
                }
            }

            if (expr is Literal l)
            {
                return this.EvaluateLiteral(l);
            }

            if (expr is VariableReference vr)
            {
                return this.EvaluateVariable(vr);
            }

            if (expr is ScalarSubquery q)
            {
                return this.EvaluateScalarSubquery(q);
            }

            if (expr is BinaryExpression bin)
            {
                return this.EvaluateFormula(bin);
            }

            if (expr is FunctionCall fn)
            {
                return this.EvaluateFunctionResult(fn.FunctionName.Value, this.ToArgs(fn.Parameters), fn);
            }

            if (expr is GlobalVariableExpression gv)
            {
                return this.EvaluateFunctionResult(gv.Name, this.ToArgs((List<ScalarExpression>)null), gv);
            }

            if (expr is SearchedCaseExpression scs)
            {
                return this.EvaluateCaseExpression(scs);
            }

            if (expr is SimpleCaseExpression sms)
            {
                return this.EvaluateCaseExpression(sms);
            }

            if (expr is PrimaryExpression prex)
            {
                return this.EvaluateBuiltInExpression(prex);
            }

            return default;
        }

        public SqlValue EvaluateVariableModification(string varName, ScalarExpression expr, AssignmentKind operKind)
        {
            return this.EvaluateVarModification(varName, expr, operKind);
        }
    }
}
