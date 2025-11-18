using NUnit.Framework;
using System.Numerics;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("TeamTools.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(SqlValueCompareResultConverter))]
    public sealed class SqlValueCompareResultConverterTests
    {
        [Test]
        public void Test_SqlValueCompareResul_Int_IsConvertedCorrectly()
        {
            Assert.That(1.ToCompareResult(), Is.EqualTo(SqlValueCompareResult.Greater), ">");
            Assert.That((-2).ToCompareResult(), Is.EqualTo(SqlValueCompareResult.Less), "<");
            Assert.That(0.ToCompareResult(), Is.EqualTo(SqlValueCompareResult.Equal), "=");
        }

        [Test]
        public void Test_SqlValueCompareResul_BigIntIsConvertedCorrectly()
        {
            Assert.That(((BigInteger)3).ToCompareResult(), Is.EqualTo(SqlValueCompareResult.Greater), ">");
            Assert.That(((BigInteger)(-2)).ToCompareResult(), Is.EqualTo(SqlValueCompareResult.Less), "<");
            Assert.That(((BigInteger)0).ToCompareResult(), Is.EqualTo(SqlValueCompareResult.Equal), "=");
        }
    }
}
