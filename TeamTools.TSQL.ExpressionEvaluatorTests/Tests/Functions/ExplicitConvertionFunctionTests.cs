using NUnit.Framework;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("TeamTools.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(ExplicitConvertionFunction))]
    public sealed class ExplicitConvertionFunctionTests : BaseMockFunctionTest
    {
        private MockFunction func;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            func = new MockFunction();
        }

        [Test]
        public void Test_ExplicitConvertionFunction_DoesNotFailOnBadArgs()
        {
            var srcValue = new ValueArgument(new MockSqlValue(new MockSqlTypeReference("foo", Factory, 1), SqlValueKind.Null, default, Factory.TypeHandler));
            var dstType = new TypeArgument(new MockSqlTypeReference("bar", Factory, 1));
            var dummyArg = new TypeArgument(new MockSqlTypeReference("far", Factory, 1));

            // empty args
            AssertNoResult(() => func.Evaluate(ArgFactory.MakeList(), Context));
            // extra arg
            AssertNoResult(() => func.Evaluate(ArgFactory.MakeList(srcValue, dstType, dummyArg), Context));
            // missing arg
            AssertNoResult(() => func.Evaluate(ArgFactory.MakeList(srcValue, dstType, dummyArg), Context));
            // wrong arg order
            AssertNoResult(() => func.Evaluate(ArgFactory.MakeList(dstType, srcValue), Context));
            // wrong arg type
            AssertNoResult(() => func.Evaluate(ArgFactory.MakeList(srcValue, srcValue), Context));
            // undefined type
            AssertNoResult(() => func.Evaluate(ArgFactory.MakeList(srcValue, new TypeArgument(null)), Context));
        }

        [Test]
        public void Test_ExplicitConvertionFunction_ReturnsNullValueForNullSource()
        {
            var srcValue = new ValueArgument(Factory.NewNull(null));
            var dstType = new TypeArgument(new MockSqlTypeReference("DUMMY", Factory, 1));

            var res = func.Evaluate(ArgFactory.MakeList(srcValue, dstType), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.TypeName, Is.EqualTo(dstType.TypeRef.TypeName));
            Assert.That(res.IsNull, Is.True);
        }

        [Test]
        public void Test_ExplicitConvertionFunction_ReturnsUnknownForMissingSource()
        {
            var srcValue = new ValueArgument(null);
            var dstType = new TypeArgument(new MockSqlTypeReference("DUMMY", Factory, 1));

            var res = func.Evaluate(ArgFactory.MakeList(srcValue, dstType), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.TypeName, Is.EqualTo(dstType.TypeRef.TypeName));
            Assert.That(res.ValueKind, Is.EqualTo(SqlValueKind.Unknown));
        }

        private class MockFunction : ExplicitConvertionFunction
        {
            public MockFunction() : base("MyFn")
            { }

            public MockFunction(string funcName) : base(funcName)
            { }
        }
    }
}
