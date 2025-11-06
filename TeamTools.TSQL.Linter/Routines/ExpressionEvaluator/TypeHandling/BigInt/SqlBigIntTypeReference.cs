using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling
{
    public class SqlBigIntTypeReference : SqlGenericTypeReference<SqlBigIntValueRange>
    {
        private static readonly IDictionary<string, int> TypeWeight = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        static SqlBigIntTypeReference()
        {
            TypeWeight.Add("dbo.BIGINT", 8);
        }

        public SqlBigIntTypeReference(string typeName, SqlBigIntValueRange size, ISqlValueFactory valueFactory)
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
