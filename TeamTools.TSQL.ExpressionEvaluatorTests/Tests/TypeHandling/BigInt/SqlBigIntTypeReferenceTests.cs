using NUnit.Framework;
using System;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator.TypeHandling
{
    [Category("TeamTools.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(SqlBigIntTypeReference))]
    public sealed class SqlBigIntTypeReferenceTests
    {
        private MockValueFactory factory;
        private SqlBigIntValueRange size;

        [SetUp]
        public void SetUp()
        {
            factory = new MockValueFactory();
            size = new SqlBigIntValueRange(-1, 1);
        }

        [Test]
        public void Test_SqlBigIntTypeReference_ConstructorFailsIfTypeIsWrong()
        {
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => new SqlBigIntTypeReference("unknown type", size, factory));
        }

        [Test]
        public void Test_SqlBigIntTypeReference_ConstructorFailsOnNullArgs()
        {
            Assert.Throws(typeof(ArgumentNullException), () => new SqlBigIntTypeReference(null, size, factory));
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => new SqlBigIntTypeReference("", size, factory));
            Assert.Throws(typeof(ArgumentNullException), () => new SqlBigIntTypeReference("BIGINT", null, factory));
            Assert.Throws(typeof(ArgumentNullException), () => new SqlBigIntTypeReference("BIGINT", size, null));
        }

        [Test]
        public void Test_SqlBigIntTypeReference_ReturnsCorrectWeight()
        {
            var typeRef = new SqlBigIntTypeReference("BIGINT", size, factory);

            Assert.That(typeRef.Bytes, Is.EqualTo(8));
        }

        [Test]
        public void Test_SqlBigIntTypeReference_EqualsIgnoresEstimateSize()
        {
#pragma warning disable NUnit2010, NUnit2045
            // same type name
            var a = new SqlBigIntTypeReference("BIGINT", new SqlBigIntValueRange(-1, 2), factory);
            var b = new SqlBigIntTypeReference("BIGINT", new SqlBigIntValueRange(100, 200), factory);
            Assert.That(a.Equals(b), Is.True);
            Assert.That(b.Equals(a), Is.True);

            Assert.That(a.Equals(a), Is.True, "self comparison");
            Assert.That(b.Equals(b), Is.True, "self comparison");
#pragma warning restore NUnit2010, NUnit2045
        }
    }
}
