using Microsoft.SqlServer.TransactSql.ScriptDom;
using NUnit.Framework;
using TeamTools.TSQL.ExpressionEvaluator.Values;
using TeamTools.TSQL.ExpressionEvaluator.Violations;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("TeamTools.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(SqlViolation))]
    public sealed class SqlViolationTests
    {
        [Test]
        public void Test_SqlViolation_DoesNotFailOnNullNode()
        {
            Assert.DoesNotThrow(() => new MockSqlViolation("msg", null));
            Assert.DoesNotThrow(() => new MockSqlViolation("msg", new SqlValueSource(SqlValueSourceKind.Variable, null)));
        }

        [Test]
        public void Test_SqlViolation_SavesPassedArgs()
        {
            var v = new MockSqlViolation("msg", new SqlValueSource(SqlValueSourceKind.Variable, new SetVariableStatement() { FirstTokenIndex = 123 }));

            Assert.That(v.Message, Is.EqualTo("msg"));
            Assert.That(v.TokenIndex, Is.EqualTo(123));
        }

        [Test]
        public void Test_SqlViolation_SavesSourceInfo()
        {
            var v = new MockSqlViolation("msg", new SqlValueSource(SqlValueSourceKind.Variable, null));

            Assert.That(v.Message, Is.EqualTo("msg"));
            Assert.That(v.Source?.SourceKind, Is.EqualTo(SqlValueSourceKind.Variable));
            Assert.That(v.TokenIndex, Is.EqualTo(0));

            var node = new DeclareVariableElement
            {
                FirstTokenIndex = 123,
                LastTokenIndex = 321,
            };

            v = new MockSqlViolation("msg", new SqlValueSource(SqlValueSourceKind.Variable, node));

            Assert.That(v.Message, Is.EqualTo("msg"));
            Assert.That(v.Source, Is.Not.Null);
            Assert.That(v.Source.SourceKind, Is.EqualTo(SqlValueSourceKind.Variable));
            Assert.That(v.Source.Node, Is.EqualTo(node));
            Assert.That(v.TokenIndex, Is.EqualTo(123));
        }

        private class MockSqlViolation : SqlViolation
        {
            public MockSqlViolation(string messsage, SqlValueSource source) : base(messsage, source)
            {
            }
        }
    }
}
