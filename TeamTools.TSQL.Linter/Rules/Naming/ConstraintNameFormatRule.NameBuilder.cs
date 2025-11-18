using System.Collections.Generic;
using System.Diagnostics;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Constraint name builder.
    /// </summary>
    internal sealed partial class ConstraintNameFormatRule
    {
        private class ConstraintNameBuilder
        {
            private static readonly Dictionary<ConstraintType, string> Prefixes;
            private static readonly string NamePartDelimiter = "_";
            private static readonly int MinColAsIs = 1;
            private static readonly int MaxColAsIs = 2;
            private static readonly NumeronimBuilder Numeronim = new NumeronimBuilder(NamePartDelimiter, MaxColAsIs, MinColAsIs);

            static ConstraintNameBuilder()
            {
                Prefixes = new Dictionary<ConstraintType, string>
                {
                    { ConstraintType.PrimaryKey, "PK" },
                    { ConstraintType.ForeignKey, "FK" },
                    { ConstraintType.Default, "DF" },
                    { ConstraintType.Check, "CK" },
                    { ConstraintType.Unique, "UQ" },
                };
            }

            public string Build(ConstraintType constraintType, string tableName, string[] columns)
            {
                Debug.Assert(Prefixes.ContainsKey(constraintType), "missing prefix definition");

                var constraintName = ObjectPools.StringBuilderPool.Get();
                constraintName.Append(Prefixes[constraintType]);
                constraintName.Append(NamePartDelimiter).Append(tableName);

                if (constraintType == ConstraintType.Check)
                {
                    // FIXME: magic regex
                    constraintName.Append(NamePartDelimiter).Append(@"([\w]{2,24})");
                }
                else if (constraintType != ConstraintType.PrimaryKey && columns != null)
                {
                    constraintName.Append(Numeronim.Build(columns));
                }

                var result = constraintName.ToString();
                ObjectPools.StringBuilderPool.Return(constraintName);
                return result;
            }
        }
    }
}
