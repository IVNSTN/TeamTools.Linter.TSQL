using System;
using System.Collections.Generic;
using TeamTools.TSQL.ExpressionEvaluator.Interfaces;
using TeamTools.TSQL.ExpressionEvaluator.Routines;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    // TODO : split to signed and unsigned type refs
    public class SqlIntTypeReference : SqlGenericTypeReference<SqlIntValueRange>
    {
        private static readonly IDictionary<string, int> TypeWeight = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { TSqlDomainAttributes.Types.Bit, 1 },
            { TSqlDomainAttributes.Types.TinyInt, 1 },
            { TSqlDomainAttributes.Types.SmallInt, 2 },
            { TSqlDomainAttributes.Types.Int, 4 },
        };

        public SqlIntTypeReference(string typeName, SqlIntValueRange size, ISqlValueFactory valueFactory)
        : base(typeName, size, valueFactory)
        {
            if (!TypeWeight.ContainsKey(typeName))
            {
                throw new ArgumentOutOfRangeException(nameof(typeName), typeName, "Incompatible type");
            }
        }

        protected override int GetBytes() => TypeWeight[TypeName];
    }
}
