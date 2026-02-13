using Microsoft.SqlServer.TransactSql.ScriptDom;
using NUnit.Framework;
using System;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;
using TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.ExpressionEvaluatorTests.Tests.TypeHandling.Decimal
{
    [Category("TeamTools.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(SqlDecimalTypeHandler))]
    public sealed class SqlDecimalTypeHandlerTests : BaseSqlTypeHandlerTestClass
    {
        private SqlDecimalTypeHandler typeHandler;

        private SqlDecimalTypeValueFactory ValueFactory => typeHandler.DecimalValueFactory;

        [OneTimeSetUp]
        public override void SetUp()
        {
            base.SetUp();
            typeHandler = new SqlDecimalTypeHandler(Converter, Violations);
        }

        [Test]
        public void Test_SqlDecimalTypeHandler_CanConvertFromValidStr()
        {
            var date = typeHandler.TypeConverter.ImplicitlyConvert<SqlDecimalTypeValue>(typeHandler.ConvertFrom(MakeStr("123.4567"), "DECIMAL"));

            Assert.That(date, Is.Not.Null);
            Assert.That(date.IsPreciseValue, Is.True);
            Assert.That(date.Value, Is.EqualTo(123.4567m));
        }

        [Test]
        public void Test_SqlDecimalTypeHandler_ChangeToAppliesProvidedValue()
        {
            var dec = typeHandler.TypeConverter.ImplicitlyConvert<SqlDecimalTypeValue>(typeHandler.ConvertFrom(MakeStr("123.4567"), "DECIMAL"));

            Assert.That(dec, Is.Not.Null);

            dec = dec.ChangeTo(200.002m, new SqlValueSource(SqlValueSourceKind.Expression, null));
            Assert.That(dec.Value, Is.EqualTo(200.002m));
        }

        [Test]
        public void Test_SqlDecimalTypeHandler_MakeSqlDataTypeReferenceFromBrokenDefinition()
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
        public void Test_SqlDecimalTypeHandler_MakeSqlDataTypeReferenceFromDefinition()
        {
            var typeDef = new SqlDataTypeReference
            {
                Name = new SchemaObjectName(),
            };

            typeDef.Name.Identifiers.Add(new Identifier { Value = "DECIMAL" });
            typeDef.Parameters.Add(new IntegerLiteral { Value = "12" });
            typeDef.Parameters.Add(new IntegerLiteral { Value = "5" });

            var typeRef = typeHandler.MakeSqlDataTypeReference(typeDef);
            Assert.That(typeRef, Is.Not.Null);
            Assert.That(typeRef.TypeName, Is.EqualTo("DECIMAL"));
            Assert.That(typeRef, Is.InstanceOf(typeof(SqlDecimalTypeReference)));
            Assert.That(((SqlDecimalTypeReference)typeRef).Size.Precision, Is.EqualTo(12));
            Assert.That(((SqlDecimalTypeReference)typeRef).Size.Scale, Is.EqualTo(5));
        }

        [Test]
        public void Test_SqlDecimalTypeHandler_MakeSqlDataTypeReferenceFromName()
        {
            var typeRef = typeHandler.MakeSqlDataTypeReference("NUMERIC");
            Assert.That(typeRef, Is.Not.Null);
            Assert.That(typeRef.TypeName, Is.EqualTo("NUMERIC"));
            Assert.That(typeRef, Is.InstanceOf(typeof(SqlDecimalTypeReference)));
            Assert.That(((SqlDecimalTypeReference)typeRef).Size.Precision, Is.EqualTo(18));
            Assert.That(((SqlDecimalTypeReference)typeRef).Size.Scale, Is.EqualTo(0));
        }

        [Test]
        public void Test_SqlDecimalTypeHandler_Sum()
        {
            var a = new SqlDecimalTypeValue(typeHandler, (SqlDecimalTypeReference)typeHandler.MakeSqlDataTypeReference("NUMERIC"), 1.9m, new SqlValueSource(SqlValueSourceKind.Literal, default));
            var b = new SqlDecimalTypeValue(typeHandler, (SqlDecimalTypeReference)typeHandler.MakeSqlDataTypeReference("NUMERIC"), 2.5m, new SqlValueSource(SqlValueSourceKind.Literal, default));
            var c = typeHandler.Sum(a, b);

            Assert.That(c, Is.Not.Null);
            Assert.That(c.TypeName, Is.EqualTo("NUMERIC"));
            Assert.That(c, Is.InstanceOf(typeof(SqlDecimalTypeValue)));

            var dec = (SqlDecimalTypeValue)c;
            Assert.That(dec.Value, Is.EqualTo(4.4m));
        }

        [Test]
        public void Test_SqlDecimalTypeHandler_Sum_OfNullReturnsNull()
        {
            var a = ValueFactory.NewNull(default);
            Assert.That(a, Is.Not.Null);

            var b = new SqlDecimalTypeValue(typeHandler, (SqlDecimalTypeReference)typeHandler.MakeSqlDataTypeReference("NUMERIC"), 2.5m, new SqlValueSource(SqlValueSourceKind.Literal, default));
            var c = typeHandler.Sum(a, b);

            Assert.That(c, Is.Not.Null);
            Assert.That(c.IsNull, Is.True);
        }

        [Test]
        public void Test_SqlDecimalTypeHandler_Divide()
        {
            var a = new SqlDecimalTypeValue(typeHandler, (SqlDecimalTypeReference)typeHandler.MakeSqlDataTypeReference("NUMERIC"), 121.121m, new SqlValueSource(SqlValueSourceKind.Literal, default));
            var b = new SqlDecimalTypeValue(typeHandler, (SqlDecimalTypeReference)typeHandler.MakeSqlDataTypeReference("NUMERIC"), 11m, new SqlValueSource(SqlValueSourceKind.Literal, default));
            var c = typeHandler.Divide(a, b);

            Assert.That(c, Is.Not.Null);
            Assert.That(c.TypeName, Is.EqualTo("NUMERIC"));
            Assert.That(c, Is.InstanceOf(typeof(SqlDecimalTypeValue)));

            var dec = (SqlDecimalTypeValue)c;
            Assert.That(dec.Value, Is.EqualTo(11.011m));
        }

        [Test]
        public void Test_SqlDecimalTypeHandler_Multiply()
        {
            var a = new SqlDecimalTypeValue(typeHandler, (SqlDecimalTypeReference)typeHandler.MakeSqlDataTypeReference("NUMERIC"), 5.5m, new SqlValueSource(SqlValueSourceKind.Literal, default));
            var b = new SqlDecimalTypeValue(typeHandler, (SqlDecimalTypeReference)typeHandler.MakeSqlDataTypeReference("NUMERIC"), 2.2m, new SqlValueSource(SqlValueSourceKind.Literal, default));
            var c = typeHandler.Multiply(a, b);

            Assert.That(c, Is.Not.Null);
            Assert.That(c.TypeName, Is.EqualTo("NUMERIC"));
            Assert.That(c, Is.InstanceOf(typeof(SqlDecimalTypeValue)));

            var dec = (SqlDecimalTypeValue)c;
            Assert.That(dec.Value, Is.EqualTo(12.1m));
        }

        [Test]
        public void Test_SqlDecimalTypeHandler_Subtract()
        {
            var a = new SqlDecimalTypeValue(typeHandler, (SqlDecimalTypeReference)typeHandler.MakeSqlDataTypeReference("NUMERIC"), 5.5m, new SqlValueSource(SqlValueSourceKind.Literal, default));
            var b = new SqlDecimalTypeValue(typeHandler, (SqlDecimalTypeReference)typeHandler.MakeSqlDataTypeReference("NUMERIC"), 2.2m, new SqlValueSource(SqlValueSourceKind.Literal, default));
            var c = typeHandler.Subtract(a, b);

            Assert.That(c, Is.Not.Null);
            Assert.That(c.TypeName, Is.EqualTo("NUMERIC"));
            Assert.That(c, Is.InstanceOf(typeof(SqlDecimalTypeValue)));

            var dec = (SqlDecimalTypeValue)c;
            Assert.That(dec.Value, Is.EqualTo(3.3m));
        }

        [Test]
        public void Test_SqlDecimalTypeHandler_CombineSize()
        {
            var a = new SqlDecimalValueRange(-100, 100, 5, 3);
            var b = new SqlDecimalValueRange(-200, 200, 15, 10);
            var c = typeHandler.CombineSize(a, b);

            Assert.That(c, Is.Not.Null);
            Assert.That(c.Low, Is.EqualTo(-200m));
            Assert.That(c.High, Is.EqualTo(200m));
            Assert.That(c.Precision, Is.EqualTo(15));
            Assert.That(c.Scale, Is.EqualTo(10));
        }

        [Test]
        public void Test_SqlDecimalTypeHandler_RevertSign_Precise()
        {
            var a = new SqlDecimalTypeValue(typeHandler, (SqlDecimalTypeReference)typeHandler.MakeSqlDataTypeReference("NUMERIC"), 123.456m, new SqlValueSource(SqlValueSourceKind.Literal, default));
            var c = typeHandler.ReverseSign(a);

            Assert.That(c, Is.Not.Null);
            Assert.That(c.TypeName, Is.EqualTo("NUMERIC"));
            Assert.That(c, Is.InstanceOf(typeof(SqlDecimalTypeValue)));

            var dec = (SqlDecimalTypeValue)c;
            Assert.That(dec.IsPreciseValue, Is.True);
            Assert.That(dec.Value, Is.EqualTo(-123.456m));
        }

        [Test]
        public void Test_SqlDecimalTypeHandler_RevertSign_Aproximate()
        {
            var a = ValueFactory.MakeApproximateValue("DECIMAL", new SqlDecimalValueRange(-222, 111, 18, 2), new SqlValueSource(SqlValueSourceKind.Expression, default));
            Assert.That(a.EstimatedSize.Low, Is.EqualTo(-222m));
            Assert.That(a.EstimatedSize.High, Is.EqualTo(111m));
            Assert.That(a.EstimatedSize.Precision, Is.EqualTo(18));
            Assert.That(a.EstimatedSize.Scale, Is.EqualTo(2));

            var c = typeHandler.ReverseSign(a);

            Assert.That(c, Is.Not.Null);
            Assert.That(c.TypeName, Is.EqualTo("DECIMAL"));
            Assert.That(c, Is.InstanceOf(typeof(SqlDecimalTypeValue)));

            var dec = (SqlDecimalTypeValue)c;
            Assert.That(dec.IsPreciseValue, Is.False);
            Assert.That(dec.EstimatedSize.Low, Is.EqualTo(-111m));
            Assert.That(dec.EstimatedSize.High, Is.EqualTo(222m));
            Assert.That(dec.EstimatedSize.Precision, Is.EqualTo(18));
            Assert.That(dec.EstimatedSize.Scale, Is.EqualTo(2));
        }

        [Test]
        public void Test_SqlDecimalTypeHandler_ReverseSign_Null()
        {
            var a = ValueFactory.NewNull(default);
            Assert.That(a, Is.Not.Null);

            var c = typeHandler.ReverseSign(a);

            Assert.That(c, Is.Not.Null);
            Assert.That(c.IsNull, Is.True);
        }

        [Test]
        public void Test_SqlDecimalTypeHandler_CanConvertFromInt()
        {
            var intHandler = new SqlIntTypeHandler(Converter, Violations);
            var a = new SqlIntTypeValue(intHandler, new SqlIntTypeReference("INT", new SqlIntValueRange(1, 1), intHandler.IntValueFactory), 1, new SqlValueSource(SqlValueSourceKind.Literal, default));
            var b = typeHandler.ConvertFrom(a, "DECIMAL");

            Assert.That(b, Is.Not.Null);
            Assert.That(b.TypeName, Is.EqualTo("DECIMAL"));
            Assert.That(b, Is.InstanceOf(typeof(SqlDecimalTypeValue)));

            var dec = (SqlDecimalTypeValue)b;
            Assert.That(dec.Value, Is.EqualTo(1m));
        }

        [Test]
        public void Test_SqlDecimalTypeHandler_ConvertionFromVariableToLessScaleTruncates()
        {
            var src = (SqlDecimalTypeValue)ValueFactory.NewLiteral("DECIMAL", "654.321", default);
            Assert.That(src.Value, Is.EqualTo(654.321m));

            // no scale
            var dec = typeHandler.ConvertValueFrom(src, ValueFactory.MakeSqlDataTypeReference("DECIMAL", 18, 0), true);

            Assert.That(dec, Is.Not.Null);
            Assert.That(dec.TypeName, Is.EqualTo("DECIMAL"));
            Assert.That(dec.Value, Is.EqualTo(654m));

            // scale = 1
            dec = typeHandler.ConvertValueFrom(src, ValueFactory.MakeSqlDataTypeReference("DECIMAL", 18, 1), true);

            Assert.That(dec, Is.Not.Null);
            Assert.That(dec.Value, Is.EqualTo(654.3m));

            // scale = 2
            dec = typeHandler.ConvertValueFrom(src, ValueFactory.MakeSqlDataTypeReference("DECIMAL", 18, 2), true);

            Assert.That(dec, Is.Not.Null);
            Assert.That(dec.Value, Is.EqualTo(654.32m));

            // scale = 3
            dec = typeHandler.ConvertValueFrom(src, ValueFactory.MakeSqlDataTypeReference("DECIMAL", 18, 3), true);

            Assert.That(dec, Is.Not.Null);
            Assert.That(dec.Value, Is.EqualTo(654.321m));
        }
    }
}
