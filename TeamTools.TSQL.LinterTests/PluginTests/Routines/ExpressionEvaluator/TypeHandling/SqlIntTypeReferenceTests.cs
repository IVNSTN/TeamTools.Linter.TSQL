using NUnit.Framework;
using System;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator.TypeHandling
{
    [Category("Linter.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(SqlIntTypeReference))]
    public sealed class SqlIntTypeReferenceTests
    {
        private MockValueFactory factory;
        private SqlIntValueRange size;

        [SetUp]
        public void SetUp()
        {
            factory = new MockValueFactory();
            size = new SqlIntValueRange(-1, 1);
        }

        [Test]
        public void Test_SqlIntTypeReference_ConstructorFailsIfTypeIsWrong()
        {
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => new SqlIntTypeReference("unknown type", size, factory));
        }

        [Test]
        public void Test_SqlIntTypeReference_ConstructorFailsOnNullArgs()
        {
            Assert.Throws(typeof(ArgumentNullException), () => new SqlIntTypeReference(null, size, factory));
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => new SqlIntTypeReference("", size, factory));
            Assert.Throws(typeof(ArgumentNullException), () => new SqlIntTypeReference("dbo.INT", null, factory));
            Assert.Throws(typeof(ArgumentNullException), () => new SqlIntTypeReference("dbo.INT", size, null));
        }

        [Test]
        public void Test_SqlIntTypeReference_ReturnsCorrectWeight()
        {
            var typeRef = new SqlIntTypeReference("dbo.INT", size, factory);
            Assert.That(typeRef.Bytes, Is.EqualTo(4));

            typeRef = new SqlIntTypeReference("dbo.SMALLINT", size, factory);
            Assert.That(typeRef.Bytes, Is.EqualTo(2));

            typeRef = new SqlIntTypeReference("dbo.TINYINT", size, factory);
            Assert.That(typeRef.Bytes, Is.EqualTo(1));
        }

        [Test]
        public void Test_SqlIntTypeReference_EqualsIgnoresEstimateSize()
        {
#pragma warning disable NUnit2010, NUnit2045 // Use EqualConstraint for better assertion messages in case of failure
            // same type name
            var a = new SqlIntTypeReference("dbo.INT", new SqlIntValueRange(-1, 2), factory);
            var b = new SqlIntTypeReference("dbo.INT", new SqlIntValueRange(100, 200), factory);
            Assert.That(a.Equals(b), Is.True);
            Assert.That(b.Equals(a), Is.True);

            Assert.That(a.Equals(a), Is.True, "self comparison");
            Assert.That(b.Equals(b), Is.True, "self comparison");

            // different type name
            a = new SqlIntTypeReference("dbo.SMALLINT", new SqlIntValueRange(100, 200), factory);
            b = new SqlIntTypeReference("dbo.INT", new SqlIntValueRange(100, 200), factory);
            Assert.That(a.Equals(b), Is.False);
            Assert.That(b.Equals(a), Is.False);

            Assert.That(a.Equals(a), Is.True, "self comparison");
            Assert.That(b.Equals(b), Is.True, "self comparison");
#pragma warning restore NUnit2010, NUnit2045 // Use EqualConstraint for better assertion messages in case of failure
        }
    }
}
