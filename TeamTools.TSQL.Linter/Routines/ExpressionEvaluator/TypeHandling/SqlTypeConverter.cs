using System;
using System.Collections.Generic;
using System.Linq;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling
{
    // TODO : define type compatibility table
    // TODO : define type precedence
    public class SqlTypeConverter : ISqlTypeConverter
    {
        private readonly ISqlTypeResolver typeResolver;

        public SqlTypeConverter(ISqlTypeResolver typeResolver)
        {
            this.typeResolver = typeResolver ?? throw new ArgumentNullException(nameof(typeResolver));
        }

        protected ISqlTypeResolver TypeResolver => typeResolver;

        public SqlValue ExplicitlyConvertTo(SqlTypeReference typeReference, SqlValue from, Action<string> redundantConversionCallback = null)
        {
            if (from is null || typeReference is null)
            {
                return default;
            }

            if (IsAlreadyOfThisType(typeReference, from))
            {
                redundantConversionCallback?.Invoke(typeReference.TypeName);
                return from;
            }

            return DoConvertTo(typeReference, from, true);
        }

        // TODO : refactor, go to abstractions, get rid of specific type hardcoding
        public virtual T ImplicitlyConvert<T>(SqlValue from)
        where T : SqlValue
        {
            if (from is null)
            {
                return default;
            }

            if (typeof(T) == typeof(SqlStrTypeValue))
            {
                return (T)ImplicitlyConvertTo("dbo.VARCHAR", from);
            }

            if (typeof(T) == typeof(SqlIntTypeValue))
            {
                return (T)ImplicitlyConvertTo("dbo.INT", from);
            }

            if (typeof(T) == typeof(SqlBigIntTypeValue))
            {
                return (T)ImplicitlyConvertTo("dbo.BIGINT", from);
            }

            return default;
        }

        public virtual SqlValue ImplicitlyConvertTo(string typeName, SqlValue from)
        {
            if (from is null || string.IsNullOrEmpty(typeName))
            {
                return default;
            }

            if (IsAlreadyOfThisType(typeName, from))
            {
                return from;
            }

            return DoConvertTo(typeName, from);
        }

        public SqlValue ImplicitlyConvertTo(SqlTypeReference typeReference, SqlValue from)
        {
            if (from is null || typeReference is null)
            {
                return default;
            }

            if (IsAlreadyOfThisType(typeReference, from))
            {
                return from;
            }

            return DoConvertTo(typeReference, from);
        }

        public string EvaluateOutputType(params SqlValue[] values) => EvaluateOutputType(values.ToList());

        // TODO : get rid of "dbo." crutches
        // TODO : move to SqlTypeResolver?
        public string EvaluateOutputType(IList<SqlValue> values)
        {
            string outType = ExpressionResultTypeEvaluator
                .GetResultingType(values.Select(v => v.TypeName.Replace("dbo.", "")));
            return string.IsNullOrEmpty(outType) ? outType : "dbo." + outType;
        }

        private static bool IsAlreadyOfThisType(string typeName, SqlValue value)
             => string.Equals(typeName, value.TypeName, StringComparison.OrdinalIgnoreCase);

        private static bool IsAlreadyOfThisType(SqlTypeReference typeRef, SqlValue value)
            => typeRef.Equals(value.TypeReference);

        private static SqlValue PostProcess(SqlValue value)
        {
            // TODO : not sure if this is converter's responsibility
            // FIXME : too much magic
            if (value != null && value.SourceKind != SqlValueSourceKind.Expression
            && (!value.IsPreciseValue || value.SourceKind == SqlValueSourceKind.Variable))
            {
                value.Source = new SqlValueSource(SqlValueSourceKind.Expression, value.Source?.Node);
            }

            return value;
        }

        private SqlValue DoConvertTo(SqlTypeReference typeReference, SqlValue from, bool forceTargetType = false)
        {
            var ev = typeResolver
                .ResolveTypeHandler(typeReference.TypeName)?
                .ConvertFrom(from, typeReference, forceTargetType);

            return PostProcess(ev);
        }

        private SqlValue DoConvertTo(string typeName, SqlValue from)
        {
            var ev = typeResolver
                .ResolveTypeHandler(typeName)?
                .ConvertFrom(from, typeName);

            return PostProcess(ev);
        }
    }
}
