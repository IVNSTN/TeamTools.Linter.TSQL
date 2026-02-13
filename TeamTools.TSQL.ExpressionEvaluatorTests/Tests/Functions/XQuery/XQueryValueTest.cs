using NUnit.Framework;
using System;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.XQueryFunctions;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("TeamTools.TSQL.ExpressionEvaluator.EvalFunctions")]
    [TestOf(typeof(XQueryValue))]
    public sealed class XQueryValueTest : BaseObjNameTest
    {
        private XQueryValue func;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new XQueryValue();
        }

        [Test]
        public void Test_XQueryValue_ReturnsPreciseType()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(MakeStr("."), MakeStr("SMALLINT")), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.False);
            Assert.That(res.TypeReference.TypeName, Is.EqualTo("SMALLINT").IgnoreCase);
        }

        [Test]
        public void Test_XQueryValue_ReturnsPreciseTypeWithLength()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(MakeStr("."), MakeStr("varchar  ( 21 )")), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.False);
            Assert.That(res.TypeReference.TypeName, Is.EqualTo("VARCHAR").IgnoreCase);
        }

        [Test]
        public void Test_XQueryValue_ReturnsPreciseTypeWithParams()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(MakeStr("."), MakeStr("DECIMAL(12,11)")), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.False);
            Assert.That(res.TypeReference.TypeName, Is.EqualTo("DECIMAL").IgnoreCase);
        }

        [Test]
        public void Test_XQueryValue_ReturnsUnknown()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(MakeStr("asdf"), MakeStr("asdf")), Context);

            Assert.That(res, Is.Null);
        }
    }
}
