using NUnit.Framework;
using System.Linq;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;
using TeamTools.TSQL.ExpressionEvaluator.Violations;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("TeamTools.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(OriginalStringPartHandler))]
    public sealed class OriginalStringPartHandlerTests : BaseMockFunctionTest
    {
        private MockFunction func;
        private MockSqlValue lenValue;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new MockFunction();

            lenValue = new MockSqlValue("INT", SqlValueKind.Precise, new SqlValueSource(SqlValueSourceKind.Literal, null), TypeHandler)
            {
                IntValue = 3,
            };
        }

        [Test]
        public void Test_OriginalStringPartHandler_DoesNotFailOnBadArgs()
        {
            AssertNoResult(() => func.Evaluate(ArgFactory.MakeList(), Context));
            // not enough args
            AssertNoResult(() => func.Evaluate(ArgFactory.MakeList(new ValueArgument(null)), Context));
            // bad arg type
            AssertNoResult(() => func.Evaluate(ArgFactory.MakeList(new TypeArgument(null), new TypeArgument(null)), Context));
            // both nulls
            AssertNoResult(() => func.Evaluate(ArgFactory.MakeListOfValues(null, null), Context));
        }

        [Test]
        public void Test_OriginalStringPartHandler_EvaluatesToExpectedValue()
        {
            var srcValue = new MockSqlValue("VARCHAR", SqlValueKind.Precise, new SqlValueSource(SqlValueSourceKind.Variable, null), TypeHandler)
            {
                StrValue = "ABCDEFGH",
            };

            var args = ArgFactory.MakeListOfValues(srcValue, lenValue);

            var res = func.Evaluate(args, Context);
            Assert.That(res, Is.Not.Null);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
            Assert.That(((SqlStrTypeValue)res).Value, Is.EqualTo("ABC"));

            // and no violation recorder
            Assert.That(Violations.ViolationCount, Is.EqualTo(0));
        }

        [Test]
        public void Test_OriginalStringPartHandler_RecordsRedundantCallViolation()
        {
            var srcValue = new MockSqlValue("VARCHAR", SqlValueKind.Precise, new SqlValueSource(SqlValueSourceKind.Variable, null), TypeHandler)
            {
                StrValue = "redundant",
            };

            var args = ArgFactory.MakeListOfValues(srcValue, lenValue);

            var res = func.Evaluate(args, Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
            Assert.That(((SqlStrTypeValue)res).Value, Is.EqualTo("redundant"));
            Assert.That(Violations.Violations.OfType<RedundantFunctionCallViolation>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void Test_OriginalStringPartHandler_RecordsRedundantCallForShorterString()
        {
            var srcValue = new MockSqlValue("VARCHAR", SqlValueKind.Precise, new SqlValueSource(SqlValueSourceKind.Variable, null), TypeHandler)
            {
                StrValue = "AB", // 'AB' length 2 is less than 3
            };

            var args = ArgFactory.MakeListOfValues(srcValue, lenValue);

            var res = func.Evaluate(args, Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
            Assert.That(((SqlStrTypeValue)res).Value, Is.EqualTo("AB"));
            Assert.That(Violations.Violations.OfType<RedundantFunctionCallViolation>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void Test_OriginalStringPartHandler_RecordsRedundantCallForStringOfTargetSize()
        {
            var srcValue = new MockSqlValue("VARCHAR", SqlValueKind.Precise, new SqlValueSource(SqlValueSourceKind.Variable, null), TypeHandler)
            {
                StrValue = "ABC", // 'ABC' length 3 === 3
            };

            var args = ArgFactory.MakeListOfValues(srcValue, lenValue);

            var res = func.Evaluate(args, Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
            Assert.That(((SqlStrTypeValue)res).Value, Is.EqualTo("ABC"));
            Assert.That(Violations.Violations.OfType<RedundantFunctionCallViolation>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void Test_OriginalStringPartHandler_ReturnsNullOnNullInput()
        {
            var srcValue = new MockSqlValue("VARCHAR", SqlValueKind.Null, new SqlValueSource(SqlValueSourceKind.Variable, null), TypeHandler);
            var args = ArgFactory.MakeListOfValues(srcValue, lenValue);

            var res = func.Evaluate(args, Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True);
            Assert.That(Violations.Violations.OfType<RedundantFunctionCallViolation>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void Test_OriginalStringPartHandler_ReturnsEmptyStringOnEmptyInput()
        {
            var srcValue = new MockSqlValue("VARCHAR", SqlValueKind.Precise, new SqlValueSource(SqlValueSourceKind.Variable, null), TypeHandler)
            {
                StrValue = "",
            };
            var args = ArgFactory.MakeListOfValues(srcValue, lenValue);

            var res = func.Evaluate(args, Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
            Assert.That((res as SqlStrTypeValue).Value, Is.EqualTo(""));
            Assert.That(Violations.Violations.OfType<RedundantFunctionCallViolation>().Count(), Is.EqualTo(1));
        }

        private sealed class MockFunction : OriginalStringPartHandler
        {
            public MockFunction() : base("MockFn", 2)
            { }

            protected override string TakeStringPartFrom(string originalValue, int startValue, int lenValue)
            {
                if (originalValue == "redundant")
                {
                    return originalValue;
                }

                return originalValue.Substring(0, lenValue);
            }
        }
    }
}
