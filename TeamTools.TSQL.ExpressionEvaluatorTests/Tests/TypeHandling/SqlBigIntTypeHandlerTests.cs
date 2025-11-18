using Microsoft.SqlServer.TransactSql.ScriptDom;
using NUnit.Framework;
using System;
using System.Numerics;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator.TypeHandling
{
    [Category("TeamTools.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(SqlBigIntTypeHandler))]
    public sealed class SqlBigIntTypeHandlerTests : BaseSqlTypeHandlerTestClass
    {
        private SqlBigIntTypeHandler typeHandler;

        private SqlBigIntTypeValueFactory ValueFactory => typeHandler.BigIntValueFactory;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            typeHandler = new SqlBigIntTypeHandler(Converter, Violations);
        }

        [Test]
        public void Test_SqlBigIntTypeHandler_FailsOnNullArgs()
        {
            Assert.Throws(typeof(ArgumentNullException), () => new SqlBigIntTypeHandler(null, Violations));
            Assert.Throws(typeof(ArgumentNullException), () => new SqlBigIntTypeHandler(Converter, null));
        }

        [Test]
        public void Test_SqlBigIntTypeHandler_ReverseSignReturnsExpectedResults()
        {
            // precise value
            var srcValue = ValueFactory.MakePreciseValue(ValueFactory.FallbackTypeName, 123, default);

            // positive to negative
            var result = typeHandler.ReverseSign(srcValue);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<SqlBigIntTypeValue>());
            Assert.That(result.IsPreciseValue, Is.True);
            Assert.That(((SqlBigIntTypeValue)result).Value, Is.EqualTo((BigInteger)(-123)));

            // negative to positive
            srcValue = srcValue.ChangeTo(-123, default);
            Assert.That(srcValue.Value, Is.EqualTo((BigInteger)(-123)));

            result = typeHandler.ReverseSign(srcValue);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<SqlBigIntTypeValue>());
            Assert.That(result.IsPreciseValue, Is.True);
            Assert.That(((SqlBigIntTypeValue)result).Value, Is.EqualTo((BigInteger)123));
        }

        [Test]
        public void Test_SqlBigIntTypeHandler_ReverseSignRevertsRange()
        {
            // precise value
            var srcValue = ValueFactory.MakeApproximateValue(ValueFactory.FallbackTypeName, new SqlBigIntValueRange(-5, -1), default);

            // positive to negative
            var result = typeHandler.ReverseSign(srcValue);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<SqlBigIntTypeValue>());
            Assert.That(result.IsPreciseValue, Is.False);
            Assert.That(((SqlBigIntTypeValue)result).EstimatedSize.Low, Is.EqualTo((BigInteger)1));
            Assert.That(((SqlBigIntTypeValue)result).EstimatedSize.High, Is.EqualTo((BigInteger)5));

            // negative to positive
            srcValue = srcValue.ChangeTo(new SqlBigIntValueRange(150, 2000), default);
            Assert.That(srcValue.EstimatedSize.Low, Is.EqualTo((BigInteger)150));
            Assert.That(srcValue.EstimatedSize.High, Is.EqualTo((BigInteger)2000));

            result = typeHandler.ReverseSign(srcValue);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<SqlBigIntTypeValue>());
            Assert.That(result.IsPreciseValue, Is.False);
            Assert.That(((SqlBigIntTypeValue)result).EstimatedSize.Low, Is.EqualTo((BigInteger)(-2000)));
            Assert.That(((SqlBigIntTypeValue)result).EstimatedSize.High, Is.EqualTo((BigInteger)(-150)));
        }

        [Test]
        public void Test_SqlBigIntTypeHandler_ReverseSignForNullAndZeroReturnsOriginal()
        {
            // null
            var srcValue = ValueFactory.MakeNull(default);
            var result = typeHandler.ReverseSign(srcValue);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<SqlBigIntTypeValue>());
            Assert.That(result, Is.EqualTo(srcValue)); // same object
            Assert.That(result.IsNull, Is.True);

            // zero
            srcValue = ValueFactory.MakePreciseValue(ValueFactory.FallbackTypeName, 0, default);
            result = typeHandler.ReverseSign(srcValue);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<SqlBigIntTypeValue>());
            Assert.That(result, Is.EqualTo(srcValue)); // same object
            Assert.That(result.IsPreciseValue, Is.True);
        }

        [Test]
        public void Test_SqlBigIntTypeHandler_ReverseSign_DoesNotFailOnBadArgs()
        {
            Assert.DoesNotThrow(() => typeHandler.ReverseSign(null));
            Assert.That(typeHandler.ReverseSign(null), Is.Null);

            // some bullshit mock type
            var value = new MockSqlValue(new SqlStrTypeReference("CHAR", 1, ValueFactory), SqlValueKind.Unknown, default, typeHandler);
            Assert.DoesNotThrow(() => typeHandler.ReverseSign(value));
            Assert.That(typeHandler.ReverseSign(value), Is.Null);
        }

        [Test]
        public void Test_SqlBigIntTypeHandler_MakeSqlDataTypeReferenceSavesAllInfo()
        {
            var node = new SqlDataTypeReference { Name = new SchemaObjectName() };
            node.Name.Identifiers.Add(new Identifier { Value = "BIGINT" });

            var res = typeHandler.MakeSqlDataTypeReference(node);

            Assert.That(res, Is.Not.Null);
            Assert.That(res, Is.InstanceOf<SqlBigIntTypeReference>());
            Assert.That(res.TypeName, Is.EqualTo("BIGINT"));
            // TODO : take a look at type ref definition, fix range values
            Assert.That((res as SqlBigIntTypeReference).Size.Low, Is.LessThan((BigInteger)0));
            Assert.That((res as SqlBigIntTypeReference).Size.High, Is.GreaterThan((BigInteger)0));
        }
    }
}
