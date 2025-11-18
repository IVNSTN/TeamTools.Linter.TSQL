using NUnit.Framework;
using System;
using System.Linq;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;
using TeamTools.TSQL.ExpressionEvaluator.Violations;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator.TypeHandling
{
    [Category("TeamTools.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(SqlTypeConverter))]
    public sealed class SqlTypeConverterTests : BaseSqlTypeHandlerTestClass
    {
        private MockValueFactory valueFactory;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            valueFactory = new MockValueFactory();
        }

        [Test]
        public void Test_SqlTypeConverter_ConstructorFailsOnNullArgs()
        {
            Assert.Throws(typeof(ArgumentNullException), () => new SqlTypeConverter(null));
        }

        [Test]
        public void Test_SqlTypeConverter_ImplicitlyConvert_DoesNotFailOnNull()
        {
            SqlTypeReference typeRef = new MockSqlTypeReference("VARCHAR", valueFactory, 1);
            SqlValue value = valueFactory.NewValue(typeRef, SqlValueKind.Unknown);

            // by type name
            Assert.DoesNotThrow(() => Converter.ImplicitlyConvertTo("INT", null));
            Assert.That(Converter.ImplicitlyConvertTo("INT", null), Is.Null);

            Assert.DoesNotThrow(() => Converter.ImplicitlyConvertTo("", value));
            Assert.That(Converter.ImplicitlyConvertTo("", value), Is.Null);

            // by type ref
            Assert.DoesNotThrow(() => Converter.ImplicitlyConvertTo(null as SqlTypeReference, value));
            Assert.That(Converter.ImplicitlyConvertTo(null as SqlTypeReference, value), Is.Null);

            Assert.DoesNotThrow(() => Converter.ImplicitlyConvertTo(typeRef, null));
            Assert.That(Converter.ImplicitlyConvertTo(typeRef, null), Is.Null);
        }

        [Test]
        public void Test_SqlTypeConverter_ImplicitlyConvert_ReturnsOriginalValueIfTypeEquals()
        {
            SqlTypeReference typeRef = new MockSqlTypeReference("NVARCHAR", valueFactory, 1);
            SqlValue value = valueFactory.NewValue(typeRef, SqlValueKind.Unknown);

            // by type name
            var convertedValue = Converter.ImplicitlyConvertTo("NVARCHAR", value);
            Assert.That(convertedValue, Is.Not.Null);
            Assert.That(value, Is.EqualTo(convertedValue));

            // by type ref
            convertedValue = Converter.ImplicitlyConvertTo(typeRef, value);
            Assert.That(convertedValue, Is.Not.Null);
            Assert.That(value, Is.EqualTo(convertedValue));
        }

        [Test]
        public void Test_SqlTypeConverter_ExplicitlyConvert_DoesNotFailOnNull()
        {
            SqlTypeReference typeRef = new MockSqlTypeReference("VARCHAR", valueFactory, 1);
            SqlValue value = valueFactory.NewValue(typeRef, SqlValueKind.Unknown);

            // by type ref
            Assert.DoesNotThrow(() => Converter.ExplicitlyConvertTo(null, value));
            Assert.That(Converter.ExplicitlyConvertTo(null, value), Is.Null);

            Assert.DoesNotThrow(() => Converter.ExplicitlyConvertTo(typeRef, null));
            Assert.That(Converter.ExplicitlyConvertTo(typeRef, null), Is.Null);
        }

        [Test]
        public void Test_SqlTypeConverter_ExplicitlyConvert_ReturnsOriginalValueIfTypeEquals()
        {
            SqlTypeReference typeRef = new MockSqlTypeReference("INT", valueFactory, 1);
            SqlValue value = valueFactory.NewValue(typeRef, SqlValueKind.Unknown);

            // by type ref
            var convertedValue = Converter.ExplicitlyConvertTo(typeRef, value);
            Assert.That(convertedValue, Is.Not.Null);
            Assert.That(value, Is.EqualTo(convertedValue));
        }

        [Test]
        public void Test_SqlTypeConverter_ExplicitlyConvert_RegistersViolationInCaseOfRedundantConversion()
        {
            Assert.That(Violations.ViolationCount, Is.EqualTo(0), "garbade detected");

            SqlTypeReference typeRef = new MockSqlTypeReference("INT", valueFactory, 1);
            SqlValue value = valueFactory.NewValue(typeRef, SqlValueKind.Unknown);

            // same type ref
            Converter.ExplicitlyConvertTo(typeRef, value, tp => Violations.RegisterViolation(new RedundantTypeConversionViolation(tp, default)));
            Assert.That(Violations.Violations.OfType<RedundantTypeConversionViolation>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void Test_SqlTypeConverter_ImplicitlyConvert_DoesNotRegistersViolationInCaseOfRedundantConversion()
        {
            Assert.That(Violations.ViolationCount, Is.EqualTo(0), "garbade detected");

            SqlTypeReference typeRef = new MockSqlTypeReference("INT", valueFactory, 1);
            SqlValue value = valueFactory.NewValue(typeRef, SqlValueKind.Unknown);

            // same type name
            Converter.ImplicitlyConvertTo("INT", value);
            Assert.That(Violations.ViolationCount, Is.EqualTo(0));

            // same type ref
            Converter.ImplicitlyConvertTo(typeRef, value);
            Assert.That(Violations.ViolationCount, Is.EqualTo(0));
        }
    }
}
