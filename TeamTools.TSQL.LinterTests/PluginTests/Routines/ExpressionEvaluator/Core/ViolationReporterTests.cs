using NUnit.Framework;
using System;
using System.Linq;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator.TypeHandling
{
    [Category("Linter.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(ViolationReporter))]
    public sealed class ViolationReporterTests
    {
        private ViolationReporter reporter;

        [SetUp]
        public void SetUp()
        {
            reporter = new ViolationReporter();
        }

        [Test]
        public void Test_ViolationReporter_FailsToAddNull()
        {
            Assert.Throws(typeof(ArgumentNullException), () => reporter.RegisterViolation(null));
        }

        [Test]
        public void Test_ViolationReporter_RegistersGoodViolation()
        {
            reporter.RegisterViolation(new RedundantTypeConversionViolation("asfd", new SqlValueSource(SqlValueSourceKind.Expression, null)));

            Assert.That(reporter.ViolationCount, Is.EqualTo(1));
            Assert.That(reporter.Violations.OfType<RedundantTypeConversionViolation>().Count(), Is.EqualTo(1));
        }
    }
}
