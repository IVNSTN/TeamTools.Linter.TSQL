using NUnit.Framework;
using System;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator.TypeHandling
{
    [Category("Linter.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(SqlStrTypeReference))]
    public sealed class SqlStrTypeReferenceTests
    {
        private MockValueFactory factory;
        private int size;

        [SetUp]
        public void SetUp()
        {
            factory = new MockValueFactory();
            size = 4000;
        }

        [Test]
        public void Test_SqlStrTypeReference_ConstructorFailsIfTypeIsWrong()
        {
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => new SqlStrTypeReference("unknown type", size, factory));
        }

        [Test]
        public void Test_SqlStrTypeReference_ConstructorFailsIfTypeIsWrongForUnicode()
        {
            // non-unicode type
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => new SqlUnicodeStrTypeReference("dbo.VARCHAR", size, factory));
        }

        [Test]
        public void Test_SqlStrTypeReference_ConstructorFailsOnNullArgs()
        {
            Assert.Throws(typeof(ArgumentNullException), () => new SqlStrTypeReference(null, size, factory));
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => new SqlStrTypeReference("", size, factory));
            Assert.Throws(typeof(ArgumentNullException), () => new SqlStrTypeReference("dbo.SYSNAME", size, null));
        }

        [Test]
        public void Test_SqlStrTypeReference_ReturnsCorrectWeight()
        {
            var typeRef = new SqlStrTypeReference("dbo.VARCHAR", size, factory);
            Assert.That(typeRef.Bytes, Is.EqualTo(size * 1));

            typeRef = new SqlStrTypeReference("dbo.CHAR", size, factory);
            Assert.That(typeRef.Bytes, Is.EqualTo(size * 1));

            Assert.That(typeRef.IsUnicode, Is.False);
        }

        [Test]
        public void Test_SqlStrTypeReference_ReturnsCorrectWeightForUnicode()
        {
            var typeRef = new SqlUnicodeStrTypeReference("dbo.NVARCHAR", size, factory);
            Assert.That(typeRef.Bytes, Is.EqualTo(size * 2));

            typeRef = new SqlUnicodeStrTypeReference("dbo.NCHAR", size, factory);
            Assert.That(typeRef.Bytes, Is.EqualTo(size * 2));

            Assert.That(typeRef.IsUnicode, Is.True);
        }

        [Test]
        public void Test_SqlStrTypeReference_EqualsRespectsSize()
        {
            // same type name and size
            var a = new SqlStrTypeReference("dbo.VARCHAR", 10, factory);
            var b = new SqlStrTypeReference("dbo.VARCHAR", 10, factory);
            Assert.That(a.Equals(b), Is.True);
            Assert.That(b.Equals(a), Is.True);

            Assert.That(a.Equals(a), Is.True, "self comparison");
            Assert.That(b.Equals(b), Is.True, "self comparison");

            // different type name
            a = new SqlUnicodeStrTypeReference("dbo.NCHAR", 20, factory);
            b = new SqlStrTypeReference("dbo.VARCHAR", 20, factory);
            Assert.That(a.Equals(b), Is.False);
            Assert.That(b.Equals(a), Is.False);

            Assert.That(a.Equals(a), Is.True, "self comparison");
            Assert.That(b.Equals(b), Is.True, "self comparison");

            // different size
            a = new SqlStrTypeReference("dbo.VARCHAR", 10, factory);
            b = new SqlStrTypeReference("dbo.VARCHAR", 20, factory);
            Assert.That(a.Equals(b), Is.False);
            Assert.That(b.Equals(a), Is.False);

            Assert.That(a.Equals(a), Is.True, "self comparison");
            Assert.That(b.Equals(b), Is.True, "self comparison");
        }

        [Test]
        public void Test_SqlStrTypeReference_ToStringReturnsFullTypeDefinition()
        {
            var def = new SqlStrTypeReference("dbo.NVARCHAR", 17, factory);

            Assert.That(def.ToString(), Is.EqualTo("dbo.NVARCHAR(17)"));
        }
    }
}
