using Microsoft.SqlServer.TransactSql.ScriptDom;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions;
using TeamTools.TSQL.ExpressionEvaluator.Core;
using TeamTools.TSQL.ExpressionEvaluator.Interfaces;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    public abstract class BaseEvaluatorTestClass
    {
        protected ViolationReporter Violations { get; private set; }

        protected SqlTypeResolver TypeResolver { get; private set; }

        protected SqlTypeConverter Converter { get; private set; }

        protected SqlVariableRegistry VarReg { get; private set; }

        protected SqlFunctionRegistry FuncReg { get; private set; }

        protected SqlLiteralValueFactory LiteralFactory { get; private set; }

        protected SqlExpressionEvaluator Eval { get; private set; }

        protected TSqlParser Parser { get; private set; }

        protected SqlIntTypeHandler IntHandler { get; private set; }

        protected SqlStrTypeHandler StrHandler { get; private set; }

        [SetUp]
        public virtual void SetUp()
        {
            // TODO : take version from config
            Parser = TSqlParser.CreateParser(SqlVersion.Sql150, true);

            Violations = new ViolationReporter();
            TypeResolver = new SqlTypeResolver();
            Converter = new SqlTypeConverter(TypeResolver);
            VarReg = new SqlVariableRegistry(Converter, Violations);
            FuncReg = new SqlFunctionRegistry();
            LiteralFactory = new SqlLiteralValueFactory(TypeResolver);
            Eval = new SqlExpressionEvaluator(VarReg, FuncReg, Converter, TypeResolver, LiteralFactory, Violations, e => new MockCondHandler());

            SqlFunctionHandlerFactory.Initialize(FuncReg);

            IntHandler = new SqlIntTypeHandler(Converter, Violations);
            TypeResolver.RegisterTypeHandler(IntHandler);

            StrHandler = new SqlStrTypeHandler(Converter, Violations);
            TypeResolver.RegisterTypeHandler(StrHandler);

            // string var with precise value
            var strVarTypeRef = new SqlStrTypeReference(
                "VARCHAR",
                10,
                TypeResolver.ResolveTypeHandler("VARCHAR").ValueFactory);

            VarReg.RegisterVariable("@str", strVarTypeRef);
            var v = StrHandler.StrValueFactory.MakeLiteral(strVarTypeRef.TypeName, "ASDF", null);
            VarReg.RegisterEvaluatedValue("@str", 1, v);
            v = StrHandler.StrValueFactory.MakePreciseValue(strVarTypeRef.TypeName, "QWERTY", null);
            v.Source = new SqlValueSource(SqlValueSourceKind.Expression, default);
            VarReg.RegisterEvaluatedValue("@str", 15, v);

            // int var with approximate value
            var intTypeRef = new SqlIntTypeReference(
                    "SMALLINT",
                    new SqlIntValueRange(-1000, 1000),
                    TypeResolver.ResolveTypeHandler("SMALLINT").ValueFactory);

            VarReg.RegisterVariable("@num", intTypeRef);
            VarReg.RegisterEvaluatedValue("@num", 1, IntHandler.IntValueFactory.MakeApproximateValue(intTypeRef.TypeName, new SqlIntValueRange(0, 100), new SqlValueSource(SqlValueSourceKind.Expression, null)));
        }

        protected TSqlFragment ParseScript(string script)
        {
            var dom = Parser.Parse(new StringReader(script), out IList<ParseError> err);
            Assert.That(err, Is.Empty, "parsing error: " + err.FirstOrDefault()?.Message);
            Assert.That(dom, Is.Not.Null, "parsing failed");

            return dom;
        }

        protected class MockCondHandler : IConditionalFlowHandler
        {
            public bool DetectPredicatesLimitingVarValues(BooleanExpression predicate) => false;

            public bool DetectEqualityLimitingVarValues(ScalarExpression sourceValue, ScalarExpression limitDefinition) => false;

            public void ResetValueEstimatesAfterConditionalBlock(TSqlFragment block)
            { }

            public void RevertValueEstimatesToBeforeBlock(TSqlFragment block)
            { }
        }

        protected class ExpressionVisitor : TSqlFragmentVisitor
        {
            private readonly Action<ScalarExpression> callback;

            public ExpressionVisitor(Action<ScalarExpression> callback)
            {
                this.callback = callback;
            }

            public static void Scan(TSqlFragment node, Action<ScalarExpression> callback)
            {
                node.Accept(new ExpressionVisitor(callback));
            }

            public override void Visit(ScalarExpression node)
            {
                callback.Invoke(node);
            }
        }
    }
}
