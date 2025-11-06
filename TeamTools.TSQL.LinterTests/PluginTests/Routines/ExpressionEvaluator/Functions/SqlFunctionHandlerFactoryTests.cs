using NUnit.Framework;
using System;
using System.Collections.Generic;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator.TypeHandling
{
    [Category("Linter.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(SqlFunctionHandlerFactory))]
    public sealed class SqlFunctionHandlerFactoryTests
    {
        [Test]
        public void Test_SqlFunctionHandlerFactory_PassesInstancesToTheRegistry()
        {
            var funcReg = new SqlFunctionRegistry();
            SqlFunctionHandlerFactory.Initialize(funcReg);

            Assert.That(funcReg.Functions, Is.Not.Empty);
        }

        [Test]
        public void Test_SqlFunctionHandler_FailsOnNullFuncName()
        {
            Assert.Throws(typeof(ArgumentNullException), () => new StubFunctionHandler(null));
            Assert.Throws(typeof(ArgumentNullException), () => new StubFunctionHandler(""));
        }

        private class StubFunctionHandler : SqlFunctionHandler
        {
            public StubFunctionHandler(string funcName) : base(funcName)
            { }

            public override SqlValue Evaluate(IList<SqlFunctionArgument> args, EvaluationContext context) => default;
        }
    }
}
