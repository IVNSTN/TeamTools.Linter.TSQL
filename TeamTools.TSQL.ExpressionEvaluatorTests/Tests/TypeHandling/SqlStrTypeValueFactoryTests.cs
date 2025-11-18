using NUnit.Framework;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator.TypeHandling
{
    [Category("TeamTools.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(SqlStrTypeValueFactory))]
    public sealed class SqlStrTypeValueFactoryTests : BaseSqlTypeHandlerTestClass
    {
        private SqlStrTypeHandler typeHandler;

        private SqlStrTypeValueFactory ValueFactory => typeHandler.StrValueFactory;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            typeHandler = new SqlStrTypeHandler(Converter, Violations);
        }

        [Test]
        public void Test_SqlStrTypeValueFactory_NewLiteralDoesNotFail()
        {
            SqlValue res;

            Assert.DoesNotThrow(() => ValueFactory.NewLiteral(ValueFactory.FallbackTypeName, null, default), "null");
            res = ValueFactory.NewLiteral(ValueFactory.FallbackTypeName, null, default);
            Assert.That(res, Is.Not.Null, "null");
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>(), "null");
            Assert.That(res.IsPreciseValue, Is.False, "null");
        }

        [Test]
        public void Test_SqlStrTypeValueFactory_NewLiteralRespectsOriginalValue()
        {
            const string original = "AsDf ";

            var res = ValueFactory.NewLiteral(ValueFactory.FallbackTypeName, original, default);

            Assert.That(res, Is.Not.Null);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res.SourceKind, Is.EqualTo(SqlValueSourceKind.Literal));
            Assert.That(string.Equals(original, (res as SqlStrTypeValue).Value, System.StringComparison.InvariantCulture), Is.True);
        }

        [Test]
        public void Test_SqlStrTypeValueFactory_MakeMethodsDontFailForUnsupportedType()
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

            Assert.DoesNotThrow(() => ValueFactory.MakeSqlTypeReference(dummyType, 1));
            Assert.That(ValueFactory.MakeSqlTypeReference(dummyType, 2), Is.Null);

            Assert.DoesNotThrow(() => ValueFactory.NewLiteral(dummyType, "123", default));
            Assert.That(ValueFactory.NewLiteral(dummyType, "123", default), Is.Null);
        }
    }
}
