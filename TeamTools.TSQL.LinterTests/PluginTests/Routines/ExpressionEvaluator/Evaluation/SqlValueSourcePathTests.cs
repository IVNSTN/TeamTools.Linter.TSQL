using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.Evaluation;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("Linter.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(SqlScriptAnalyzer))]
    public sealed class SqlValueSourcePathTests : BaseEvaluatorTestClass
    {
        [Test]
        public void Test_Evaluation_IssuesSourceAsExpressionFromFormula()
        {
            var ev = ExtractExpressionValue("SELECT @num + 5");
            Assert.That(ev.SourceKind, Is.EqualTo(SqlValueSourceKind.Expression));

            ev = ExtractExpressionValue("SELECT @num + 5 + CHARINDEX('x', @str)");
            Assert.That(ev.SourceKind, Is.EqualTo(SqlValueSourceKind.Expression));
        }

        [Test]
        public void Test_Evaluation_IssuesSourceAsExpressionFromFunction()
        {
            var ev = ExtractExpressionValue("SET @var = LEFT(@str, 1)");
            Assert.That(ev.SourceKind, Is.EqualTo(SqlValueSourceKind.Expression));

            ev = ExtractExpressionValue("SELECT CHARINDEX('x', @str)");
            Assert.That(ev.SourceKind, Is.EqualTo(SqlValueSourceKind.Expression));
        }

        [Test]
        public void Test_Evaluation_PreciseFunctionResultFromLiteralsIsLiteral()
        {
            var ev = ExtractExpressionValue("SELECT CHARINDEX('x', 'aaa-x-bbb')");
            Assert.That(ev.SourceKind, Is.EqualTo(SqlValueSourceKind.Literal));

            Assert.That(ev, Is.InstanceOf<SqlIntTypeValue>());
            Assert.That(ev.IsPreciseValue, Is.True);
            Assert.That((ev as SqlIntTypeValue).Value, Is.EqualTo(5));
        }

        [Test]
        public void Test_Evaluation_IssuesSourceAsVariableFromVarReference()
        {
            var ev = ExtractExpressionValue("SET @another_var = @str");
            Assert.That(ev.SourceKind, Is.EqualTo(SqlValueSourceKind.Variable));
        }

        [Test]
        public void Test_Evaluation_IssuesSourceAsVariableEvenAfterUnaryOperation()
        {
            var ev = ExtractExpressionValue("SET @another_var = -(@num)");
            Assert.That(ev.SourceKind, Is.EqualTo(SqlValueSourceKind.Variable));

            ev = ExtractExpressionValue("SELECT @another_var = + @num");
            Assert.That(ev.SourceKind, Is.EqualTo(SqlValueSourceKind.Variable));
        }

        [Test]
        public void Test_Evaluation_VarRegistryAlwaysIssuesSourceAsVariable()
        {
            // value origin was defined as literal (see SetUp method)
            var ev = VarReg.GetValueAt("@str", 2);
            Assert.That(ev.SourceKind, Is.EqualTo(SqlValueSourceKind.Variable));

            // value origin was defined as expression (see SetUp method)
            ev = VarReg.GetValueAt("@str", 100);
            Assert.That(ev.SourceKind, Is.EqualTo(SqlValueSourceKind.Variable));
        }

        [Test]
        public void Test_Evaluation_RedundantConversionLeavesVarAsVar()
        {
            var ev = VarReg.GetValueAt("@num", 2);
            Assert.That(ev, Is.Not.Null);
            Assert.That(ev.TypeName, Is.EqualTo("dbo.SMALLINT"));

            ev = Converter.ImplicitlyConvertTo("dbo.SMALLINT", ev);

            Assert.That(ev, Is.Not.Null);
            Assert.That(ev.TypeName, Is.EqualTo("dbo.SMALLINT"));
            Assert.That(ev.SourceKind, Is.EqualTo(SqlValueSourceKind.Variable), "implicit");

            ev = Converter.ExplicitlyConvertTo(MakeRef("dbo.SMALLINT"), ev);

            Assert.That(ev, Is.Not.Null);
            Assert.That(ev.TypeName, Is.EqualTo("dbo.SMALLINT"));
            Assert.That(ev.SourceKind, Is.EqualTo(SqlValueSourceKind.Variable), "explicit");
        }

        [Test]
        public void Test_Evaluation_ConversionTurnsVarIntoExpression()
        {
            var ev = VarReg.GetValueAt("@num", 2);

            Assert.That(ev, Is.Not.Null);
            Assert.That(ev.TypeName, Is.EqualTo("dbo.SMALLINT"));
            Assert.That(ev.SourceKind, Is.EqualTo(SqlValueSourceKind.Variable));

            var str = Converter.ImplicitlyConvertTo("dbo.VARCHAR", ev);

            Assert.That(str, Is.Not.Null);
            Assert.That(str.TypeName, Is.EqualTo("dbo.VARCHAR"));
            Assert.That(str.SourceKind, Is.EqualTo(SqlValueSourceKind.Expression), "implicit");

            str.Source = new SqlValueSource(SqlValueSourceKind.Variable, null);
            var backInt = Converter.ExplicitlyConvertTo(MakeRef("dbo.INT"), str);

            Assert.That(backInt, Is.Not.Null);
            Assert.That(backInt.TypeName, Is.EqualTo("dbo.INT"));
            Assert.That(backInt.SourceKind, Is.EqualTo(SqlValueSourceKind.Expression), "explicit");
        }

        [Test]
        public void Test_Evaluation_IssuesSourceAsLiteralFromLiteral()
        {
            var ev = ExtractExpressionValue("SET @int = 123");
            Assert.That(ev.SourceKind, Is.EqualTo(SqlValueSourceKind.Literal));

            ev = ExtractExpressionValue("SELECT 'ABCD'");
            Assert.That(ev.SourceKind, Is.EqualTo(SqlValueSourceKind.Literal));
        }

        [Test]
        public void Test_Evaluation_RedundantConversionLeavesLiteralAsLiteral()
        {
            var ev = ExtractExpressionValue("SET @int = 123");

            Assert.That(ev, Is.Not.Null);
            Assert.That(ev.TypeName, Is.EqualTo("dbo.INT"));
            Assert.That(ev.SourceKind, Is.EqualTo(SqlValueSourceKind.Literal));

            ev = Converter.ImplicitlyConvertTo("dbo.INT", ev);

            Assert.That(ev, Is.Not.Null);
            Assert.That(ev.TypeName, Is.EqualTo("dbo.INT"));
            Assert.That(ev.SourceKind, Is.EqualTo(SqlValueSourceKind.Literal), "implicit");

            ev = Converter.ExplicitlyConvertTo(MakeRef("dbo.INT"), ev);

            Assert.That(ev, Is.Not.Null);
            Assert.That(ev.TypeName, Is.EqualTo("dbo.INT"));
            Assert.That(ev.SourceKind, Is.EqualTo(SqlValueSourceKind.Literal), "explicit");
        }

        [Test]
        public void Test_Evaluation_ConversionLeavesPreciseLiteralAsLiteral()
        {
            var ev = ExtractExpressionValue("SET @int = 123");

            Assert.That(ev, Is.Not.Null);
            Assert.That(ev.TypeName, Is.EqualTo("dbo.INT"));
            Assert.That(ev.SourceKind, Is.EqualTo(SqlValueSourceKind.Literal));

            var str = Converter.ImplicitlyConvertTo("dbo.VARCHAR", ev);

            Assert.That(str, Is.Not.Null);
            Assert.That(str.TypeName, Is.EqualTo("dbo.VARCHAR"));
            Assert.That(str.SourceKind, Is.EqualTo(SqlValueSourceKind.Literal), "implicit");

            str.Source = new SqlValueSource(SqlValueSourceKind.Literal, null);
            var backInt = Converter.ExplicitlyConvertTo(MakeRef("dbo.INT"), str);

            Assert.That(backInt, Is.Not.Null);
            Assert.That(backInt.TypeName, Is.EqualTo("dbo.INT"));
            Assert.That(backInt.SourceKind, Is.EqualTo(SqlValueSourceKind.Literal), "explicit");
        }

        [Test]
        public void Test_Evaluation_LiteralMathResultsWithLiteral()
        {
            var ev = ExtractExpressionValue("SELECT 3 - 1");
            Assert.That(ev.SourceKind, Is.EqualTo(SqlValueSourceKind.Literal));

            ev = ExtractExpressionValue("SELECT 'a' + 'b'");
            Assert.That(ev.SourceKind, Is.EqualTo(SqlValueSourceKind.Literal));
        }

        [Test]
        public void Test_Evaluation_VariableMathResultsWithExpression()
        {
            var ev = ExtractExpressionValue("SET @another_var = @num * 2");
            Assert.That(ev.SourceKind, Is.EqualTo(SqlValueSourceKind.Expression));

            ev = ExtractExpressionValue("SET @another_var = 'ABCD' + @str");
            Assert.That(ev.SourceKind, Is.EqualTo(SqlValueSourceKind.Expression));
        }

        [Test]
        public void Test_Evaluation_UnknownVariableMathResultsWithNull()
        {
            var ev = ExtractExpressionValue("SELECT @unknown * 2", false);
            Assert.That(ev, Is.Null);

            ev = ExtractExpressionValue("SELECT 'ABCD' + @dummy", false);
            Assert.That(ev, Is.Null);
        }

        private SqlValue ExtractExpressionValue(string script, bool strict = true)
        {
            var dom = ParseScript(script);

            var values = new List<SqlValue>();
            ExpressionVisitor.Scan(
                dom,
                expr =>
                {
                    var ev = Eval.EvaluateExpression(expr);
                    if (ev != null || !strict)
                    {
                        values.Add(ev);
                    }
                });

            if (strict)
            {
                Assert.That(values, Is.Not.Empty);
            }

            // the topmost parent expression is visited first
            // nested expressions are visited afterwards
            var ev = values.FirstOrDefault();

            if (strict)
            {
                Assert.That(ev, Is.Not.Null, "failed to evaluate");
                Assert.That(ev.Source, Is.Not.Null, "no source");
            }

            return ev;
        }

        private SqlTypeReference MakeRef(string typeName)
        {
            return IntHandler.MakeSqlDataTypeReference(typeName);
        }
    }
}
