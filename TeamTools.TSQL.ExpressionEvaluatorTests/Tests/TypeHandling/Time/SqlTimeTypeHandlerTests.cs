using NUnit.Framework;
using System;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;
using TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.ExpressionEvaluatorTests.Tests.TypeHandling.Time
{
    [Category("TeamTools.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(SqlTimeTypeHandler))]
    internal class SqlTimeTypeHandlerTests : BaseSqlTypeHandlerTestClass
    {
        private SqlTimeTypeHandler typeHandler;

        private SqlTimeTypeValueFactory ValueFactory => typeHandler.TimeValueFactory;

        [OneTimeSetUp]
        public override void SetUp()
        {
            base.SetUp();
            typeHandler = new SqlTimeTypeHandler(Converter, Violations);
        }

        [Test]
        public void Test_SqlTimeTypeHandler_CanConvertFromValidStr()
        {
            var time = typeHandler.TypeConverter.ImplicitlyConvert<SqlTimeOnlyValue>(typeHandler.ConvertFrom(MakeStr("12:30:55"), "TIME"));

            Assert.That(time, Is.Not.Null);
            Assert.That(time.IsPreciseValue, Is.True);
            Assert.That(time.Value, Is.EqualTo(new TimeSpan(12, 30, 55)));
        }

        [Test]
        public void Test_SqlTimeTypeHandler_ChangeToAppliesProvidedValue()
        {
            var time = typeHandler.TypeConverter.ImplicitlyConvert<SqlTimeOnlyValue>(typeHandler.ConvertFrom(MakeStr("12:30:55"), "TIME"));

            Assert.That(time, Is.Not.Null);

            time = time.ChangeTo(new TimeSpan(15, 44, 21), new SqlValueSource(SqlValueSourceKind.Expression, null));
            Assert.That(time.Value, Is.EqualTo(new TimeSpan(15, 44, 21)));
        }
    }
}
