using NUnit.Framework;
using System.Numerics;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.ExpressionEvaluatorTests.Tests.TypeHandling.Binary
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Assertion", "NUnit2010:Use EqualConstraint for better assertion messages in case of failure", Justification = "For self comparison test")]
    [Category("TeamTools.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(HexValue))]
    internal class HexValueTests
    {
        [Test]
        public void Test_HexValue_MakeFromString()
        {
            var a = new HexValue("1001");
            Assert.That(a.AsNumber, Is.EqualTo((BigInteger)4097));
            Assert.That(a.AsString, Is.EqualTo("1001"));
        }

        [Test]
        public void Test_HexValue_MakeFromInt()
        {
            var a = new HexValue(1);
            Assert.That(a.AsNumber, Is.EqualTo((BigInteger)1));
            Assert.That(a.AsString, Is.EqualTo("01"));

            a = new HexValue(100);
            Assert.That(a.AsNumber, Is.EqualTo((BigInteger)100));
            Assert.That(a.AsString, Is.EqualTo("64"));

            a = new HexValue(1000);
            Assert.That(a.AsNumber, Is.EqualTo((BigInteger)1000));
            Assert.That(a.AsString, Is.EqualTo("03E8"));
        }

        [Test]
        public void Test_HexValue_MakeFromBigInt()
        {
            var a = new HexValue((BigInteger)33);
            Assert.That(a.AsNumber, Is.EqualTo((BigInteger)33));
            Assert.That(a.AsString, Is.EqualTo("21"));
        }

        [Test]
        public void Test_HexValue_MakeFromBadStringDoesNotFail()
        {
            Assert.DoesNotThrow(() => new HexValue("0xZX<MCNB"));
            var v = new HexValue("0xZX<MCNB");
            Assert.That(v, Is.Not.Null);
            Assert.That(v.AsString, Is.Null);
        }

        [Test]
        public void Test_HexValue_MakeFromEmptyStringSetsZero()
        {
            Assert.DoesNotThrow(() => new HexValue("0x"));
            var v = new HexValue("0x");
            Assert.That(v, Is.Not.Null);
            Assert.That(v.AsNumber, Is.EqualTo((BigInteger)0));
        }

        [Test]
        public void Test_HexValue_ChangeAsNumber()
        {
            var a = new HexValue(31);
            Assert.That(a.AsNumber, Is.EqualTo((BigInteger)31));
            Assert.That(a.AsString, Is.EqualTo("1F"));

            a.AsNumber = 242;

            Assert.That(a.AsNumber, Is.EqualTo((BigInteger)242));
            Assert.That(a.AsString, Is.EqualTo("F2"));
        }

        [Test]
        public void Test_HexValue_ChangeAsString()
        {
            var a = new HexValue(1);
            Assert.That(a.AsNumber, Is.EqualTo((BigInteger)1));
            Assert.That(a.AsString, Is.EqualTo("01"));

            a.AsString = "1F1";

            Assert.That(a.AsString, Is.EqualTo("01F1"));
            Assert.That(a.AsNumber, Is.EqualTo((BigInteger)497));
        }

        [Test]
        public void Test_HexValue_ConcatBasedOnStr()
        {
            var a = new HexValue("100");
            var b = new HexValue("200");
            var c = a + b;

            Assert.That(c, Is.Not.Null);
            Assert.That(c.AsString, Is.EqualTo("01000200"));
            Assert.That(c.AsNumber, Is.EqualTo((BigInteger)16777728));
        }

        [Test]
        public void Test_HexValue_ConcatBasedOnInt()
        {
            var a = new HexValue(100);
            Assert.That(a.AsString, Is.EqualTo("64"));
            var b = new HexValue(200);
            Assert.That(b.AsString, Is.EqualTo("C8"));
            var c = a + b;

            Assert.That(c, Is.Not.Null);
            Assert.That(c.AsString, Is.EqualTo("64C8"));
            Assert.That(c.AsNumber, Is.EqualTo((BigInteger)25800));
        }

        [Test]
        public void Test_HexValue_MinLengthPrependsZeroes()
        {
            var a = new HexValue(4097, 5);
            Assert.That(a.AsNumber, Is.EqualTo((BigInteger)4097));
            Assert.That(a.AsString, Is.EqualTo("0000001001"));

            var b = new HexValue("0xFF", 5);
            Assert.That(b.AsNumber, Is.EqualTo((BigInteger)255));
            Assert.That(b.AsString, Is.EqualTo("00000000FF"));
        }

        [Test]
        public void Test_HexValue_ToStringPrependsShebang()
        {
            var a = new HexValue(1);
            Assert.That(a.ToString(), Is.EqualTo("0x01"));
        }

        [Test]
        public void Test_HexValue_Equals()
        {
            var a = new HexValue(1);
            var b = new HexValue(1);
            var c = new HexValue(0);

            Assert.That(a.Equals(a), Is.True);
            Assert.That(a.Equals((object)b), Is.True);
            Assert.That(a == b, Is.True);
            Assert.That(a, Is.EqualTo(b));
            Assert.That(a, Is.Not.EqualTo(c));
            Assert.That(a == c, Is.False);
            Assert.That(a != c, Is.True);
            Assert.That(a == null, Is.False);
            Assert.That(a != null, Is.True);
            Assert.That(a == (object)"", Is.False);
            Assert.That(a.Equals(123), Is.False);
        }

        [Test]
        public void Test_HexValue_GreaterThan()
        {
            var a = new HexValue(1);
            var b = new HexValue(1);
            var c = new HexValue(0);

            Assert.That(a, Is.Not.GreaterThan(b));
            Assert.That(a, Is.GreaterThan(c));

            Assert.That(a > b, Is.False);
            Assert.That(a >= b, Is.True);
            Assert.That(a > c, Is.True);
        }

        [Test]
        public void Test_HexValue_LessThan()
        {
            var a = new HexValue(1);
            var b = new HexValue(1);
            var c = new HexValue(0);

            Assert.That(a, Is.Not.LessThan(b));
            Assert.That(c, Is.LessThan(a));

            Assert.That(a < b, Is.False);
            Assert.That(a <= b, Is.True);
            Assert.That(c < a, Is.True);
        }

        [Test]
        public void Test_HexValue_CompareTo()
        {
            var a = new HexValue(1);
            var b = new HexValue(1);
            var c = new HexValue(0);

            Assert.That(a.CompareTo(b), Is.Zero);
            Assert.That(a.CompareTo(c), Is.EqualTo(1));
            Assert.That(c.CompareTo(a), Is.EqualTo(-1));
        }
    }
}
