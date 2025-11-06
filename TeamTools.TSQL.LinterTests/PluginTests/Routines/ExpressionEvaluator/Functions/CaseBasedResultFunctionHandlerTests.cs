using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("Linter.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(CaseBasedResultFunctionHandler))]
    public sealed class CaseBasedResultFunctionHandlerTests : BaseMockFunctionTest
    {
        private MockFunction func;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new MockFunction(Factory.TypeHandler);
        }

        [Test]
        public void Test_CaseBasedResultFunctionHandler_ConstructorFailsOnBadArgs()
        {
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => new MockFunction("MyFn", -1, 2));
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => new MockFunction("MyFn", 3, 2));
        }

        [Test]
        public void Test_CaseBasedResultFunctionHandler_EvaluateFailsOnNullContext()
        {
            Assert.Throws(typeof(ArgumentNullException), () => func.Evaluate(ArgFactory.MakeList(), null));
        }

        [Test]
        public void Test_CaseBasedResultFunctionHandler_EvaluateRecordsViolationOnInvalidArgNumber()
        {
            Assert.That(Violations.ViolationCount, Is.EqualTo(0), "garbage detected");

            var args = ArgFactory.MakeList();
            // zero args
            AssertNoResult(() => func.Evaluate(args, Context));
            Assert.That(Violations.Violations.OfType<InvalidNumberOfArgumentsViolation>().Count(), Is.EqualTo(1));

            // too many args (see MockFunctionDefinition)
            args = ArgFactory.MakeListOfValues(null, null, null, null, null);

            AssertNoResult(() => func.Evaluate(args, Context));
            Assert.That(Violations.Violations.OfType<InvalidNumberOfArgumentsViolation>().Count(), Is.EqualTo(2));
        }

        [Test]
        public void Test_CaseBasedResultFunctionHandler_EvaluateDoesNotFailOnNullArgs()
        {
            // null values
            var args = ArgFactory.MakeList(
                (ValueArgument)null,
                new ValueArgument(null),
                new ValueArgument(null));

            AssertNoResult(() => func.Evaluate(args, Context));
        }

        [Test]
        public void Test_CaseBasedResultFunctionHandler_DoesNotFailIfTypeIsUnsupported()
        {
            var args = ArgFactory.MakeListOfValues(
                new MockSqlValue(new MockSqlTypeReference("dbo.SMALLINT", Factory, 123), SqlValueKind.Unknown, default, Factory.TypeHandler),
                new MockSqlValue(new MockSqlTypeReference("dbo.TINYINT", Factory, 123), SqlValueKind.Unknown, default, Factory.TypeHandler),
                new MockSqlValue(new MockSqlTypeReference("dbo.INT", Factory, 123), SqlValueKind.Unknown, default, Factory.TypeHandler));

            func.EvaluatedTypeResult = null;
            AssertNoResult(() => func.Evaluate(args, Context));

            func.EvaluatedTypeResult = "";
            AssertNoResult(() => func.Evaluate(args, Context));

            // no handler for such type
            func.EvaluatedTypeResult = "foobar";
            AssertNoResult(() => func.Evaluate(args, Context));
        }

        [Test]
        public void Test_CaseBasedResultFunctionHandler_EvaluatesToUnknown()
        {
            // TODO : move to real classes
            var args = ArgFactory.MakeListOfValues(
                new MockSqlValue(new MockSqlTypeReference("dbo.SMALLINT", Factory, 123), SqlValueKind.Unknown, default, Factory.TypeHandler),
                new MockSqlValue(new MockSqlTypeReference("dbo.TINYINT", Factory, 123), SqlValueKind.Unknown, default, Factory.TypeHandler),
                new MockSqlValue(new MockSqlTypeReference("dbo.INT", Factory, 123), SqlValueKind.Unknown, default, Factory.TypeHandler));

            var res = func.Evaluate(args, Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsPreciseValue, Is.False);
        }

        private class MockFunction : CaseBasedResultFunctionHandler
        {
            private readonly ISqlTypeHandler typeHandler;

            public MockFunction(ISqlTypeHandler typeHandler) : base("MyFn", 1, 3)
            {
                this.typeHandler = typeHandler;
            }

            public MockFunction(string funcName, int minArgs, int maxArgs) : base(funcName, minArgs, maxArgs)
            { }

            public string EvaluatedTypeResult { get; set; } = "dbo.INT";

            protected override string DoEvaluateResultType(CallSignature<CaseArgs> args) => EvaluatedTypeResult;

            protected override SqlValue EvaluateValuesToSpecificResult(IList<SqlValue> values, EvaluationContext context) => default;
        }
    }
}
