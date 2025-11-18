using NUnit.Framework;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.SysFunctions;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("TeamTools.TSQL.ExpressionEvaluator.EvalFunctions")]
    [TestOf(typeof(DbId))]
    public sealed class DbIdTests : BaseMockFunctionTest
    {
        private DbId func;
        private SqlValue dbName;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new DbId();
            dbName = MakeStr("some_db");
        }

        [Test]
        public void Test_DbId_ReturnsNullOnNullArgs()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(Factory.NewNull(default)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True);
        }

        [Test]
        public void Test_DbId_ReturnsApproximateValue()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(dbName), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.False);
            Assert.That(res, Is.InstanceOf<SqlIntTypeValue>());
        }

        [Test]
        public void Test_DbId_ReturnsCurrentDbIdIfNoArgsProvided()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.False);
            Assert.That(res, Is.InstanceOf<CurrentDatabaseId>());
        }
    }
}
