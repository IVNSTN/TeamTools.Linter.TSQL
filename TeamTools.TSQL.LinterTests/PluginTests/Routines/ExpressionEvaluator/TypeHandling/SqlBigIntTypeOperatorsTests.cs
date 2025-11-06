using NUnit.Framework;
using System.Numerics;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator.TypeHandling
{
    [Category("Linter.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(SqlBigIntTypeHandler))]
    public sealed class SqlBigIntTypeOperatorsTests : BaseSqlTypeHandlerTestClass
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
        public void Test_SqlBigIntTypeHandler_Plus()
        {
            var a = ValueFactory.MakePreciseValue(ValueFactory.FallbackTypeName, 3, default);
            var b = ValueFactory.MakePreciseValue(ValueFactory.FallbackTypeName, 100, default);
            var c = typeHandler.Sum(a, b);

            Assert.That(c, Is.Not.Null);
            Assert.That(c, Is.InstanceOf<SqlBigIntTypeValue>());
            Assert.That(c.IsPreciseValue, Is.True);
            Assert.That((c as SqlBigIntTypeValue).Value, Is.EqualTo((BigInteger)103));
        }

        [Test]
        public void Test_SqlBigIntTypeHandler_Minus()
        {
            var a = ValueFactory.MakePreciseValue(ValueFactory.FallbackTypeName, 3, default);
            var b = ValueFactory.MakePreciseValue(ValueFactory.FallbackTypeName, 100, default);

            var c = typeHandler.Subtract(a, b);

            Assert.That(c, Is.Not.Null);
            Assert.That(c, Is.InstanceOf<SqlBigIntTypeValue>());
            Assert.That(c.IsPreciseValue, Is.True);
            Assert.That((c as SqlBigIntTypeValue).Value, Is.EqualTo((BigInteger)(-97)));
        }

        [Test]
        public void Test_SqlBigIntTypeHandler_Multiply()
        {
            var a = ValueFactory.MakePreciseValue(ValueFactory.FallbackTypeName, 3, default);
            var b = ValueFactory.MakePreciseValue(ValueFactory.FallbackTypeName, 100, default);

            var c = typeHandler.Multiply(a, b);

            Assert.That(c, Is.Not.Null);
            Assert.That(c, Is.InstanceOf<SqlBigIntTypeValue>());
            Assert.That(c.IsPreciseValue, Is.True);
            Assert.That((c as SqlBigIntTypeValue).Value, Is.EqualTo((BigInteger)300));
        }

        [Test]
        public void Test_SqlBigIntTypeHandler_Divide()
        {
            var a = ValueFactory.MakeLiteral(ValueFactory.FallbackTypeName, 100, default);
            var b = ValueFactory.MakeLiteral(ValueFactory.FallbackTypeName, 4, default);

            var c = typeHandler.Divide(a, b);

            Assert.That(c, Is.Not.Null);
            Assert.That(c, Is.InstanceOf<SqlBigIntTypeValue>());
            Assert.That(c.IsPreciseValue, Is.True);
            Assert.That((c as SqlBigIntTypeValue).Value, Is.EqualTo((BigInteger)25));
        }
    }
}
