using Microsoft.SqlServer.TransactSql.ScriptDom;
using NUnit.Framework;
using System;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator.TypeHandling
{
    [Category("Linter.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(SqlStrTypeHandler))]
    public sealed class SqlStrTypeHandlerTests : BaseSqlTypeHandlerTestClass
    {
        private SqlStrTypeHandler typeHandler;

        private SqlStrTypeValueFactory ValueFactory => typeHandler.StrValueFactory;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            typeHandler = new SqlStrTypeHandler(Converter, Violations);
        }

        [Test]
        public void Test_SqlStrTypeHandler_FailsOnNullArgs()
        {
            Assert.Throws(typeof(ArgumentNullException), () => new SqlStrTypeHandler(null, Violations));
            Assert.Throws(typeof(ArgumentNullException), () => new SqlStrTypeHandler(Converter, null));
        }

        [Test]
        public void Test_SqlStrTypeHandler_SumReturnsConcatenatedStrings()
        {
            var a = new SqlStrTypeValue(typeHandler, new SqlStrTypeReference("dbo.VARCHAR", 100, typeHandler.GetValueFactory()), "ABC-", default);

            var b = new SqlStrTypeValue(typeHandler, new SqlStrTypeReference("dbo.VARCHAR", 100, typeHandler.GetValueFactory()), "-DEF", default);

            var c = typeHandler.Sum(a, b);

            Assert.That(c, Is.Not.Null);
            Assert.That(c, Is.InstanceOf<SqlStrTypeValue>());
            Assert.That(c.IsPreciseValue, Is.True);
            Assert.That((c as SqlStrTypeValue).Value, Is.EqualTo("ABC--DEF"));
            Assert.That((c as SqlStrTypeValue).EstimatedSize, Is.EqualTo(8));
        }

        [Test]
        public void Test_SqlStrTypeHandler_MakeSqlDataTypeReferenceSavesAllInfo()
        {
            var node = new SqlDataTypeReference { Name = new SchemaObjectName() };
            node.Name.Identifiers.Add(new Identifier { Value = "NCHAR" });
            node.Parameters.Add(new IntegerLiteral { Value = "256" });

            var res = typeHandler.MakeSqlDataTypeReference(node);

            Assert.That(res, Is.Not.Null);
            Assert.That(res, Is.InstanceOf<SqlStrTypeReference>());
            Assert.That(res.TypeName, Is.EqualTo("dbo.NCHAR"));
            Assert.That((res as SqlStrTypeReference).Size, Is.EqualTo(256));
        }

        [Test]
        public void Test_SqlStrTypeHandler_MakeSqlDataTypeReferenceDoesNotFailOnNull()
        {
            Assert.DoesNotThrow(() => typeHandler.MakeSqlDataTypeReference((SqlDataTypeReference)null));
            Assert.That(typeHandler.MakeSqlDataTypeReference((SqlDataTypeReference)null), Is.Null);

            Assert.DoesNotThrow(() => typeHandler.MakeSqlDataTypeReference(""));
            Assert.That(typeHandler.MakeSqlDataTypeReference(""), Is.Null);
        }

        [Test]
        public void Test_SqlStrTypeHandler_MakeSqlDataTypeReferenceFromStringUsesDefaultSize()
        {
            // char
            var res = typeHandler.MakeSqlDataTypeReference("dbo.CHAR");

            Assert.That(res, Is.Not.Null);
            Assert.That(res, Is.InstanceOf<SqlStrTypeReference>());
            Assert.That(res.TypeName, Is.EqualTo("dbo.CHAR"));
            // 1 is the default size by design for SQL string types
            Assert.That((res as SqlStrTypeReference).Size, Is.EqualTo(1));

            // varchar
            res = typeHandler.MakeSqlDataTypeReference("dbo.VARCHAR");

            Assert.That(res, Is.Not.Null);
            Assert.That(res, Is.InstanceOf<SqlStrTypeReference>());
            Assert.That(res.TypeName, Is.EqualTo("dbo.VARCHAR"));
            // 30 is the default size by design for SQL string types
            Assert.That((res as SqlStrTypeReference).Size, Is.EqualTo(30));
        }

        [Test]
        public void Test_SqlStrTypeHandler_MergeTakesBiggestLength()
        {
            var a = typeHandler.StrValueFactory.MakeApproximateValue("dbo.VARCHAR", 12, null);
            var b = typeHandler.StrValueFactory.MakeApproximateValue("dbo.VARCHAR", 28, null);

            var c = typeHandler.MergeEstimation(a, b);

            Assert.That(c, Is.Not.Null);
            Assert.That(c.IsPreciseValue, Is.False);
            Assert.That(c.EstimatedSize, Is.EqualTo(28));
        }
    }
}
