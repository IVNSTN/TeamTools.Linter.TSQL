using Microsoft.SqlServer.TransactSql.ScriptDom;
using NUnit.Framework;
using System;
using TeamTools.TSQL.ExpressionEvaluator.Core;
using TeamTools.TSQL.ExpressionEvaluator.Evaluation;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator.BuiltInFunctions
{
    public abstract class BaseArgValidationTest
    {
        protected SqlValue StrValue { get; private set; }

        protected SqlValue IntValue { get; private set; }

        protected EvaluationContext Context { get; private set; }

        protected ViolationReporter Violations { get; private set; }

        public virtual void SetUp()
        {
            var typeResolver = new SqlTypeResolver();
            var converter = new SqlTypeConverter(typeResolver);
            Violations = new ViolationReporter();
            var varReg = new SqlVariableRegistry(converter, Violations);
            var funcReg = new SqlFunctionRegistry();
            var literals = new SqlLiteralValueFactory(typeResolver);

            var strHandler = new SqlStrTypeHandler(converter, Violations);
            var intHandler = new SqlIntTypeHandler(converter, Violations);

            typeResolver.RegisterTypeHandler(strHandler);
            typeResolver.RegisterTypeHandler(intHandler);

            Context = new EvaluationContext(
                new SqlExpressionEvaluator(varReg, funcReg, converter, typeResolver, literals, Violations, null),
                converter,
                typeResolver,
                varReg,
                Violations,
                "MyFn",
                new FunctionCall() { FunctionName = new Identifier() { Value = "MyFn" } });

            StrValue = strHandler.StrValueFactory.MakePreciseValue("NVARCHAR", "123", new SqlValueSource(SqlValueSourceKind.Variable, null));
            IntValue = intHandler.IntValueFactory.MakePreciseValue("TINYINT", 123, new SqlValueSource(SqlValueSourceKind.Expression, null));
        }

        protected static void AssertNotValid(Func<bool> call)
        {
            Assert.DoesNotThrow(() => call.Invoke());
            Assert.That(call.Invoke(), Is.False);
        }
    }
}
