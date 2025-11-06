using NUnit.Framework;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("Linter.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(SqlZeroArgFunctionHandler))]
    public sealed class SqlZeroArgFunctionHandlerTests : BaseMockFunctionTest
    {
        private MockFunction func;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new MockFunction();
        }

        [Test]
        public void Test_SqlZeroArgFunctionHandler_AllowsOnlyZeroArgs()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues((SqlValue)null), Context);

            Assert.That(res, Is.Null);
        }

        [Test]
        public void Test_SqlZeroArgFunctionHandler_EvaluatesPredefinedResultType()
        {
            var res = func.Evaluate(ArgFactory.MakeList(), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.TypeName, Is.EqualTo("DUMMY"));
        }

        [Test]
        public void Test_SqlZeroArgFunctionHandler_EvaluatesToApproximateValue()
        {
            var res = func.Evaluate(ArgFactory.MakeList(), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsPreciseValue, Is.False);
        }

        private sealed class MockFunction : SqlZeroArgFunctionHandler
        {
            public MockFunction() : base("MyFn", "DUMMY")
            { }
        }
    }
}
