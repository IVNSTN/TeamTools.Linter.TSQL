using NUnit.Framework;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator.TypeHandling
{
    [Category("Linter.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(SqlIntTypeHandler))]
    public sealed class SqlIntTypeOperatorsTests : BaseSqlTypeHandlerTestClass
    {
        private SqlIntTypeHandler typeHandler;

        private SqlIntTypeValueFactory ValueFactory => typeHandler.IntValueFactory;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            typeHandler = new SqlIntTypeHandler(Converter, Violations);
        }

        [Test]
        public void Test_SqlIntTypeHandler_Plus()
        {
            var a = ValueFactory.MakePreciseValue(ValueFactory.FallbackTypeName, 3, default);
            var b = ValueFactory.MakePreciseValue(ValueFactory.FallbackTypeName, 100, default);
            var c = typeHandler.Sum(a, b);

            Assert.That(c, Is.Not.Null);
            Assert.That(c, Is.InstanceOf<SqlIntTypeValue>());
            Assert.That(c.IsPreciseValue, Is.True);
            Assert.That((c as SqlIntTypeValue).Value, Is.EqualTo(103));
        }

        [Test]
        public void Test_SqlIntTypeHandler_Minus()
        {
            var a = ValueFactory.MakePreciseValue(ValueFactory.FallbackTypeName, 3, default);
            var b = ValueFactory.MakePreciseValue(ValueFactory.FallbackTypeName, 100, default);

            var c = typeHandler.Subtract(a, b);

            Assert.That(c, Is.Not.Null);
            Assert.That(c, Is.InstanceOf<SqlIntTypeValue>());
            Assert.That(c.IsPreciseValue, Is.True);
            Assert.That((c as SqlIntTypeValue).Value, Is.EqualTo(-97));
        }

        [Test]
        public void Test_SqlIntTypeHandler_Multiply()
        {
            var a = ValueFactory.MakePreciseValue(ValueFactory.FallbackTypeName, 3, default);
            var b = ValueFactory.MakePreciseValue(ValueFactory.FallbackTypeName, 100, default);

            var c = typeHandler.Multiply(a, b);

            Assert.That(c, Is.Not.Null);
            Assert.That(c, Is.InstanceOf<SqlIntTypeValue>());
            Assert.That(c.IsPreciseValue, Is.True);
            Assert.That((c as SqlIntTypeValue).Value, Is.EqualTo(300));
        }

        [Test]
        public void Test_SqlIntTypeHandler_Divide()
        {
            var a = ValueFactory.MakeLiteral(ValueFactory.FallbackTypeName, 100, default);
            var b = ValueFactory.MakeLiteral(ValueFactory.FallbackTypeName, 4, default);

            var c = typeHandler.Divide(a, b);

            Assert.That(c, Is.Not.Null);
            Assert.That(c, Is.InstanceOf<SqlIntTypeValue>());
            Assert.That(c.IsPreciseValue, Is.True);
            Assert.That((c as SqlIntTypeValue).Value, Is.EqualTo(25));
        }
    }
}
