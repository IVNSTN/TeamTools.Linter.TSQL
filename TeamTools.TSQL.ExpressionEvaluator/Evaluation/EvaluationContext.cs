using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.Interfaces;
using TeamTools.TSQL.ExpressionEvaluator.Values;
using TeamTools.TSQL.ExpressionEvaluator.Violations;

namespace TeamTools.TSQL.ExpressionEvaluator.Evaluation
{
    [ExcludeFromCodeCoverage]
    public class EvaluationContext
    {
        public EvaluationContext(
            IExpressionEvaluator evaluator,
            ISqlTypeConverter converter,
            ISqlTypeResolver typeResolver,
            IVariableEvaluator variables,
            IViolationRegistrar violations,
            string functionName,
            TSqlFragment node)
        {
            Evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
            Converter = converter ?? throw new ArgumentNullException(nameof(converter));
            TypeResolver = typeResolver ?? throw new ArgumentNullException(nameof(typeResolver));
            Violations = violations ?? throw new ArgumentNullException(nameof(variables));
            Variables = variables ?? throw new ArgumentNullException(nameof(violations));
            FunctionName = functionName ?? throw new ArgumentNullException(nameof(functionName));
            // TODO : isn't node required as well?
            Node = node;

            NewSource = new SqlValueSource(SqlValueSourceKind.Expression, Node);
        }

        public IExpressionEvaluator Evaluator { get; }

        public ISqlTypeConverter Converter { get; }

        public ISqlTypeResolver TypeResolver { get; }

        public IViolationRegistrar Violations { get; }

        public IVariableEvaluator Variables { get; }

        public string FunctionName { get; }

        public TSqlFragment Node { get; }

        public SqlValueSource NewSource { get; }

        public void RedundantCall(string descr, SqlValueSource src = null)
            => Violations.RegisterViolation(new RedundantFunctionCallViolation(FunctionName, descr, NewSource));

        public void RedundantArgument(string argName, string descr)
            => Violations.RegisterViolation(new RedundantFunctionArgumentViolation(FunctionName, argName, descr, NewSource));

        public void InvalidArgument(string argName, string descr = "")
            => Violations.RegisterViolation(new InvalidArgumentViolation(FunctionName, argName, descr, NewSource));

        public void ArgumentOutOfRange(string argName, string descr = "")
            => Violations.RegisterViolation(new ArgumentOutOfRangeViolation(FunctionName, argName, descr, NewSource));

        public void ImplicitTruncation(int typeSize, int valueSize, string value)
            => Violations.RegisterViolation(new ImplicitTruncationViolation(typeSize, valueSize, value, NewSource));

        public void InvalidNumberOfArgs(int requiredArgCount, int actualArgCount)
            => Violations.RegisterViolation(new InvalidNumberOfArgumentsViolation(FunctionName, requiredArgCount, actualArgCount, NewSource));

        public void InvalidNumberOfArgs(int minArgCount, int maxArgCount, int actualArgCount)
        {
            if (minArgCount == maxArgCount)
            {
                InvalidNumberOfArgs(minArgCount, actualArgCount);
            }
            else
            {
                Violations.RegisterViolation(new InvalidNumberOfArgumentsViolation(FunctionName, minArgCount, maxArgCount, actualArgCount, NewSource));
            }
        }
    }
}
