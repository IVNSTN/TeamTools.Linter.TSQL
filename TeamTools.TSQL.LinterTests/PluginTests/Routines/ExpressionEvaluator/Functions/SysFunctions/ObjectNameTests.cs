using NUnit.Framework;
using System.Linq;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.SysFunctions;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("Linter.TSQL.ExpressionEvaluator")]
    [TestOfRule(typeof(ObjectName))]
    public sealed class ObjectNameTests : BaseObjNameTest
    {
        private ObjectName func;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new ObjectName();
        }

        [Test]
        public void Test_ObjectName_ReturnsNullOnNullArgs()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(Factory.NewNull(default)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True);
        }

        [Test]
        public void Test_ObjectName_ReturnsApproximateName()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(RandomObjId), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.False);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
        }

        [Test]
        public void Test_ObjectName_ReturnsPreciseValueForCurrentProc()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(CurrentProcId), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
            Assert.That((res as SqlStrTypeValue).Value, Is.EqualTo("myproc"));
        }

        [Test]
        public void Test_ObjectName_RegistersViolationForRedundantCurrentDbArg()
        {
            var currentDbId = new CurrentDatabaseId(new SqlIntTypeHandler(Context.Converter, Context.Violations), default);

            var res = func.Evaluate(ArgFactory.MakeListOfValues(RandomObjId, currentDbId), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.False);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());

            Assert.That(Violations.Violations.OfType<RedundantFunctionArgumentViolation>().Count(), Is.EqualTo(1));
        }
    }
}
