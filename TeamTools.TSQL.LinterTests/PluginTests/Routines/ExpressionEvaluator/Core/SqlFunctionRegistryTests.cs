using NUnit.Framework;
using System;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.ConversionFunctions;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.StrFunctions;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("Linter.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(SqlFunctionRegistry))]
    public sealed class SqlFunctionRegistryTests
    {
        private SqlFunctionRegistry funcReg;

        [SetUp]
        public void SetUp()
        {
            funcReg = new SqlFunctionRegistry();
        }

        [Test]
        public void Test_SqlFunctionRegistry_IsFunctionRegistered_DoesNotFail()
        {
            Assert.DoesNotThrow(() => funcReg.IsFunctionRegistered(null));
            Assert.That(funcReg.IsFunctionRegistered(null), Is.False);

            Assert.DoesNotThrow(() => funcReg.IsFunctionRegistered("dummy"));
            Assert.That(funcReg.IsFunctionRegistered("dummy"), Is.False);
        }

        [Test]
        public void Test_SqlFunctionRegistry_RegistersFunctions()
        {
            funcReg.RegisterFunction(new Upper());

            Assert.That(funcReg.IsFunctionRegistered("UPPER"), Is.True);
        }

        [Test]
        public void Test_SqlFunctionRegistry_RegisterFunctionFailsOnNullArg()
        {
            var fn = new IsNull();

            Assert.Throws(typeof(ArgumentNullException), () => funcReg.RegisterFunction(null));
        }

        [Test]
        public void Test_SqlFunctionRegistry_RegisterFunctionFailsOnDup()
        {
            var fn = new IsNull();

            Assert.DoesNotThrow(() => funcReg.RegisterFunction(fn));
            Assert.Throws(typeof(ArgumentException), () => funcReg.RegisterFunction(fn));
        }

        [Test]
        public void Test_SqlFunctionRegistry_GetFunction_DoesNotFail()
        {
            Assert.DoesNotThrow(() => funcReg.GetFunction(null));
            Assert.That(funcReg.GetFunction(null), Is.Null);

            Assert.DoesNotThrow(() => funcReg.GetFunction("dummy"));
            Assert.That(funcReg.GetFunction("dummy"), Is.Null);
        }

        [Test]
        public void Test_SqlFunctionRegistry_GetFunction_ReturnsFoundInstance()
        {
            funcReg.RegisterFunction(new Upper());

            var fn = funcReg.GetFunction("UPPER");

            Assert.That(fn, Is.Not.Null);
            Assert.That(fn, Is.InstanceOf<Upper>());
        }
    }
}
