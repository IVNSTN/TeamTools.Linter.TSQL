using NUnit.Framework;
using System.Numerics;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator.TypeHandling
{
    [Category("Linter.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(SqlBigIntTypeValueFactory))]
    public sealed class SqlBigIntTypeValueFactoryTests : BaseSqlTypeHandlerTestClass
    {
        private SqlBigIntTypeHandler typeHandler;

        private SqlBigIntTypeValueFactory ValueFactory => typeHandler.BigIntValueFactory;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            typeHandler = new SqlBigIntTypeHandler(Converter, Violations);
        }

        [Test]
        public void Test_SqlBigIntTypeValueFactory_NewLiteralDoesNotFail()
        {
            SqlValue res;

            Assert.DoesNotThrow(() => ValueFactory.NewLiteral(ValueFactory.FallbackTypeName, null, default), "null");
            res = ValueFactory.NewLiteral(ValueFactory.FallbackTypeName, null, default);
            Assert.That(res, Is.Not.Null, "null");
            Assert.That(res, Is.InstanceOf<SqlBigIntTypeValue>(), "null");
            Assert.That(res.IsPreciseValue, Is.False, "null");

            Assert.DoesNotThrow(() => ValueFactory.NewLiteral(ValueFactory.FallbackTypeName, "adsf", default), "text");
            res = ValueFactory.NewLiteral(ValueFactory.FallbackTypeName, "adsf", default);
            Assert.That(res, Is.Not.Null, "text");
            Assert.That(res, Is.InstanceOf<SqlBigIntTypeValue>(), "text");
            Assert.That(res.IsPreciseValue, Is.False, "text");
        }

        [Test]
        public void Test_SqlBigIntTypeValueFactory_NewLiteralRespectsOriginalValue()
        {
            BigInteger original = (BigInteger)int.MaxValue * -10;

            var res = ValueFactory.NewLiteral(ValueFactory.FallbackTypeName, original.ToString(), default);

            Assert.That(res, Is.Not.Null);
            Assert.That(res, Is.InstanceOf<SqlBigIntTypeValue>());
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res.SourceKind, Is.EqualTo(SqlValueSourceKind.Literal));
            Assert.That((res as SqlBigIntTypeValue).Value, Is.EqualTo(original));
        }

        [Test]
        public void Test_SqlBigIntTypeValueFactory_MakeMethodsDontFailForUnsupportedType()
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

            Assert.DoesNotThrow(() => ValueFactory.MakeSqlTypeReference(dummyType));
            Assert.That(ValueFactory.MakeSqlTypeReference(dummyType), Is.Null);

            Assert.DoesNotThrow(() => ValueFactory.NewLiteral(dummyType, "123", default));
            Assert.That(ValueFactory.NewLiteral(dummyType, "123", default), Is.Null);
        }
    }
}
