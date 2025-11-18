using NUnit.Framework;
using TeamTools.TSQL.ExpressionEvaluator.Evaluation;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("TeamTools.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(SqlScriptAnalyzer))]
    public class EvaluateProcIdTests : BaseEvaluatorTestClass
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
        public void Test_SqlScriptAnalyzerEvaluateProcId_RegistersGlobalVarForProcs()
        {
            ValidateVariableEvaluation(
                @"CREATE PROCEDURE dbo.my_proc AS
                BEGIN
                    DECLARE @object_name NVARCHAR(128) = OBJECT_NAME(@@PROCID);
                END;",
                "my_proc");
        }

        [Test]
        public void Test_SqlScriptAnalyzerEvaluateProcId_RegistersGlobalVarForTriggers()
        {
            ValidateVariableEvaluation(
                @"CREATE TRIGGER dbo.my_trigger ON dbo.my_table AFTER INSERT AS
                BEGIN
                    DECLARE @object_name NVARCHAR(128) = OBJECT_NAME(@@PROCID);
                END;",
                "my_trigger");
        }

        [Test]
        public void Test_SqlScriptAnalyzerEvaluateProcId_RegistersGlobalVarForFuncs()
        {
            ValidateVariableEvaluation(
                @"CREATE FUNCTION dbo.my_fn()
                RETURNS NVARCHAR(128)
                AS
                BEGIN
                    DECLARE @object_name NVARCHAR(128) = OBJECT_NAME(@@PROCID);
                    RETURN @object_name;
                END;",
                "my_fn");
        }

        [Test]
        public void Test_SqlScriptAnalyzerEvaluateProcId_IgnoresClrProcs()
        {
            var dom = ParseScript(@"CREATE PROCEDURE dbo.my_proc AS
            EXTERNAL NAME myassembly.myclass.mymethod");

            dom.Accept(declareVisitor);
            dom.Accept(analyzer);

            Assert.That(VarReg.IsVariableRegistered("@@PROCID"), Is.False, "global var registered");
        }

        private void ValidateVariableEvaluation(string script, string objectName)
        {
            var dom = ParseScript(script);

            dom.Accept(declareVisitor);
            dom.Accept(analyzer);

            Assert.That(VarReg.IsVariableRegistered("@@PROCID"), Is.True, "global not registered");
            Assert.That(VarReg.IsVariableRegistered("@object_name"), Is.True, "local var registered");
            var value = VarReg.GetValueAt("@object_name", 100);

            Assert.That(value, Is.Not.Null);
            Assert.That(value.IsPreciseValue, Is.True);
            Assert.That(value, Is.InstanceOf<SqlStrTypeValue>());
            Assert.That((value as SqlStrTypeValue).Value, Is.EqualTo(objectName));
        }
    }
}
