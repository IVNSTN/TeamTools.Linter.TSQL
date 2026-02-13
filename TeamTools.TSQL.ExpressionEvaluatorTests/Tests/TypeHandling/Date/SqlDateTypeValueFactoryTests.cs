using NUnit.Framework;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;
using TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.ExpressionEvaluatorTests.Tests.TypeHandling.Date
{
    [Category("TeamTools.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(SqlDateTypeValueFactory))]
    public sealed class SqlDateTypeValueFactoryTests : BaseSqlTypeHandlerTestClass
    {
        private SqlDateTypeHandler typeHandler;

        private SqlDateTypeValueFactory ValueFactory => typeHandler.DateValueFactory;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            typeHandler = new SqlDateTypeHandler(Converter, Violations);
        }

        [Test]
        public void Test_SqlDateTypeValue_DeepCloneCopiesPreciseValue()
        {
            var value = ValueFactory.MakePreciseValue("DATE", new System.DateTime(2005, 07, 31), new SqlValueSource(SqlValueSourceKind.Literal, null));
            Assert.That(value, Is.Not.Null);
            Assert.That(value.Value, Is.EqualTo(new System.DateTime(2005, 07, 31)));

            var clone = value.DeepClone();
            Assert.That(clone, Is.Not.Null);
            Assert.That(clone.IsPreciseValue, Is.True);
            Assert.That(clone.Value, Is.EqualTo(value.Value));
            Assert.That(clone.TypeName, Is.EqualTo("DATE"));
        }

        [Test]
        public void Test_SqlDateTypeValueFactory_MakeMethodsDontFailForUnsupportedType()
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
