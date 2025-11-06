using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
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
            private static readonly IDictionary<ConstraintType, string> Prefixes = new Dictionary<ConstraintType, string>();
            private static readonly string NamePartDelimiter = "_";
            private static readonly int MinColAsIs = 1;
            private static readonly int MaxColAsIs = 2;
            private static readonly NumeronimBuilder Numeronim = new NumeronimBuilder(NamePartDelimiter, MaxColAsIs, MinColAsIs);

            static ConstraintNameBuilder()
            {
                Prefixes.Add(ConstraintType.PrimaryKey, "PK");
                Prefixes.Add(ConstraintType.ForeignKey, "FK");
                Prefixes.Add(ConstraintType.Default, "DF");
                Prefixes.Add(ConstraintType.Check, "CK");
                Prefixes.Add(ConstraintType.Unique, "UQ");
            }

            public string Build(ConstraintType constraintType, string tableName, IEnumerable<string> columns)
            {
                Debug.Assert(Prefixes.ContainsKey(constraintType), "missing prefix definition");

                var constraintName = new StringBuilder();
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

                return constraintName.ToString();
            }
        }
    }
}
