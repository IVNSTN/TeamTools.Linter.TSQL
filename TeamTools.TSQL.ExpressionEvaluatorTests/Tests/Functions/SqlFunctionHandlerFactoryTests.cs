using NUnit.Framework;
using System;
using System.Collections.Generic;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.Core;
using TeamTools.TSQL.ExpressionEvaluator.Evaluation;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator.TypeHandling
{
    [Category("TeamTools.TSQL.ExpressionEvaluator")]
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

            public override SqlValue Evaluate(List<SqlFunctionArgument> args, EvaluationContext context) => default;
        }
    }
}
