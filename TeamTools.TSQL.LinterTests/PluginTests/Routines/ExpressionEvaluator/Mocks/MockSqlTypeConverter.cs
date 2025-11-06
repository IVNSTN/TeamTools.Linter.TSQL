using System;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    public sealed class MockSqlTypeConverter : SqlTypeConverter
    {
        private readonly ISqlTypeHandler mockTypeHandler;

        public MockSqlTypeConverter(ISqlTypeHandler mockTypeHandler, ISqlTypeResolver typeResolver)
        : base(typeResolver)
        {
            this.mockTypeHandler = mockTypeHandler;
        }

        public override T ImplicitlyConvert<T>(SqlValue from)
        {
            if (from is MockSqlValue mv)
            {
                if (typeof(T) == typeof(SqlStrTypeValue))
                {
                    if (from.TypeName.EndsWith("CHAR"))
                    {
                        return (T)GenerateFromMock(from.TypeReference.TypeName, mv);
                    }

                    return (T)GenerateFromMock("dbo.NVARCHAR", mv);
                }

                if (typeof(T) == typeof(SqlIntTypeValue))
                {
                    if (from.TypeName.EndsWith("INT"))
                    {
                        return (T)GenerateFromMock(from.TypeReference.TypeName, mv);
                    }

                    return (T)GenerateFromMock("dbo.INT", mv);
                }
            }

            return base.ImplicitlyConvert<T>(from);
        }

        public override SqlValue ImplicitlyConvertTo(string typeName, SqlValue from)
        {
            if (from is MockSqlValue mockVal)
            {
                return GenerateFromMock(typeName, mockVal);
            }
            else
            {
                return base.ImplicitlyConvertTo(typeName, from);
            }
        }

        private SqlValue GenerateFromMock(string typeName, MockSqlValue from)
        {
            if (typeName.EndsWith("INT"))
            {
                if (from.IsNull)
                {
                    return new SqlIntTypeValue(
                       new SqlIntTypeHandler(this, new ViolationReporter()),
                       new SqlIntTypeReference(typeName, new SqlIntValueRange(0, 1000), mockTypeHandler.ValueFactory),
                       SqlValueKind.Null,
                       from.Source);
                }

                if (from.IsPreciseValue)
                {
                    return new SqlIntTypeValue(
                        new SqlIntTypeHandler(this, new ViolationReporter()),
                        new SqlIntTypeReference(typeName, new SqlIntValueRange(from.IntValue, from.IntValue), mockTypeHandler.ValueFactory),
                        from.IntValue,
                        from.Source);
                }

                var range = new SqlIntValueRange(1, 2);
                if (from.TypeReference is SqlIntTypeReference intRef)
                {
                    range = intRef.Size;
                }

                return new SqlIntTypeValue(
                    new SqlIntTypeHandler(this, new ViolationReporter()),
                    new SqlIntTypeReference(typeName, range, mockTypeHandler.ValueFactory),
                    SqlValueKind.Unknown,
                    from.Source);
            }
            else if (typeName.EndsWith("CHAR"))
            {
                Func<string, int, SqlStrTypeReference> makeTypeRef = (typeName, size) => new SqlStrTypeReference(typeName, size, mockTypeHandler.ValueFactory);

                if (typeName.StartsWith("dbo.N"))
                {
                    makeTypeRef = (typeName, size) => new SqlUnicodeStrTypeReference(typeName, size, mockTypeHandler.ValueFactory);
                }

                if (from.IsNull)
                {
                    return new SqlStrTypeValue(
                       new SqlStrTypeHandler(this, new ViolationReporter()),
                       makeTypeRef(typeName, 8000),
                       SqlValueKind.Null,
                       from.Source);
                }

                if (from.IsPreciseValue)
                {
                    return new SqlStrTypeValue(
                        new SqlStrTypeHandler(this, new ViolationReporter()),
                        makeTypeRef(typeName, from.StrValue.Length),
                        from.StrValue,
                        from.Source);
                }

                int approximateLength = 7; // dummy
                if (from.TypeReference is SqlStrTypeReference strRef)
                {
                    approximateLength = strRef.Size;
                }

                return new SqlStrTypeValue(
                    new SqlStrTypeHandler(this, new ViolationReporter()),
                    makeTypeRef(typeName, approximateLength),
                    SqlValueKind.Unknown,
                    from.Source);
            }

            return new SqlStrTypeValue(
                new SqlStrTypeHandler(this, new ViolationReporter()),
                new SqlStrTypeReference("dbo.CHAR", 10, mockTypeHandler.ValueFactory),
                SqlValueKind.Unknown,
                from.Source);
        }
    }
}
