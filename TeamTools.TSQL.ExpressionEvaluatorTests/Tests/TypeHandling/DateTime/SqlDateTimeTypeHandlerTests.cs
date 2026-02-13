using NUnit.Framework;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;
using TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.ExpressionEvaluatorTests.Tests.TypeHandling.DateTime
{
    [Category("TeamTools.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(SqlDateTimeTypeHandler))]
    internal class SqlDateTimeTypeHandlerTests : BaseSqlTypeHandlerTestClass
    {
        private SqlDateTimeTypeHandler typeHandler;

        private SqlDateTimeTypeValueFactory ValueFactory => typeHandler.DateTimeValueFactory;

        [OneTimeSetUp]
        public override void SetUp()
        {
            base.SetUp();
            typeHandler = new SqlDateTimeTypeHandler(Converter, Violations);
        }

        [Test]
        public void Test_SqlDateTimeTypeHandler_CanConvertFromValidStr()
        {
            var date = typeHandler.TypeConverter.ImplicitlyConvert<SqlDateTimeValue>(typeHandler.ConvertFrom(MakeStr("2007-12-30 12:30:55"), "DATETIME"));

            Assert.That(date, Is.Not.Null);
            Assert.That(date.IsPreciseValue, Is.True);
            Assert.That(date.Value, Is.EqualTo(new System.DateTime(2007, 12, 30, 12, 30, 55)));
        }

        [Test]
        public void Test_SqlDateTimeTypeHandler_ChangeToAppliesProvidedValue()
        {
            var date = typeHandler.TypeConverter.ImplicitlyConvert<SqlDateTimeValue>(typeHandler.ConvertFrom(MakeStr("2007-12-30 12:30:55"), "DATETIME"));

            Assert.That(date, Is.Not.Null);

            date = date.ChangeTo(new System.DateTime(2015, 11, 23, 15, 44, 23), new SqlValueSource(SqlValueSourceKind.Expression, null));
            Assert.That(date.Value, Is.EqualTo(new System.DateTime(2015, 11, 23, 15, 44, 23)));
        }
    }
}
