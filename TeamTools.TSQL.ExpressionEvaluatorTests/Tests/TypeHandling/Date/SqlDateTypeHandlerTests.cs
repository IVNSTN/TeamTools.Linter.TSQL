using NUnit.Framework;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;
using TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.ExpressionEvaluatorTests.Tests.TypeHandling.Date
{
    [Category("TeamTools.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(SqlDateTypeHandler))]
    public sealed class SqlDateTypeHandlerTests : BaseSqlTypeHandlerTestClass
    {
        private SqlDateTypeHandler typeHandler;

        private SqlDateTypeValueFactory ValueFactory => typeHandler.DateValueFactory;

        [OneTimeSetUp]
        public override void SetUp()
        {
            base.SetUp();
            typeHandler = new SqlDateTypeHandler(Converter, Violations);
        }

        [Test]
        public void Test_SqlDateTypeHandler_CanConvertFromValidStr()
        {
            var date = typeHandler.TypeConverter.ImplicitlyConvert<SqlDateOnlyValue>(typeHandler.ConvertFrom(MakeStr("2007-12-30"), "DATE"));

            Assert.That(date, Is.Not.Null);
            Assert.That(date.IsPreciseValue, Is.True);
            Assert.That(date.Value, Is.EqualTo(new System.DateTime(2007, 12, 30)));
        }

        [Test]
        public void Test_SqlDateTypeHandler_ChangeToAppliesProvidedValue()
        {
            var date = typeHandler.TypeConverter.ImplicitlyConvert<SqlDateOnlyValue>(typeHandler.ConvertFrom(MakeStr("2007-12-30"), "DATE"));

            Assert.That(date, Is.Not.Null);

            date = date.ChangeTo(new System.DateTime(2015, 11, 23), new SqlValueSource(SqlValueSourceKind.Expression, null));
            Assert.That(date.Value, Is.EqualTo(new System.DateTime(2015, 11, 23)));
        }
    }
}
