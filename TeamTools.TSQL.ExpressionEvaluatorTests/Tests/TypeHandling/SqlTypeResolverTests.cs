using Microsoft.SqlServer.TransactSql.ScriptDom;
using NUnit.Framework;
using System;
using System.Linq;
using TeamTools.TSQL.ExpressionEvaluator.Core;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator.TypeHandling
{
    [Category("TeamTools.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(SqlTypeResolver))]
    public sealed class SqlTypeResolverTests
    {
        private SqlTypeResolver typeResolver;
        private MockTypeHandler typeHandler;

        [SetUp]
        public void SetUp()
        {
            typeResolver = new SqlTypeResolver();
            typeHandler = new MockTypeHandler(new MockValueFactory());
            typeResolver.RegisterTypeHandler(typeHandler);
        }

        [Test]
        public void Test_SqlTypeResolver_FailsOnTypeCollision()
        {
            // trying to push another handler with the same supported type list
            Assert.Throws(
                typeof(ArgumentException),
                () => typeResolver.RegisterTypeHandler(new MockTypeHandler(new MockValueFactory())));
        }

        [Test]
        public void Test_SqlTypeResolver_RegisterFailsOnNullArg()
        {
            Assert.Throws(typeof(ArgumentNullException), () => typeResolver.RegisterTypeHandler(null));
        }

        [Test]
        public void Test_SqlTypeResolver_RegistersAllProvidedTypes()
        {
            var typeList = typeHandler.SupportedTypes.ToList();

            Assert.That(typeList, Is.Not.Empty);

            foreach (var t in typeList)
            {
                Assert.That(typeResolver.IsSupportedType(t), Is.True, t);
            }
        }

        [Test]
        public void Test_SqlTypeResolver_ResolveType_DoesNotFailOnEmptyTypeName()
        {
            // trying to push another handler with the same supported type list
            Assert.DoesNotThrow(() => typeResolver.ResolveTypeHandler(null));
            Assert.DoesNotThrow(() => typeResolver.ResolveType(""));
            Assert.DoesNotThrow(() => typeResolver.ResolveType((DataTypeReference)null));

            Assert.That(typeResolver.ResolveTypeHandler(null), Is.Null);
            Assert.That(typeResolver.ResolveType(""), Is.Null);
            Assert.That(typeResolver.ResolveType((DataTypeReference)null), Is.Null);
        }

        [Test]
        public void Test_SqlTypeResolver_ResolveType_ReturnsNullForUnknownType()
        {
            Assert.DoesNotThrow(() => typeResolver.ResolveTypeHandler("unknown-type"));
            Assert.DoesNotThrow(() => typeResolver.ResolveType("unknown-type"));

            Assert.That(typeResolver.ResolveTypeHandler("unknown-type"), Is.Null);
            Assert.That(typeResolver.ResolveType("unknown-type"), Is.Null);
        }

        [Test]
        public void Test_SqlTypeResolver_ReturnsTypeReferenceForKnownType()
        {
            var sqlTypeRef = new SqlDataTypeReference() { Name = new SchemaObjectName() };
            sqlTypeRef.Name.Identifiers.Add(new Identifier { Value = "INT" });

            var typeRef = typeResolver.ResolveType(sqlTypeRef);

            Assert.That(typeRef, Is.Not.Null);
            Assert.That(typeRef.TypeName, Is.EqualTo("INT"));
        }

        [Test]
        public void Test_SqlTypeResolver_ReturnsTypeByName()
        {
            var typeRef = typeResolver.ResolveType("VARCHAR");

            Assert.That(typeRef, Is.Not.Null);
            Assert.That(typeRef.TypeName, Is.EqualTo("VARCHAR"));
        }

        [Test]
        public void Test_SqlTypeResolver_IsSupportedTypeDoesNoeFailOnNull()
        {
            Assert.DoesNotThrow(() => typeResolver.IsSupportedType(""));
            Assert.That(typeResolver.IsSupportedType(""), Is.False);

            Assert.DoesNotThrow(() => typeResolver.IsSupportedType(null));
            Assert.That(typeResolver.IsSupportedType(null), Is.False);
        }
    }
}
