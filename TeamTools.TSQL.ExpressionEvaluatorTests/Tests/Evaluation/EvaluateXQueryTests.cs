using Microsoft.SqlServer.TransactSql.ScriptDom;
using NUnit.Framework;
using System;
using TeamTools.TSQL.ExpressionEvaluator.Evaluation;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Violations;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("TeamTools.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(SqlScriptAnalyzer))]
    public class EvaluateXQueryTests : BaseEvaluatorTestClass
    {
        private VariableDeclarationVisitor declareVisitor;
        private SqlScriptAnalyzer analyzer;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            declareVisitor = new VariableDeclarationVisitor(VarReg, TypeResolver);
            analyzer = new SqlScriptAnalyzer(VarReg, Eval, TypeResolver, Converter, new MockCondHandler(), Violations);
        }

        [Test]
        public void Test_XQueryValue_ExtractsTypeInfo()
        {
            var dom = ParseScript(@"
            DECLARE @xvalue VARCHAR(15)
            SET @xvalue = (
                    SELECT foo
                    FROM @bar
                    FOR XML PATH(''), TYPE
                ).value('.', 'varchar(4000)');
            ");

            dom.Accept(declareVisitor);
            dom.Accept(analyzer);

            // Because 4000 is way longer than 15
            Assert.That(Violations.ViolationCount, Is.EqualTo(1));
            Assert.That(Violations.Violations[0], Is.InstanceOf(typeof(ImplicitTruncationViolation)));

            var v = (ImplicitTruncationViolation)Violations.Violations[0];
            Assert.That(v.ValueSize, Is.EqualTo(4000));
            Assert.That(v.TypeSize, Is.EqualTo(15));
        }
    }
}
