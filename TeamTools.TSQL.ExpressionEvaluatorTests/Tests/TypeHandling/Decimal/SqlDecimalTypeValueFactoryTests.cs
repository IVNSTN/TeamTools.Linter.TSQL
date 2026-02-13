using NUnit.Framework;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;
using TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.ExpressionEvaluatorTests.Tests.TypeHandling.Decimal
{
    [Category("TeamTools.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(SqlDecimalTypeValueFactory))]
    public sealed class SqlDecimalTypeValueFactoryTests : BaseSqlTypeHandlerTestClass
    {
        private SqlDecimalTypeHandler typeHandler;

        private SqlDecimalTypeValueFactory ValueFactory => typeHandler.DecimalValueFactory;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            typeHandler = new SqlDecimalTypeHandler(Converter, Violations);
        }

        [Test]
        public void Test_SqlDecimalTypeValue_DeepCloneCopiesPreciseValue()
        {
            var value = ValueFactory.MakePreciseValue("DECIMAL", 123.4567m, new SqlValueSource(SqlValueSourceKind.Literal, null));
            Assert.That(value, Is.Not.Null);
            Assert.That(value.Value, Is.EqualTo(123.4567m));

            var clone = value.DeepClone();
            Assert.That(clone, Is.Not.Null);
            Assert.That(clone.IsPreciseValue, Is.True);
            Assert.That(clone.Value, Is.EqualTo(value.Value));
            Assert.That(clone.TypeName, Is.EqualTo("DECIMAL"));
        }

        [Test]
        public void Test_SqlDecimalTypeValueFactory_NewLiteral()
        {
            var value = ValueFactory.NewLiteral("DECIMAL", "123.4567", default);

            Assert.That(value, Is.Not.Null);
            Assert.That(value.TypeName, Is.EqualTo("DECIMAL"));
        }

        [Test]
        public void Test_SqlDecimalTypeValueFactory_MakeMethodsDontFailForUnsupportedType()
        {
            CallMakeMethodsWith("dummy");
            CallMakeMethodsWith("");
            CallMakeMethodsWith(null);
        }

        [Test]
        public void Test_SqlDecimalTypeValueFactory_MakeReturnsUnknownForEmptyString()
        {
            Assert.DoesNotThrow(() => ValueFactory.NewLiteral("DECIMAL", "", null));
            var value = ValueFactory.NewLiteral("DECIMAL", "", null);

            Assert.That(value, Is.Not.Null);
            Assert.That(value.ValueKind, Is.EqualTo(SqlValueKind.Unknown));
        }

        [Test]
        public void Test_SqlDecimalTypeValueFactory_MakeNull()
        {
            var value = ValueFactory.MakeNullValue("DECIMAL", null);
            Assert.That(value, Is.Not.Null);
            Assert.That(value.IsNull, Is.True);

            var v2 = ValueFactory.NewNull(null);
            Assert.That(v2, Is.Not.Null);
            Assert.That(v2.IsNull, Is.True);
        }

        private void CallMakeMethodsWith(string dummyType)
        {
            Assert.DoesNotThrow(() => ValueFactory.MakeApproximateValue(dummyType, default, default));
            Assert.That(ValueFactory.MakeApproximateValue(dummyType, default, default), Is.Null);

            Assert.DoesNotThrow(() => ValueFactory.MakeLiteral(dummyType, default, default));
            Assert.That(ValueFactory.MakeLiteral(dummyType, default, default), Is.Null);

            Assert.DoesNotThrow(() => ValueFactory.MakeNullValue(dummyType, default));
            Assert.That(ValueFactory.MakeNullValue(dummyType, default), Is.Null);

            Assert.DoesNotThrow(() => ValueFactory.MakePreciseValue(dummyType, default, default));
            Assert.That(ValueFactory.MakePreciseValue(dummyType, default, default), Is.Null);

            Assert.DoesNotThrow(() => ValueFactory.MakeUnknownValue(dummyType));
            Assert.That(ValueFactory.MakeUnknownValue(dummyType), Is.Null);

            Assert.DoesNotThrow(() => ValueFactory.MakeSqlDataTypeReference(dummyType, 0));
            Assert.That(ValueFactory.MakeSqlDataTypeReference(dummyType, 1), Is.Null);

            Assert.DoesNotThrow(() => ValueFactory.NewLiteral(dummyType, "123", default));
            Assert.That(ValueFactory.NewLiteral(dummyType, "123", default), Is.Null);
        }
    }
}
