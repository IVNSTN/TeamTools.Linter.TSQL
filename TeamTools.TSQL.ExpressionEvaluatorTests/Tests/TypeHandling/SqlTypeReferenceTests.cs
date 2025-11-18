using NUnit.Framework;
using System;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator.Values
{
    [Category("TeamTools.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(SqlTypeReference))]
    public sealed class SqlTypeReferenceTests
    {
        private MockValueFactory factory;

        [SetUp]
        public void SetUp()
        {
            factory = new MockValueFactory();
        }

        [Test]
        public void Test_SqlTypeReference_ConstructorFailsIfParamsNotProvided()
        {
            Assert.Throws(typeof(ArgumentNullException), () => new MockSqlTypeReference(null, factory, 123));
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => new MockSqlTypeReference("", factory, 123));
            Assert.Throws(typeof(ArgumentNullException), () => new MockSqlTypeReference("VARCHAR", null, 123));
        }

        [Test]
        public void Test_SqlTypeReference_MakeGoesToFactory()
        {
            var typeRef = new MockSqlTypeReference("VARCHAR", factory, 123);

            // no calls made before
            Assert.That(factory.NewCalls, Is.EqualTo(0));

            Assert.That(typeRef.MakeNullValue(), Is.Not.Null);
            Assert.That(typeRef.MakeUnknownValue(), Is.Not.Null);
            Assert.That(typeRef.MakeValue(SqlValueKind.Unknown), Is.Not.Null);

            Assert.That(factory.NewCalls, Is.EqualTo(3));
        }

        [Test]
        public void Test_SqlTypeReference_FactoryReturnsValueWithExactSameTypeRef()
        {
            var typeRef = new MockSqlTypeReference("INT", factory, 123);

            var value = typeRef.MakeNullValue();
            Assert.That(value, Is.Not.Null);
            Assert.That(value.TypeReference, Is.Not.Null);
            Assert.That(value.TypeReference, Is.InstanceOf<MockSqlTypeReference>());
            Assert.That(value.TypeReference.TypeName, Is.EqualTo("INT"));
            Assert.That((value.TypeReference as MockSqlTypeReference).Dummy, Is.EqualTo(123));
        }

        [Test]
        public void Test_SqlTypeReference_DoesNotAllowPreciseValueCreation()
        {
            var typeRef = new MockSqlTypeReference("VARCHAR", factory, 123);

            Assert.Throws(typeof(ArgumentOutOfRangeException), () => typeRef.MakeValue(SqlValueKind.Precise));
        }
    }
}
