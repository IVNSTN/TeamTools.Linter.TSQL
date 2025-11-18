using NUnit.Framework;
using System;
using System.Linq;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;
using TeamTools.TSQL.ExpressionEvaluator.Violations;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("TeamTools.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(OriginalStringManipulation))]
    public sealed class OriginalStringManipulationTests : BaseMockFunctionTest
    {
        private MockFunction func;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            func = new MockFunction();
        }

        [Test]
        public void Test_OriginalStringManipulation_FailsOnNullArgs()
        {
            Assert.Throws(typeof(ArgumentNullException), () => func.Evaluate(null, Context));
        }

        [Test]
        public void Test_OriginalStringManipulation_EvaluatesToExpectedValue()
        {
            var srcValue = new MockSqlValue("VARCHAR", SqlValueKind.Precise, new SqlValueSource(SqlValueSourceKind.Variable, null), TypeHandler)
            {
                StrValue = "asdf",
            };
            var args = ArgFactory.MakeListOfValues(srcValue);

            var res = func.Evaluate(args, Context);
            Assert.That(res, Is.Not.Null);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
            Assert.That(((SqlStrTypeValue)res).Value, Is.EqualTo("(asdf)"));

            // and no violation recorder
            Assert.That(Violations.ViolationCount, Is.EqualTo(0));
        }

        [Test]
        public void Test_OriginalStringManipulation_RecordsRedundantCallViolation()
        {
            var srcValue = new MockSqlValue("VARCHAR", SqlValueKind.Precise, new SqlValueSource(SqlValueSourceKind.Variable, null), TypeHandler)
            {
                StrValue = "redundant",
            };
            var args = ArgFactory.MakeListOfValues(srcValue);

            var res = func.Evaluate(args, Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
            Assert.That(((SqlStrTypeValue)res).Value, Is.EqualTo("redundant"));
            Assert.That(Violations.Violations.OfType<RedundantFunctionCallViolation>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void Test_OriginalStringManipulation_ReturnsNullOnNullInput()
        {
            var srcValue = new MockSqlValue("VARCHAR", SqlValueKind.Null, new SqlValueSource(SqlValueSourceKind.Variable, null), TypeHandler);
            var args = ArgFactory.MakeListOfValues(srcValue);

            var res = func.Evaluate(args, Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True);
            Assert.That(Violations.Violations.OfType<RedundantFunctionCallViolation>().Count(), Is.EqualTo(1));
        }

        private sealed class MockFunction : OriginalStringManipulation
        {
            public MockFunction() : base("MockFn", "redundancy warning")
            { }

            protected override string ModifyString(string originalValue)
            {
                if (originalValue == "redundant")
                {
                    return originalValue;
                }

                return "(" + originalValue + ")";
            }
        }
    }
}
