using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling
{
    // TODO : split to signed and unsigned type refs
    public class SqlIntTypeReference : SqlGenericTypeReference<SqlIntValueRange>
    {
        private static readonly IDictionary<string, int> TypeWeight = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        static SqlIntTypeReference()
        {
            TypeWeight.Add("dbo.BIT", 1);
            TypeWeight.Add("dbo.TINYINT", 1);
            TypeWeight.Add("dbo.SMALLINT", 2);
            TypeWeight.Add("dbo.INT", 4);
        }

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
