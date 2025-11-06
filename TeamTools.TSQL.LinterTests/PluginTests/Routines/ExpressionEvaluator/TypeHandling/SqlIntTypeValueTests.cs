using NUnit.Framework;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator.TypeHandling
{
    [Category("Linter.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(SqlIntTypeValue))]
    public sealed class SqlIntTypeValueTests : BaseSqlTypeHandlerTestClass
    {
        private SqlIntTypeHandler typeHandler;

        private SqlIntTypeValueFactory ValueFactory => typeHandler.IntValueFactory;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            typeHandler = new SqlIntTypeHandler(Converter, Violations);

            TypeResolver.RegisterTypeHandler(typeHandler);
        }

        [Test]
        public void Test_SqlIntTypeValue_ChangeToReturnsExpectedValue()
        {
            var value = ValueFactory.MakePreciseValue("dbo.TINYINT", 123, new SqlValueSource(SqlValueSourceKind.Literal, null));

            Assert.That(value, Is.Not.Null);

            var nextValue = value.ChangeTo(5, new SqlValueSource(SqlValueSourceKind.Variable, null));
            Assert.That(nextValue, Is.Not.Null);
            Assert.That(nextValue.TypeName, Is.EqualTo("dbo.TINYINT"));
            Assert.That(nextValue.SourceKind, Is.EqualTo(SqlValueSourceKind.Variable));
            Assert.That(nextValue.IsPreciseValue, Is.True);
            Assert.That(nextValue.Value, Is.EqualTo(5));
        }

        [Test]
        public void Test_SqlIntTypeValue_ChangeSizeToReturnsExpectedValue()
        {
            var value = ValueFactory.MakeApproximateValue("dbo.SMALLINT", new SqlIntValueRange(-10000, +10000), new SqlValueSource(SqlValueSourceKind.Literal, null));

            Assert.That(value, Is.Not.Null);

            var newSize = new SqlIntValueRange(5, 10);
            var nextValue = value.ChangeTo(newSize, new SqlValueSource(SqlValueSourceKind.Variable, null));
            Assert.That(nextValue, Is.Not.Null);
            Assert.That(nextValue.TypeName, Is.EqualTo("dbo.SMALLINT"));
            Assert.That(nextValue.SourceKind, Is.EqualTo(SqlValueSourceKind.Variable));
            Assert.That(nextValue.IsPreciseValue, Is.False);
            Assert.That(nextValue.EstimatedSize, Is.EqualTo(newSize));
        }

        [Test]
        public void Test_SqlIntTypeValue_GetHandlerReturnsExpectedValue()
        {
            var value = ValueFactory.MakeApproximateValue("dbo.SMALLINT", new SqlIntValueRange(-10000, +10000), new SqlValueSource(SqlValueSourceKind.Literal, null));

            Assert.That(value, Is.Not.Null);
            Assert.That(value.GetTypeHandler(), Is.EqualTo(typeHandler));
        }
    }
}
