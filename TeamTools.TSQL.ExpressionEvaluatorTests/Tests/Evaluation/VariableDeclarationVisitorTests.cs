using NUnit.Framework;
using TeamTools.TSQL.ExpressionEvaluator.Evaluation;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("TeamTools.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(VariableDeclarationVisitor))]
    public sealed class VariableDeclarationVisitorTests : BaseEvaluatorTestClass
    {
        private VariableDeclarationVisitor declareVisitor;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            declareVisitor = new VariableDeclarationVisitor(VarReg, TypeResolver);
        }

        [Test]
        public void Test_VariableDeclarationVisitor_RegistersVariables()
        {
            var dom = ParseScript("DECLARE @my_var NCHAR(10)");

            dom.Accept(declareVisitor);

            Assert.That(VarReg.IsVariableRegistered("@my_var"), Is.True);
        }

        [Test]
        public void Test_VariableDeclarationVisitor_RegistersParameters()
        {
            var dom = ParseScript(@"CREATE PROCEDURE dbo.my_proc
                @my_arg SMALLINT
            AS RETURN 1");

            dom.Accept(declareVisitor);

            Assert.That(VarReg.IsVariableRegistered("@my_arg"), Is.True);
        }
    }
}
