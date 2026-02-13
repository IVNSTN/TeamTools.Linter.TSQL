using NUnit.Framework;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator.TypeHandling
{
    [Category("TeamTools.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(SqlStrTypeValue))]
    public sealed class SqlStrTypeValueTests : BaseSqlTypeHandlerTestClass
    {
        private SqlStrTypeHandler typeHandler;

        private SqlStrTypeValueFactory ValueFactory => typeHandler.StrValueFactory;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            typeHandler = new SqlStrTypeHandler(Converter, Violations);

            TypeResolver.RegisterTypeHandler(typeHandler);
        }

        [Test]
        public void Test_SqlStrTypeValue_ChangeToReturnsExpectedValue()
        {
            var value = ValueFactory.MakePreciseValue("CHAR", "asdf", new SqlValueSource(SqlValueSourceKind.Literal, null));

            Assert.That(value, Is.Not.Null);

            var nextValue = value.ChangeTo("qwerty", new SqlValueSource(SqlValueSourceKind.Variable, null));
            Assert.That(nextValue, Is.Not.Null);
            Assert.That(nextValue.TypeName, Is.EqualTo("CHAR"));
            Assert.That(nextValue.SourceKind, Is.EqualTo(SqlValueSourceKind.Variable));
            Assert.That(nextValue.IsPreciseValue, Is.True);
            Assert.That(nextValue.Value, Is.EqualTo("qwerty"));
        }

        [Test]
        public void Test_SqlStrTypeValue_ChangeSizeToReturnsExpectedValue()
        {
            var value = ValueFactory.MakeApproximateValue("NVARCHAR", 12, new SqlValueSource(SqlValueSourceKind.Literal, null));

            Assert.That(value, Is.Not.Null);

            const int newSize = 255;
            var nextValue = value.ChangeTo(newSize, new SqlValueSource(SqlValueSourceKind.Expression, null));
            Assert.That(nextValue, Is.Not.Null);
            Assert.That(nextValue.TypeName, Is.EqualTo("NVARCHAR"));
            Assert.That(nextValue.SourceKind, Is.EqualTo(SqlValueSourceKind.Expression));
            Assert.That(nextValue.IsPreciseValue, Is.False);
            Assert.That(nextValue.EstimatedSize, Is.EqualTo(newSize));
        }

        [Test]
        public void Test_SqlStrTypeValue_GetHandlerReturnsExpectedValue()
        {
            var value = ValueFactory.MakeApproximateValue("NVARCHAR", 12, new SqlValueSource(SqlValueSourceKind.Literal, null));

            Assert.That(value, Is.Not.Null);
            Assert.That(value.GetTypeHandler(), Is.EqualTo(typeHandler));
        }
    }
}
