using Microsoft.SqlServer.TransactSql.ScriptDom;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using TeamTools.TSQL.ExpressionEvaluator.Interfaces;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator.Values
{
    [Category("TeamTools.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(SqlLiteralValueFactory))]
    public sealed class SqlLiteralValueFactoryTests
    {
        private SqlLiteralValueFactory factory;
        private MockTypeResolver typeResolver;
        private MockValueFactory valueFactory;
        private MockTypeHandler typeHandler;

        [SetUp]
        public void SetUp()
        {
            valueFactory = new MockValueFactory();
            typeHandler = new MockTypeHandler(valueFactory);
            valueFactory.TypeHandler = typeHandler;
            typeResolver = new MockTypeResolver(typeHandler);
            factory = new SqlLiteralValueFactory(typeResolver);
        }

        [Test]
        public void Test_LiteralValueFactory_ConstuctorFailsIfBadArgs()
        {
            Assert.Throws(
                typeof(ArgumentNullException),
                () => new SqlLiteralValueFactory(null));
        }

        [Test]
        public void Test_LiteralValueFactory_ProducesStringValueFromStringLiteral()
        {
            var literal = new StringLiteral
            {
                Value = "dummy",
                IsNational = false,
            };

            var sqlValue = factory.Make(literal);

            Assert.That(sqlValue, Is.Not.Null);
            Assert.That(sqlValue, Is.InstanceOf<MockSqlValue>());

            var strValue = sqlValue as MockSqlValue;
            Assert.That(strValue.StrValue, Is.EqualTo(literal.Value));
            Assert.That(strValue.ValueKind, Is.EqualTo(SqlValueKind.Precise));
            Assert.That(strValue.SourceKind, Is.EqualTo(SqlValueSourceKind.Literal));

            Assert.That(strValue.TypeName, Is.EqualTo("VARCHAR"));
        }

        [Test]
        public void Test_LiteralValueFactory_ProducesUnicodeValueFromStringLiteral()
        {
            var literal = new StringLiteral
            {
                Value = "dummy",
                IsNational = true,
            };

            var sqlValue = factory.Make(literal);

            Assert.That(sqlValue, Is.Not.Null);
            Assert.That(sqlValue, Is.InstanceOf<MockSqlValue>());

            var strValue = sqlValue as MockSqlValue;
            Assert.That(strValue.StrValue, Is.EqualTo(literal.Value));
            Assert.That(strValue.ValueKind, Is.EqualTo(SqlValueKind.Precise));
            Assert.That(strValue.SourceKind, Is.EqualTo(SqlValueSourceKind.Literal));

            Assert.That(strValue.TypeName, Is.EqualTo("NVARCHAR"));
        }

        [Test]
        public void Test_LiteralValueFactory_ProducesIntValue_FromIntLiteral()
        {
            const int value = -123;
            var literal = new IntegerLiteral
            {
                Value = value.ToString(),
            };

            var sqlValue = factory.Make(literal);

            Assert.That(sqlValue, Is.Not.Null);
            Assert.That(sqlValue, Is.InstanceOf<MockSqlValue>());

            var intValue = sqlValue as MockSqlValue;
            Assert.That(intValue.IntValue, Is.EqualTo(value));
            Assert.That(intValue.ValueKind, Is.EqualTo(SqlValueKind.Precise));
            Assert.That(intValue.SourceKind, Is.EqualTo(SqlValueSourceKind.Literal));
            Assert.That(intValue.TypeName, Is.EqualTo("INT"));
        }

        [Test]
        public void Test_LiteralValueFactory_ProducesNullWithFallbackType()
        {
            var fragment = new SetVariableStatement();
            var sqlValue = factory.MakeNull(fragment);

            Assert.That(sqlValue, Is.Not.Null);
            Assert.That(sqlValue.IsNull, Is.True);
            Assert.That(sqlValue.Source?.Node, Is.Not.Null);
            Assert.That(sqlValue.Source.Node, Is.EqualTo(fragment));
            Assert.That(sqlValue.ValueKind, Is.EqualTo(SqlValueKind.Null));
            Assert.That(sqlValue.SourceKind, Is.EqualTo(SqlValueSourceKind.Literal));
            Assert.That(sqlValue.TypeName, Is.EqualTo("VARCHAR"));
        }

        [Test]
        public void Test_LiteralValueFactory_DoesNotFail_OnUnsupportedType()
        {
            var literal = new MaxLiteral
            {
                Value = "asdf",
            };

            Assert.DoesNotThrow(() => factory.Make(literal));
        }

        [Test]
        public void Test_LiteralValueFactory_ReturnsNull_OnUnsupportedType()
        {
            var literal = new MaxLiteral
            {
                Value = "asdf",
            };

            try
            {
                var res = factory.Make(literal);
                Assert.That(res, Is.Null);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        private class MockTypeResolver : ISqlTypeResolver
        {
            private static readonly List<string> Types
                = new List<string> { "VARCHAR", "NVARCHAR", "INT" };

            private readonly ISqlTypeHandler typeHandler;

            public MockTypeResolver(ISqlTypeHandler typeHandler)
            {
                this.typeHandler = typeHandler;
            }

            public bool IsSupportedType(string typeName)
            {
                return Types.Contains(typeName);
            }

            public SqlTypeReference ResolveType(DataTypeReference dataType)
            {
                return ResolveTypeHandler(dataType?.Name?.GetFullName())?
                    .MakeSqlDataTypeReference(dataType);
            }

            public SqlTypeReference ResolveType(string typeName)
            {
                return ResolveTypeHandler(typeName)?
                    .MakeSqlDataTypeReference(typeName);
            }

            public ISqlTypeHandler ResolveTypeHandler(string typeName)
            {
                if (IsSupportedType(typeName))
                {
                    return typeHandler;
                }

                return default;
            }
        }
    }
}
