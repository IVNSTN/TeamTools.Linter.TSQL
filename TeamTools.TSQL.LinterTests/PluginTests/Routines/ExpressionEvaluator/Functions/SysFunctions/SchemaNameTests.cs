using NUnit.Framework;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.SysFunctions;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("Linter.TSQL.ExpressionEvaluator")]
    [TestOfRule(typeof(SchemaName))]
    public sealed class SchemaNameTests : BaseObjNameTest
    {
        private SchemaName func;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new SchemaName();
        }

        [Test]
        public void Test_SchemaName_ReturnsNullOnNullArgs()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(Factory.NewNull(default)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True);
        }

        [Test]
        public void Test_SchemaName_ReturnsApproximateName()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(RandomObjId), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.False);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
        }

        [Test]
        public void Test_SchemaName_ReturnsPreciseValueForCurrentProc()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(CurrentProcId), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
            Assert.That((res as SqlStrTypeValue).Value, Is.EqualTo("myschema"));
        }
    }
}
