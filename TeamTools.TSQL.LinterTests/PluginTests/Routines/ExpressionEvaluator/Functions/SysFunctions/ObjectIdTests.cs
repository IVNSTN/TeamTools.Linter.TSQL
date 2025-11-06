using NUnit.Framework;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.SysFunctions;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("Linter.TSQL.ExpressionEvaluator")]
    [TestOfRule(typeof(ObjectId))]
    public sealed class ObjectIdTests : BaseMockFunctionTest
    {
        private ObjectId func;
        private SqlValue objectName;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new ObjectId();
            objectName = MakeStr("dbo.my_table");
        }

        [Test]
        public void Test_ObjectId_ReturnsNullOnNullArgs()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(Factory.NewNull(default), MakeStr("U")), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True, "null name");

            res = func.Evaluate(ArgFactory.MakeListOfValues(objectName, Factory.NewNull(default)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True, "null type");
        }

        [Test]
        public void Test_ObjectId_ReturnsUnknownValue()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(objectName, MakeStr("U")), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.False);
            Assert.That(res, Is.InstanceOf<SqlIntTypeValue>());
        }
    }
}
