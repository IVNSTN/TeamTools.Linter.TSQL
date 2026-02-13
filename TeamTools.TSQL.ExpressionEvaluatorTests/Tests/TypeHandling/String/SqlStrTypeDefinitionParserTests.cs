using Microsoft.SqlServer.TransactSql.ScriptDom;
using NUnit.Framework;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator.TypeHandling
{
    [Category("TeamTools.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(SqlStrTypeDefinitionParser))]
    public sealed class SqlStrTypeDefinitionParserTests
    {
        [Test]
        public void Test_SqlStrTypeDefinitionParser_HandlesMaxCorrectly()
        {
            var node = new SqlDataTypeReference { Name = new SchemaObjectName() };
            node.Name.Identifiers.Add(new Identifier { Value = "VARCHAR" });
            node.Parameters.Add(new MaxLiteral());

            var res = SqlStrTypeDefinitionParser.GetTypeSize(node, StubGetDefaultSize);

            Assert.That(res, Is.EqualTo(int.MaxValue));
        }

        [Test]
        public void Test_SqlStrTypeDefinitionParser_AsksForDefaultIfSizeOmitted()
        {
            var node = new SqlDataTypeReference { Name = new SchemaObjectName() };
            // no size parameter
            node.Name.Identifiers.Add(new Identifier { Value = "VARCHAR" });

            var res = SqlStrTypeDefinitionParser.GetTypeSize(node, StubGetDefaultSize);

            Assert.That(res, Is.EqualTo(777));
        }

        [Test]
        public void Test_SqlStrTypeDefinitionParser_ExtractsLengthValue()
        {
            var node = new SqlDataTypeReference { Name = new SchemaObjectName() };
            node.Name.Identifiers.Add(new Identifier { Value = "VARCHAR" });
            node.Parameters.Add(new IntegerLiteral { Value = "256" });

            var res = SqlStrTypeDefinitionParser.GetTypeSize(node, StubGetDefaultSize);

            Assert.That(res, Is.EqualTo(256));
        }

        [Test]
        public void Test_SqlStrTypeDefinitionParser_DoesNotFailOnBadArgs()
        {
            Assert.DoesNotThrow(() => SqlStrTypeDefinitionParser.GetTypeSize(null, StubGetDefaultSize));
            Assert.DoesNotThrow(() => SqlStrTypeDefinitionParser.GetTypeSize(new SqlDataTypeReference(), null));
        }

        private int StubGetDefaultSize(string typeName)
        {
            return 777;
        }
    }
}
