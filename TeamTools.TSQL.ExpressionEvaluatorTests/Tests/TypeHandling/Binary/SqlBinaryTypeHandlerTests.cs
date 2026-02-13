using Microsoft.SqlServer.TransactSql.ScriptDom;
using NUnit.Framework;
using System;
using System.Numerics;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;
using TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.ExpressionEvaluatorTests.Tests.TypeHandling.Binary
{
    [Category("TeamTools.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(SqlBinaryTypeHandler))]
    public sealed class SqlBinaryTypeHandlerTests : BaseSqlTypeHandlerTestClass
    {
        private SqlBinaryTypeHandler typeHandler;

        private SqlBinaryTypeValueFactory ValueFactory => typeHandler.BinaryValueFactory;

        [OneTimeSetUp]
        public override void SetUp()
        {
            base.SetUp();
            typeHandler = new SqlBinaryTypeHandler(Converter, Violations);
        }

        [Test]
        public void Test_SqlBinaryTypeHandler_CanConvertFromValidStr()
        {
            var date = typeHandler.TypeConverter.ImplicitlyConvert<SqlBinaryTypeValue>(typeHandler.ConvertFrom(MakeStr("0x1F"), "BINARY"));

            Assert.That(date, Is.Not.Null);
            Assert.That(date.IsPreciseValue, Is.True);
            Assert.That(date.Value, Is.EqualTo(new HexValue(31)));
        }

        [Test]
        public void Test_SqlBinaryTypeHandler_ChangeToAppliesProvidedValue()
        {
            var bin = typeHandler.TypeConverter.ImplicitlyConvert<SqlBinaryTypeValue>(typeHandler.ConvertFrom(MakeStr("0x1F"), "BINARY"));

            Assert.That(bin, Is.Not.Null);

            bin = bin.ChangeTo(new HexValue(42), new SqlValueSource(SqlValueSourceKind.Expression, null));
            Assert.That(bin.Value, Is.EqualTo(new HexValue(42)));
        }

        [Test]
        public void Test_SqlBinaryTypeHandler_MakeSqlDataTypeReferenceFromBrokenDefinition()
        {
            var typeDef = new SqlDataTypeReference(); // no initialization
            try
            {
                var typeRef = typeHandler.MakeSqlDataTypeReference(typeDef);
                Assert.That(typeRef, Is.Null);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }

            try
            {
                var typeRef = typeHandler.MakeSqlDataTypeReference((DataTypeReference)null);
                Assert.That(typeRef, Is.Null);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void Test_SqlBinaryTypeHandler_MakeSqlDataTypeReferenceFromDefinition()
        {
            var typeDef = new SqlDataTypeReference
            {
                Name = new SchemaObjectName(),
            };

            typeDef.Name.Identifiers.Add(new Identifier { Value = "BINARY" });
            typeDef.Parameters.Add(new IntegerLiteral { Value = "700" });

            var typeRef = typeHandler.MakeSqlDataTypeReference(typeDef);
            Assert.That(typeRef, Is.Not.Null);
            Assert.That(typeRef.TypeName, Is.EqualTo("BINARY"));
            Assert.That(typeRef, Is.InstanceOf(typeof(SqlBinaryTypeReference)));
            Assert.That(((SqlBinaryTypeReference)typeRef).Size, Is.EqualTo(700));
        }

        [Test]
        public void Test_SqlBinaryTypeHandler_MakeSqlDataTypeReferenceFromName()
        {
            var typeRef = typeHandler.MakeSqlDataTypeReference("VARBINARY");
            Assert.That(typeRef, Is.Not.Null);
            Assert.That(typeRef.TypeName, Is.EqualTo("VARBINARY"));
            Assert.That(typeRef, Is.InstanceOf(typeof(SqlBinaryTypeReference)));
            Assert.That(((SqlBinaryTypeReference)typeRef).Size, Is.EqualTo(30));
        }

        [Test]
        public void Test_SqlBinaryTypeHandler_ConcatWorksSimilarToStrings()
        {
            var a = new SqlBinaryTypeValue(typeHandler, (SqlBinaryTypeReference)typeHandler.MakeSqlDataTypeReference("VARBINARY"), new HexValue("10"), new SqlValueSource(SqlValueSourceKind.Literal, default));
            var b = new SqlBinaryTypeValue(typeHandler, (SqlBinaryTypeReference)typeHandler.MakeSqlDataTypeReference("VARBINARY"), new HexValue("20"), new SqlValueSource(SqlValueSourceKind.Literal, default));
            var c = typeHandler.Sum(a, b);

            Assert.That(c, Is.Not.Null);
            Assert.That(c.TypeName, Is.EqualTo("VARBINARY"));
            Assert.That(c, Is.InstanceOf(typeof(SqlBinaryTypeValue)));

            var bin = (SqlBinaryTypeValue)c;
            Assert.That(bin.Value.AsString, Is.EqualTo("1020"));
            Assert.That(bin.Value.AsNumber, Is.EqualTo((BigInteger)4128));
        }

        [Test]
        public void Test_SqlBinaryTypeHandler_CanConvertFromInt()
        {
            var intHandler = new SqlIntTypeHandler(Converter, Violations);
            var a = new SqlIntTypeValue(intHandler, new SqlIntTypeReference("INT", new SqlIntValueRange(1, 1), intHandler.IntValueFactory), 1, new SqlValueSource(SqlValueSourceKind.Literal, default));
            var b = typeHandler.ConvertFrom(a, "BINARY");

            Assert.That(b, Is.Not.Null);
            Assert.That(b.TypeName, Is.EqualTo("BINARY"));
            Assert.That(b, Is.InstanceOf(typeof(SqlBinaryTypeValue)));

            var bin = (SqlBinaryTypeValue)b;
            Assert.That(bin.Value.AsString, Is.EqualTo("01"));
            Assert.That(bin.Value.AsNumber, Is.EqualTo((BigInteger)1));
        }

        [Test]
        public void Test_SqlBinaryTypeHandler_ConvertionFromIntUsesOriginalByteLength()
        {
            var intHandler = new SqlIntTypeHandler(Converter, Violations);
            var a = new SqlIntTypeValue(intHandler, new SqlIntTypeReference("INT", new SqlIntValueRange(1, 1), intHandler.IntValueFactory), 1, new SqlValueSource(SqlValueSourceKind.Literal, default));
            var b = typeHandler.ConvertFrom(a, "VARBINARY");

            Assert.That(b, Is.Not.Null);
            Assert.That(b.TypeName, Is.EqualTo("VARBINARY"));
            Assert.That(b, Is.InstanceOf(typeof(SqlBinaryTypeValue)));

            var bin = (SqlBinaryTypeValue)b;
            Assert.That(bin.Value.AsString, Is.EqualTo("01"));
            Assert.That(bin.Value.AsNumber, Is.EqualTo((BigInteger)1));
        }

        [Test]
        public void Test_SqlBinaryTypeHandler_ConvertionFromVariableToFixedLengthAppendsZeroes()
        {
            var src = (SqlBinaryTypeValue)ValueFactory.NewLiteral("VARBINARY", "0x00FF", default);
            var fixedBin = typeHandler.ConvertValueFrom(src, ValueFactory.MakeSqlDataTypeReference("BINARY", 4), true);
            var varBin = typeHandler.ConvertValueFrom(src, ValueFactory.MakeSqlDataTypeReference("VARBINARY", 4), true);

            // no additional padding
            Assert.That(varBin, Is.Not.Null);
            Assert.That(varBin.TypeName, Is.EqualTo("VARBINARY"));
            Assert.That(varBin.Value.AsString, Is.EqualTo("00FF"));

            // additional tail of zeroes
            Assert.That(fixedBin, Is.Not.Null);
            Assert.That(fixedBin.TypeName, Is.EqualTo("BINARY"));
            Assert.That(fixedBin.Value.AsString, Is.EqualTo("00FF0000"));
        }

        [Test]
        public void Test_SqlBinaryTypeHandler_ConvertionFromIntToFixedLengthPrependsZeroes()
        {
            var intHandler = new SqlIntTypeHandler(Converter, Violations);
            var number = new SqlIntTypeValue(intHandler, new SqlIntTypeReference("INT", new SqlIntValueRange(1, 1), intHandler.IntValueFactory), 1, new SqlValueSource(SqlValueSourceKind.Literal, default));

            var fixedBin = (SqlBinaryTypeValue)typeHandler.ConvertFrom(number, new SqlBinaryTypeReference("BINARY", 5, ValueFactory), true);

            Assert.That(fixedBin, Is.Not.Null);
            Assert.That(fixedBin.Value.AsNumber, Is.EqualTo((BigInteger)1));
            Assert.That(fixedBin.Value.AsString, Is.EqualTo("0000000001"));
        }

        [Test]
        public void Test_SqlBinaryTypeHandler_ConvertionFromIntToVariableLengthPrependsZeroesBasedOnSourceTypeSize()
        {
            var intHandler = new SqlIntTypeHandler(Converter, Violations);

            var number = new SqlIntTypeValue(intHandler, new SqlIntTypeReference("INT", new SqlIntValueRange(1, 1), intHandler.IntValueFactory), 1, new SqlValueSource(SqlValueSourceKind.Literal, default));
            var fixedBin = (SqlBinaryTypeValue)typeHandler.ConvertFrom(number, new SqlBinaryTypeReference("VARBINARY", 100, ValueFactory), true);

            Assert.That(fixedBin, Is.Not.Null);
            Assert.That(fixedBin.Value.AsNumber, Is.EqualTo((BigInteger)1));
            Assert.That(fixedBin.Value.AsString, Is.EqualTo("00000001")); // because INT is 4 bytes

            number = new SqlIntTypeValue(intHandler, new SqlIntTypeReference("SMALLINT", new SqlIntValueRange(1, 1), intHandler.IntValueFactory), 1, new SqlValueSource(SqlValueSourceKind.Literal, default));
            fixedBin = (SqlBinaryTypeValue)typeHandler.ConvertFrom(number, new SqlBinaryTypeReference("VARBINARY", 100, ValueFactory), true);

            Assert.That(fixedBin, Is.Not.Null);
            Assert.That(fixedBin.Value.AsNumber, Is.EqualTo((BigInteger)1));
            Assert.That(fixedBin.Value.AsString, Is.EqualTo("0001")); // because SMALLINT is 2 bytes
        }

        [Test]
        public void Test_SqlBinaryTypeHandler_ConvertionFromIntToVariableLengthTruncatesFromStart()
        {
            var intHandler = new SqlIntTypeHandler(Converter, Violations);

            var number = new SqlIntTypeValue(intHandler, new SqlIntTypeReference("INT", new SqlIntValueRange(1, 1), intHandler.IntValueFactory), 1, new SqlValueSource(SqlValueSourceKind.Literal, default));
            var fixedBin = (SqlBinaryTypeValue)typeHandler.ConvertFrom(number, new SqlBinaryTypeReference("VARBINARY", 1, ValueFactory), true);

            Assert.That(fixedBin, Is.Not.Null);
            Assert.That(fixedBin.Value.AsNumber, Is.EqualTo((BigInteger)1));
            Assert.That(fixedBin.Value.AsString, Is.EqualTo("01")); // nevertheless INT is 4 bytes, '1' is just one and can be stored in 1 byte
        }

        [Test]
        public void Test_SqlBinaryTypeHandler_ConvertionFromIntToFixedLengthTruncatesFromEnd()
        {
            var intHandler = new SqlIntTypeHandler(Converter, Violations);

            var number = new SqlIntTypeValue(intHandler, new SqlIntTypeReference("INT", new SqlIntValueRange(1, 1), intHandler.IntValueFactory), 43981, new SqlValueSource(SqlValueSourceKind.Literal, default));
            var fixedBin = (SqlBinaryTypeValue)typeHandler.ConvertFrom(number, new SqlBinaryTypeReference("BINARY", 1, ValueFactory), true);

            // 43981 == 0xABCD
            Assert.That(fixedBin, Is.Not.Null);
            Assert.That(fixedBin.Value.AsString, Is.EqualTo("AB")); // regular binary truncation cuts the end (just like strings)
        }
    }
}
