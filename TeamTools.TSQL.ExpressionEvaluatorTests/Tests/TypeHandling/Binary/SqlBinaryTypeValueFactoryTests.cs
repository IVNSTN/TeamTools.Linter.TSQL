using Microsoft.SqlServer.TransactSql.ScriptDom;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using TeamTools.TSQL.ExpressionEvaluator;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;
using TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.ExpressionEvaluatorTests.Tests.TypeHandling.Binary
{
    [Category("TeamTools.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(SqlBinaryTypeValueFactory))]
    public sealed class SqlBinaryTypeValueFactoryTests : BaseSqlTypeHandlerTestClass
    {
        private SqlBinaryTypeHandler typeHandler;

        private SqlBinaryTypeValueFactory ValueFactory => typeHandler.BinaryValueFactory;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            typeHandler = new SqlBinaryTypeHandler(Converter, Violations);
        }

        [Test]
        public void Test_SqlBinaryTypeValue_DeepCloneCopiesPreciseValue()
        {
            var value = ValueFactory.MakePreciseValue("BINARY", new HexValue(34), new SqlValueSource(SqlValueSourceKind.Literal, null));
            Assert.That(value, Is.Not.Null);
            Assert.That(value.Value.AsNumber, Is.EqualTo((BigInteger)34));

            var clone = value.DeepClone();
            Assert.That(clone, Is.Not.Null);
            Assert.That(clone.IsPreciseValue, Is.True);
            Assert.That(clone.Value, Is.EqualTo(value.Value));
            Assert.That(clone.Value.ToString(), Is.EqualTo("0x22"));
            Assert.That(clone.TypeName, Is.EqualTo("BINARY"));
        }

        [Test]
        public void Test_SqlBinaryTypeValueFactory_NewLiteral()
        {
            var value = ValueFactory.NewLiteral("BINARY", "0xFA0031", default);

            Assert.That(value, Is.Not.Null);
            Assert.That(value.TypeName, Is.EqualTo("BINARY"));
        }

        [Test]
        public void Test_SqlBinaryTypeValueFactory_LiteralsParsedWithRespectToDBEnginePeculiarities()
        {
            const string script = @"
                DECLARE @a VARBINARY(3)  = CAST(1 AS BIGINT)
                DECLARE @b BINARY(1)     = 1
                DECLARE @c VARBINARY(10) = 1
                DECLARE @d BINARY(10)    = 1
                DECLARE @e VARBINARY(10) = CAST(1 AS BIGINT)

                PRINT @a
                PRINT @b
                PRINT @c
                PRINT @d
                PRINT @e

                --- expected result:
                -- 0x000001
                -- 0x01
                -- 0x00000001
                -- 0x00000000000000000001
                -- 0x0000000000000001
           ";

            Dictionary<string, string> expectedResults = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "@a", "0x000001" },
                { "@b", "0x01" },
                { "@c", "0x00000001" },
                { "@d", "0x00000000000000000001" },
                { "@e", "0x0000000000000001" },
            };

            var dom = TSqlParser.CreateParser(SqlVersion.Sql150, true);
            using var reader = new StringReader(script);
            var sql = (TSqlScript)dom.Parse(reader, out var errors);

            Assert.That(errors, Is.Empty);
            Assert.That(sql.Batches.Count, Is.EqualTo(1));

            var eval = new ScalarExpressionEvaluator(sql.Batches[0]);

            foreach (var v in expectedResults)
            {
                var value = eval.GetValueAt(v.Key, sql.LastTokenIndex);
                Assert.That(value, Is.Not.Null, v.Key);
                Assert.That(value, Is.InstanceOf(typeof(SqlBinaryTypeValue)));

                var bin = ((SqlBinaryTypeValue)value).Value;
                Assert.That(bin, Is.Not.Null, v.Key);
                Assert.That(bin.ToString(), Is.EqualTo(v.Value), v.Key);
            }
        }

        [Test]
        public void Test_SqlBinaryTypeValueFactory_MakeMethodsDontFailForUnsupportedType()
        {
            CallMakeMethodsWith("dummy");
            CallMakeMethodsWith("");
            CallMakeMethodsWith(null);
        }

        [Test]
        public void Test_SqlBinaryTypeValueFactory_MakeThrowsForNull()
        {
            Assert.Throws(typeof(ArgumentNullException), () => ValueFactory.MakeLiteral("BINARY", null, null));
        }

        [Test]
        public void Test_SqlBinaryTypeValueFactory_MakeReturnsUnknownForEmptyString()
        {
            Assert.DoesNotThrow(() => ValueFactory.NewLiteral("BINARY", "", null));
            var value = ValueFactory.NewLiteral("BINARY", "", null);

            Assert.That(value, Is.Not.Null);
            Assert.That(value.ValueKind, Is.EqualTo(SqlValueKind.Unknown));
        }

        [Test]
        public void Test_SqlBinaryTypeValueFactory_MakeNull()
        {
            var value = ValueFactory.MakeNullValue("BINARY", null);
            Assert.That(value, Is.Not.Null);
            Assert.That(value.IsNull, Is.True);

            var v2 = ValueFactory.NewNull(null);
            Assert.That(v2, Is.Not.Null);
            Assert.That(v2.IsNull, Is.True);
        }

        private void CallMakeMethodsWith(string dummyType)
        {
            Assert.DoesNotThrow(() => ValueFactory.MakeApproximateValue(dummyType, default, default));
            Assert.That(ValueFactory.MakeApproximateValue(dummyType, default, default), Is.Null);

            Assert.DoesNotThrow(() => ValueFactory.MakeLiteral(dummyType, default, default));
            Assert.That(ValueFactory.MakeLiteral(dummyType, default, default), Is.Null);

            Assert.DoesNotThrow(() => ValueFactory.MakeNullValue(dummyType, default));
            Assert.That(ValueFactory.MakeNullValue(dummyType, default), Is.Null);

            Assert.DoesNotThrow(() => ValueFactory.MakePreciseValue(dummyType, default, default));
            Assert.That(ValueFactory.MakePreciseValue(dummyType, default, default), Is.Null);

            Assert.DoesNotThrow(() => ValueFactory.MakeUnknownValue(dummyType));
            Assert.That(ValueFactory.MakeUnknownValue(dummyType), Is.Null);

            Assert.DoesNotThrow(() => ValueFactory.MakeSqlDataTypeReference(dummyType, 0));
            Assert.That(ValueFactory.MakeSqlDataTypeReference(dummyType, 1), Is.Null);

            Assert.DoesNotThrow(() => ValueFactory.NewLiteral(dummyType, "123", default));
            Assert.That(ValueFactory.NewLiteral(dummyType, "123", default), Is.Null);
        }
    }
}
