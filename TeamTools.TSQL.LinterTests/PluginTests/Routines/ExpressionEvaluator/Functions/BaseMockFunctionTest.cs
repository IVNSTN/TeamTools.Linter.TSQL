using Microsoft.SqlServer.TransactSql.ScriptDom;
using NUnit.Framework;
using System;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    public abstract class BaseMockFunctionTest
    {
        protected MockTypeHandler TypeHandler { get; private set; }

        protected MockValueFactory Factory { get; private set; }

        protected ViolationReporter Violations { get; private set; }

        protected EvaluationContext Context { get; private set; }

        public virtual void SetUp()
        {
            Violations = new ViolationReporter();

            var typeResolver = new SqlTypeResolver();
            Factory = new MockValueFactory();
            TypeHandler = new MockTypeHandler(Factory);
            Factory.TypeHandler = TypeHandler;
            var converter = new MockSqlTypeConverter(TypeHandler, typeResolver);

            typeResolver.RegisterTypeHandler(TypeHandler);

            var magicValue = new MockSqlValue("dbo.INT", SqlValueKind.Precise, new SqlValueSource(SqlValueSourceKind.Literal, null), Factory.TypeHandler);

            Context = new EvaluationContext(
                new MockEvaluator(magicValue),
                converter,
                typeResolver,
                new SqlVariableRegistry(converter, Violations),
                Violations,
                "MyFn",
                new SetVariableStatement { FirstTokenIndex = 123, LastTokenIndex = 125 });
        }

        protected static void AssertNoResult(Func<object> call)
        {
            object res = string.Empty; // dummy initialization

            Assert.DoesNotThrow(() => res = call.Invoke());
            Assert.That(res, Is.Null);
        }

        protected SqlStrTypeValue MakeStr(string value)
        {
            return Context.Converter.ImplicitlyConvert<SqlStrTypeValue>(
                Factory.NewLiteral("dbo.VARCHAR", value, default));
        }

        protected SqlIntTypeValue MakeInt(int value)
        {
            return Context.Converter.ImplicitlyConvert<SqlIntTypeValue>(
                Factory.NewLiteral("dbo.INT", value.ToString(), default));
        }
    }
}
