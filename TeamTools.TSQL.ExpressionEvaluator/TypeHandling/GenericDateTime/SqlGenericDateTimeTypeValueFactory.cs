using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.TSQL.ExpressionEvaluator.Interfaces;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    public abstract class SqlGenericDateTimeTypeValueFactory<TValue, TSqlValue> : SqlGenericValueFactory<TSqlValue, SqlDateTimeValueRange, TValue>, ISqlValueFactory
    where TValue : IComparable<TValue>
    where TSqlValue : SqlValue
    {
        private static readonly string DateTimeFallbackTypeName = TSqlDomainAttributes.Types.DateTime;

        private static readonly Dictionary<string, SqlDateTimeValueRange> DefaultRanges = new Dictionary<string, SqlDateTimeValueRange>(StringComparer.OrdinalIgnoreCase)
        {
            { TSqlDomainAttributes.Types.SmallDateTime, new SqlDateTimeValueRange(new SqlDateTimeRelativeValue(TimeDetails.DayTime, DateDetails.Small)) },
            { TSqlDomainAttributes.Types.DateTime, new SqlDateTimeValueRange(new SqlDateTimeRelativeValue(TimeDetails.RegularDateTime, DateDetails.Full)) },
            { TSqlDomainAttributes.Types.DateTime2, new SqlDateTimeValueRange(new SqlDateTimeRelativeValue(TimeDetails.Detailed, DateDetails.Full)) },
            { "DATE", new SqlDateTimeValueRange(new SqlDateTimeRelativeValue(TimeDetails.None, DateDetails.Full)) },
            { "TIME", new SqlDateTimeValueRange(new SqlDateTimeRelativeValue(TimeDetails.Detailed, DateDetails.None)) },
        };

        protected SqlGenericDateTimeTypeValueFactory() : base(DateTimeFallbackTypeName)
        {
        }

        public override SqlDateTimeValueRange GetDefaultTypeSize(string typeName)
        {
            if (!string.IsNullOrEmpty(typeName) && DefaultRanges.TryGetValue(typeName, out var range))
            {
                return range;
            }

            return default;
        }

        public abstract SqlValue NewLiteral(string typeName, string value, TSqlFragment source);

        public abstract SqlValue NewNull(TSqlFragment source);

        public abstract SqlValue NewValue(SqlTypeReference typeRef, SqlValueKind valueKind);
    }
}
