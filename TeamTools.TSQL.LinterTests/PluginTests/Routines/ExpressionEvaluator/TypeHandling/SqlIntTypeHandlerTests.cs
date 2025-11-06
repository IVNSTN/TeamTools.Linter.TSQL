using Microsoft.SqlServer.TransactSql.ScriptDom;
using NUnit.Framework;
using System;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator.TypeHandling
{
    [Category("Linter.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(SqlIntTypeHandler))]
    public sealed class SqlIntTypeHandlerTests : BaseSqlTypeHandlerTestClass
    {
        private SqlIntTypeHandler typeHandler;

        private SqlIntTypeValueFactory ValueFactory => typeHandler.IntValueFactory;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            typeHandler = new SqlIntTypeHandler(Converter, Violations);
        }

        [Test]
        public void Test_SqlIntTypeHandler_FailsOnNullArgs()
        {
            Assert.Throws(typeof(ArgumentNullException), () => new SqlIntTypeHandler(null, Violations));
            Assert.Throws(typeof(ArgumentNullException), () => new SqlIntTypeHandler(Converter, null));
        }

        [Test]
        public void Test_SqlIntTypeHandler_ReverseSignReturnsExpectedResults()
        {
            // precise value
            var srcValue = ValueFactory.MakePreciseValue(ValueFactory.FallbackTypeName, 123, default);

            // positive to negative
            var result = typeHandler.ReverseSign(srcValue);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<SqlIntTypeValue>());
            Assert.That(result.IsPreciseValue, Is.True);
            Assert.That(((SqlIntTypeValue)result).Value, Is.EqualTo(-123));

            // negative to positive
            srcValue = srcValue.ChangeTo(-123, default);
            Assert.That(srcValue.Value, Is.EqualTo(-123));

            result = typeHandler.ReverseSign(srcValue);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<SqlIntTypeValue>());
            Assert.That(result.IsPreciseValue, Is.True);
            Assert.That(((SqlIntTypeValue)result).Value, Is.EqualTo(123));
        }

        [Test]
        public void Test_SqlIntTypeHandler_ReverseSignRevertsRange()
        {
            // precise value
            var srcValue = ValueFactory.MakeApproximateValue(ValueFactory.FallbackTypeName, new SqlIntValueRange(-5, -1), default);

            // positive to negative
            var result = typeHandler.ReverseSign(srcValue);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<SqlIntTypeValue>());
            Assert.That(result.IsPreciseValue, Is.False);
            Assert.That(((SqlIntTypeValue)result).EstimatedSize.Low, Is.EqualTo(1));
            Assert.That(((SqlIntTypeValue)result).EstimatedSize.High, Is.EqualTo(5));

            // negative to positive
            srcValue = srcValue.ChangeTo(new SqlIntValueRange(150, 2000), default);
            Assert.That(srcValue.EstimatedSize.Low, Is.EqualTo(150));
            Assert.That(srcValue.EstimatedSize.High, Is.EqualTo(2000));

            result = typeHandler.ReverseSign(srcValue);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<SqlIntTypeValue>());
            Assert.That(result.IsPreciseValue, Is.False);
            Assert.That(((SqlIntTypeValue)result).EstimatedSize.Low, Is.EqualTo(-2000));
            Assert.That(((SqlIntTypeValue)result).EstimatedSize.High, Is.EqualTo(-150));
        }

        [Test]
        public void Test_SqlIntTypeHandler_ReverseSignForNullAndZeroReturnsOriginal()
        {
            // null
            var srcValue = ValueFactory.MakeNull(default);
            var result = typeHandler.ReverseSign(srcValue);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<SqlIntTypeValue>());
            Assert.That(result, Is.EqualTo(srcValue)); // same object
            Assert.That(result.IsNull, Is.True);

            // zero
            srcValue = ValueFactory.MakePreciseValue(ValueFactory.FallbackTypeName, 0, default);
            result = typeHandler.ReverseSign(srcValue);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<SqlIntTypeValue>());
            Assert.That(result, Is.EqualTo(srcValue)); // same object
            Assert.That(result.IsPreciseValue, Is.True);
        }

        [Test]
        public void Test_SqlIntTypeHandler_ReverseSign_DoesNotFailOnBadArgs()
        {
            Assert.DoesNotThrow(() => typeHandler.ReverseSign((SqlIntTypeValue)null));
            Assert.That(typeHandler.ReverseSign((SqlIntTypeValue)null), Is.Null);

            // some bullshit mock type
            var value = new MockSqlValue(new SqlStrTypeReference("dbo.CHAR", 1, ValueFactory), SqlValueKind.Unknown, default, typeHandler);
            Assert.DoesNotThrow(() => typeHandler.ReverseSign(value));
            Assert.That(typeHandler.ReverseSign(value), Is.Null);
        }

        [Test]
        public void Test_SqlStrTypeHandler_MakeSqlDataTypeReferenceSavesAllInfo()
        {
            var node = new SqlDataTypeReference { Name = new SchemaObjectName() };
            node.Name.Identifiers.Add(new Identifier { Value = "TINYINT" });

            var res = typeHandler.MakeSqlDataTypeReference(node);

            Assert.That(res, Is.Not.Null);
            Assert.That(res, Is.InstanceOf<SqlIntTypeReference>());
            Assert.That(res.TypeName, Is.EqualTo("dbo.TINYINT"));
            Assert.That((res as SqlIntTypeReference).Size, Is.EqualTo(new SqlIntValueRange(0, 255)));
        }

        [Test]
        public void Test_SqlIntTypeHandler_MergeTakesWidestRange()
        {
            var a = typeHandler.IntValueFactory.MakeApproximateValue("dbo.SMALLINT", new SqlIntValueRange(-5, -1), null);
            var b = typeHandler.IntValueFactory.MakeApproximateValue("dbo.INT", new SqlIntValueRange(12, 50), null);

            var c = typeHandler.MergeEstimation(a, b);

            Assert.That(c, Is.Not.Null);
            Assert.That(c.IsPreciseValue, Is.False);
            Assert.That(c.EstimatedSize.High, Is.EqualTo(50));
            Assert.That(c.EstimatedSize.Low, Is.EqualTo(-5));
        }
    }
}
