using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Index name builder.
    /// </summary>
    internal partial class IndexNamePatternRule
    {
        private class IndexNameBuilder
        {
            private static readonly string NamePartDelimiter = "_";
            private static readonly string ColumnStorePrefix = "CL";
            private static readonly string UniqueIndexPrefix = "IU";
            private static readonly string RegularIndexPrefix = "IX";
            private static readonly string FilteredSuffix = "F";
            private static readonly string IncludeSuffix = "I";
            private static readonly int MinColAsIs = 1;
            private static readonly int MaxColAsIs = 2;
            private static readonly NumeronimBuilder Numeronim = new NumeronimBuilder(NamePartDelimiter, MaxColAsIs, MinColAsIs);

            public string Build(
                string tableSchema,
                string tableName,
                bool isColumnStore,
                bool isClustered,
                bool isUnique,
                bool isFiltered,
                bool hasInclude,
                IEnumerable<ColumnReferenceExpression> cols)
            {
                var idxName = new StringBuilder();

                if (isColumnStore)
                {
                    idxName.Append(ColumnStorePrefix);
                }
                else if (isUnique)
                {
                    idxName.Append(UniqueIndexPrefix);
                }
                else
                {
                    idxName.Append(RegularIndexPrefix);
                }

                if (hasInclude)
                {
                    idxName.Append(IncludeSuffix);
                }

                if (isFiltered)
                {
                    idxName.Append(FilteredSuffix);
                }

                // TODO : support dbo schema omitted
                if (!string.IsNullOrEmpty(tableSchema))
                {
                    idxName.Append(NamePartDelimiter).Append(tableSchema);
                }

                idxName.Append(NamePartDelimiter).Append(tableName);

                if (cols != null)
                {
                    idxName.Append(GetColumnListOrNumeronim(cols));
                }

                return idxName.ToString();
            }

            private static string GetColumnListOrNumeronim(IEnumerable<ColumnReferenceExpression> cols)
            {
                var colNames = cols.Select(col => col.MultiPartIdentifier.Identifiers.Last().Value);
                return Numeronim.Build(colNames);
            }
        }
    }
}
