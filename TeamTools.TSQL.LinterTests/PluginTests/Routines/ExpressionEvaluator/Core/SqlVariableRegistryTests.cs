using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("Linter.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(SqlVariableRegistry))]
    public sealed class SqlVariableRegistryTests
    {
        private static readonly string MyVar = "@my_var";

        private MockTypeConverter converter;
        private ViolationReporter violations;
        private MockValueFactory valueFactory;
        private MockTypeHandler typeHandler;
        private SqlVariableRegistry varReg;
        private SqlIntTypeReference intRef;
        private MockSqlValue value;

        [SetUp]
        public void SetUp()
        {
            converter = new MockTypeConverter();
            violations = new ViolationReporter();
            valueFactory = new MockValueFactory();
            typeHandler = new MockTypeHandler(valueFactory);
            valueFactory.TypeHandler = new MockTypeHandler(valueFactory);

            varReg = new SqlVariableRegistry(converter, violations);
            intRef = new SqlIntTypeReference("dbo.INT", new SqlIntValueRange(0, 10), valueFactory);

            value = new MockSqlValue(
                intRef,
                SqlValueKind.Precise,
                new SqlValueSource(SqlValueSourceKind.Literal, null),
                typeHandler);

            varReg.RegisterVariable(MyVar, intRef);
        }

        [Test]
        public void Test_SqlVariableRegistry_Constructor_FailsOnNullArgs()
        {
            Assert.Throws(typeof(ArgumentNullException), () => new SqlVariableRegistry(converter, null));
            Assert.Throws(typeof(ArgumentNullException), () => new SqlVariableRegistry(null, violations));
        }

        [Test]
        public void Test_SqlVariableRegistry_RegisterEvaluatedValue_DoesNotFailOnNull()
        {
            Assert.DoesNotThrow(() => varReg.RegisterEvaluatedValue("", 1, value));
            Assert.DoesNotThrow(() => varReg.RegisterEvaluatedValue(MyVar, 1, null));
            Assert.DoesNotThrow(() => varReg.RegisterEvaluatedValue("@unregistered_var", 1, value));
            Assert.DoesNotThrow(() => varReg.RegisterEvaluatedValue("@some_var", -1, value));

            // unregistered var did not get registered
            Assert.That(varReg.Variables, Has.Count.EqualTo(1));
            // because of bad args no value was registered
            Assert.That(varReg.GetValueAt(MyVar, 1), Is.Null);
        }

        [Test]
        public void Test_SqlVariableRegistry_GetValueAt_DoesNotFail()
        {
            Assert.DoesNotThrow(() => varReg.GetValueAt(null, 123));
            Assert.DoesNotThrow(() => varReg.GetValueAt("dummy", 123));
            Assert.DoesNotThrow(() => varReg.GetValueAt("dummy", 123));
        }

        /* FIXME : register vars of any type first
        [Test]
        public void Test_SqlVariableRegistry_GetValueAt_RecordsViolationForUnregisteredVariable()
        {
            Assert.That(varReg.GetValueAt("@unknown_var", Is.Null, 1));
            Assert.That(violations.ViolationCount, Is.EqualTo(1));
            Assert.That(violations.Violations.First(), Is.InstanceOf<UnregisteredVariableViolation>());
        }
        */

        [Test]
        public void Test_SqlVariableRegistry_GetValueAt_ReturnsNearestValue()
        {
            var valueA = new MockSqlValue(intRef, SqlValueKind.Unknown, new SqlValueSource(SqlValueSourceKind.Literal, null), typeHandler);
            var valueB = new MockSqlValue(intRef, SqlValueKind.Unknown, new SqlValueSource(SqlValueSourceKind.Literal, null), typeHandler);

            varReg.RegisterEvaluatedValue(MyVar, 10, valueA);
            varReg.RegisterEvaluatedValue(MyVar, 20, valueB);

            var res = varReg.GetValueAt(MyVar, 9);
            Assert.That(res, Is.Null);

            res = varReg.GetValueAt(MyVar, 19);
            Assert.That(res, Is.Not.Null);
            Assert.That(valueA, Is.EqualTo(res));

            res = varReg.GetValueAt(MyVar, 20);
            Assert.That(res, Is.Not.Null);
            Assert.That(valueB, Is.EqualTo(res));

            res = varReg.GetValueAt(MyVar, 300);
            Assert.That(res, Is.Not.Null);
            Assert.That(valueB, Is.EqualTo(res));
        }

        [Test]
        public void Test_SqlVariableRegistry_GetValueAt_SetsSourceAsVar()
        {
            // value came from literal
            var valueA = new MockSqlValue(intRef, SqlValueKind.Unknown, new SqlValueSource(SqlValueSourceKind.Literal, null), typeHandler);
            // value came from expression
            var valueB = new MockSqlValue(intRef, SqlValueKind.Unknown, new SqlValueSource(SqlValueSourceKind.Expression, null), typeHandler);

            varReg.RegisterEvaluatedValue(MyVar, 10, valueA);
            varReg.RegisterEvaluatedValue(MyVar, 20, valueB);

            // now they are both Variable values
            var res = varReg.GetValueAt(MyVar, 11);
            Assert.That(res, Is.Not.Null);
            Assert.That(res.SourceKind, Is.EqualTo(SqlValueSourceKind.Variable));

            res = varReg.GetValueAt(MyVar, 20);
            Assert.That(res, Is.Not.Null);
            Assert.That(res.SourceKind, Is.EqualTo(SqlValueSourceKind.Variable));
        }

        [Test]
        public void Test_SqlVariableRegistry_ResetsAllModifiedInBlock()
        {
            // was precise
            var valueA = new MockSqlValue(intRef, SqlValueKind.Precise, new SqlValueSource(SqlValueSourceKind.Literal, null), typeHandler);

            varReg.RegisterEvaluatedValue(MyVar, 10, valueA);

            varReg.ResetEvaluatedValuesAfterBlock(9, 12, null);

            // and became unknown
            var res = varReg.GetValueAt(MyVar, 13);
            Assert.That(res, Is.Not.Null);
            Assert.That(res.ValueKind, Is.EqualTo(SqlValueKind.Unknown));

            // inside block it is still precise
            res = varReg.GetValueAt(MyVar, 11);
            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsPreciseValue, Is.True);
        }

        [Test]
        public void Test_SqlVariableRegistry_ResetIgnoresNotModifiedInBlock()
        {
            // was precise
            var valueA = new MockSqlValue(intRef, SqlValueKind.Precise, new SqlValueSource(SqlValueSourceKind.Literal, null), typeHandler);

            varReg.RegisterEvaluatedValue(MyVar, 10, valueA);

            // it was assigned not inside the block
            varReg.ResetEvaluatedValuesAfterBlock(12, 15, null);

            // and stays precise
            var res = varReg.GetValueAt(MyVar, 10);
            Assert.That(res, Is.Not.Null);
            Assert.That(res.ValueKind, Is.EqualTo(SqlValueKind.Precise));

            res = varReg.GetValueAt(MyVar, 13);
            Assert.That(res, Is.Not.Null);
            Assert.That(res.ValueKind, Is.EqualTo(SqlValueKind.Precise));

            res = varReg.GetValueAt(MyVar, 17);
            Assert.That(res, Is.Not.Null);
            Assert.That(res.ValueKind, Is.EqualTo(SqlValueKind.Precise));
        }

        [Test]
        public void Test_SqlVariableRegistry_IsVariableRegistered_DoesNotFailOnNull()
        {
            Assert.DoesNotThrow(() => varReg.IsVariableRegistered(null));
            Assert.That(varReg.IsVariableRegistered(null), Is.False);

            Assert.DoesNotThrow(() => varReg.IsVariableRegistered(""));
            Assert.That(varReg.IsVariableRegistered(""), Is.False);
        }

        [Test]
        public void Test_SqlVariableRegistry_RegisterVariable_FailsOnNullArg()
        {
            Assert.Throws(typeof(ArgumentNullException), () => varReg.RegisterVariable(null, intRef));
            Assert.Throws(typeof(ArgumentNullException), () => varReg.RegisterVariable("@var", null));
        }

        [Test]
        public void Test_SqlVariableRegistry_RegisterVariable_DoesNotFailIfVarRegistered()
        {
            Assert.DoesNotThrow(() => varReg.RegisterVariable(MyVar, intRef));
        }

        [Test]
        public void Test_SqlVariableRegistry_RegistersVariables()
        {
            varReg.RegisterVariable("@another_var", intRef);
            Assert.That(varReg.IsVariableRegistered("@another_var"), Is.True);

            var v = varReg.Variables["@another_var"];

            Assert.That(v, Is.Not.Null);
        }

        [Test]
        public void Test_SqlVariableRegistry_ReturnsVarTypeRef()
        {
            var typeRef = varReg.GetVariableTypeReference(MyVar);
            Assert.That(typeRef, Is.Not.Null);
            Assert.That(typeRef.TypeName, Is.EqualTo("dbo.INT"));
        }

        [Test]
        public void Test_SqlVariableRegistry_GetVariableTypeReferenceDoesNotFailOnBadVar()
        {
            Assert.DoesNotThrow(() => varReg.GetVariableTypeReference(""));
            Assert.That(varReg.GetVariableTypeReference(""), Is.Null);

            Assert.DoesNotThrow(() => varReg.GetVariableTypeReference(null));
            Assert.That(varReg.GetVariableTypeReference(null), Is.Null);

            Assert.DoesNotThrow(() => varReg.GetVariableTypeReference("@unknown@var"));
            Assert.That(varReg.GetVariableTypeReference("@unknown@var"), Is.Null);
        }

        private class MockTypeConverter : ISqlTypeConverter
        {
            public string EvaluateOutputType(params SqlValue[] values)
            {
                return EvaluateOutputType(values.ToList());
            }

            public string EvaluateOutputType(IList<SqlValue> values)
            {
                return values.FirstOrDefault()?.TypeName;
            }

            public SqlValue ExplicitlyConvertTo(string typeName, SqlValue from, Action<string> callback = null)
            {
                return from;
            }

            public SqlValue ExplicitlyConvertTo(SqlTypeReference typeReference, SqlValue from, Action<string> callback = null)
            {
                return from;
            }

            public T ImplicitlyConvert<T>(SqlValue from)
            where T : SqlValue
            {
                return from as T;
            }

            public SqlValue ImplicitlyConvertTo(string typeName, SqlValue from)
            {
                return from;
            }

            public SqlValue ImplicitlyConvertTo(SqlTypeReference typeReference, SqlValue from)
            {
                return from;
            }
        }
    }
}
