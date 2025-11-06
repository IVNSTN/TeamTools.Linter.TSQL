using NUnit.Framework;
using System.Numerics;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("Linter.TSQL.ExpressionEvaluator")]
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
