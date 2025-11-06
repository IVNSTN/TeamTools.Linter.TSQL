using NUnit.Framework;
using System.Numerics;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator.TypeHandling
{
    [Category("Linter.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(SqlBigIntTypeValue))]
    public sealed class SqlBigIntTypeValueTests : BaseSqlTypeHandlerTestClass
    {
        private SqlBigIntTypeHandler typeHandler;

        private SqlBigIntTypeValueFactory ValueFactory => typeHandler.BigIntValueFactory;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            typeHandler = new SqlBigIntTypeHandler(Converter, Violations);

            TypeResolver.RegisterTypeHandler(typeHandler);
        }

        [Test]
        public void Test_SqlBigIntTypeValue_ChangeToReturnsExpectedValue()
        {
            var value = ValueFactory.MakePreciseValue("dbo.BIGINT", -5, new SqlValueSource(SqlValueSourceKind.Literal, null));

            Assert.That(value, Is.Not.Null);

            var nextValue = value.ChangeTo(-5000, new SqlValueSource(SqlValueSourceKind.Expression, null));
            Assert.That(nextValue, Is.Not.Null);
            Assert.That(nextValue.TypeName, Is.EqualTo("dbo.BIGINT"));
            Assert.That(nextValue.SourceKind, Is.EqualTo(SqlValueSourceKind.Expression));
            Assert.That(nextValue.IsPreciseValue, Is.True);
            Assert.That(nextValue.Value, Is.EqualTo((BigInteger)(-5000)));
        }

        [Test]
        public void Test_SqlBigIntTypeValue_ChangeSizeToReturnsExpectedValue()
        {
            var value = ValueFactory.MakeApproximateValue("dbo.BIGINT", new SqlBigIntValueRange(-10000, +10000), new SqlValueSource(SqlValueSourceKind.Literal, null));

            Assert.That(value, Is.Not.Null);

            var newSize = new SqlBigIntValueRange(5, 10);
            var nextValue = value.ChangeTo(newSize, new SqlValueSource(SqlValueSourceKind.Variable, null));
            Assert.That(nextValue, Is.Not.Null);
            Assert.That(nextValue.TypeName, Is.EqualTo("dbo.BIGINT"));
            Assert.That(nextValue.SourceKind, Is.EqualTo(SqlValueSourceKind.Variable));
            Assert.That(nextValue.IsPreciseValue, Is.False);
            Assert.That(nextValue.EstimatedSize, Is.EqualTo(newSize));
        }

        [Test]
        public void Test_SqlBigIntTypeValue_GetHandlerReturnsExpectedValue()
        {
            var value = ValueFactory.MakeApproximateValue("dbo.BIGINT", new SqlBigIntValueRange(-10000, +10000), new SqlValueSource(SqlValueSourceKind.Literal, null));

            Assert.That(value, Is.Not.Null);
            Assert.That(value.GetTypeHandler(), Is.EqualTo(typeHandler));
        }
    }
}
