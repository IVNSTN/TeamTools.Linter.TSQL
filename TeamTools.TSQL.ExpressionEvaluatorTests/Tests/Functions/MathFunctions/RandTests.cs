using NUnit.Framework;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.MathFunctions;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("TeamTools.TSQL.ExpressionEvaluator.EvalFunctions")]
    [TestOf(typeof(Rand))]
    public sealed class RandTests : BaseMockFunctionTest
    {
        private Rand func;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new Rand();
        }

        [Test]
        public void Test_Rand_ReturnsPredefinedRange()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(Factory.NewNull(default)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.False);
            Assert.That(res, Is.InstanceOf<SqlIntTypeValue>());
            Assert.That((res as SqlIntTypeValue).EstimatedSize.Low, Is.EqualTo(0));
            Assert.That((res as SqlIntTypeValue).EstimatedSize.High, Is.EqualTo(1));
        }
    }
}
