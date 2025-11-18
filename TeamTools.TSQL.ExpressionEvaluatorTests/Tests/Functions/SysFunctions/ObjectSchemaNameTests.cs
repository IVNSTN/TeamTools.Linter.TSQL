using NUnit.Framework;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.SysFunctions;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("TeamTools.TSQL.ExpressionEvaluator.EvalFunctions")]
    [TestOf(typeof(ObjectSchemaName))]
    public sealed class ObjectSchemaNameTests : BaseObjNameTest
    {
        private ObjectSchemaName func;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new ObjectSchemaName();
        }

        [Test]
        public void Test_ObjectSchemaName_ReturnsNullOnNullArgs()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(Factory.NewNull(default)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True);
        }

        [Test]
        public void Test_ObjectSchemaName_ReturnsApproximateName()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(RandomObjId), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.False);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
        }

        [Test]
        public void Test_ObjectSchemaName_ReturnsPreciseValueForCurrentProc()
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
