using NUnit.Framework;
using System;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;
using TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.ExpressionEvaluatorTests.Tests.TypeHandling.Time
{
    [Category("TeamTools.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(SqlTimeTypeValueFactory))]
    internal class SqlTimeTypeValueFactoryTests : BaseSqlTypeHandlerTestClass
    {
        private SqlTimeTypeHandler typeHandler;

        private SqlTimeTypeValueFactory ValueFactory => typeHandler.TimeValueFactory;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            typeHandler = new SqlTimeTypeHandler(Converter, Violations);
        }

        [Test]
        public void Test_SqlTimeTypeValue_DeepCloneCopiesPreciseValue()
        {
            var value = ValueFactory.MakePreciseValue("TIME", new TimeSpan(12, 30, 55), new SqlValueSource(SqlValueSourceKind.Literal, null));
            Assert.That(value, Is.Not.Null);
            Assert.That(value.Value, Is.EqualTo(new TimeSpan(12, 30, 55)));

            var clone = value.DeepClone();
            Assert.That(clone, Is.Not.Null);
            Assert.That(clone.IsPreciseValue, Is.True);
            Assert.That(clone.Value, Is.EqualTo(value.Value));
            Assert.That(clone.TypeName, Is.EqualTo("TIME"));
        }

        [Test]
        public void Test_SqlTimeTypeValueFactory_MakeMethodsDontFailForUnsupportedType()
        {
            CallMakeMethodsWith("dummy");
            CallMakeMethodsWith("");
            CallMakeMethodsWith(null);
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

            Assert.DoesNotThrow(() => ValueFactory.MakeSqlDataTypeReference(dummyType));
            Assert.That(ValueFactory.MakeSqlDataTypeReference(dummyType), Is.Null);

            Assert.DoesNotThrow(() => ValueFactory.NewLiteral(dummyType, "123", default));
            Assert.That(ValueFactory.NewLiteral(dummyType, "123", default), Is.Null);
        }
    }
}
