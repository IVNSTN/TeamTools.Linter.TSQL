using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;
using TeamTools.TSQL.Linter.Routines.TableDefinitionExtractor;

namespace TeamTools.TSQL.Linter.Rules
{
    // TODO : ignore SPARSE columns?
    [RuleIdentity("DD0999", "BLOATED_CLUSTERED_IDX")]
    [IndexRule]
    internal sealed class BloatedClusteredIndexRule : AbstractRule
    {
        private static readonly int MaxColsPerIndex = 5;
        // I had to peek some number. Stands for SYSNAME
        private static readonly int MaxByteSizePerKey = 128;
        // If there are no non-clustered indices on a table then a little more feels to be not very bad
        private static readonly int MaxColsPerIndexForSingleIndex = 8;
        private static readonly int MaxByteSizePerKeyForSingleIndex = 512;
        private static readonly string TooManyColsTemplate = "has {0} cols on {1}";
        private static readonly string TooBigKeySizeTemplate = "estimated key size {0} is too big on {1} with {2} columns indexed";

        private static readonly IDictionary<string, int> TypeSize = new SortedDictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        private static readonly ICollection<string> DoubleByteTypes = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

        static BloatedClusteredIndexRule()
        {
            // TODO : implement more accurate estimation
            // with appropriate NULL, BIT and variable sized columns
            // TODO : consolidate all the metadata in resource file
            // TODO : treat numeric types more accurate
            TypeSize.Add("dbo.BIT", 1);
            TypeSize.Add("dbo.TINYINT", 1);
            TypeSize.Add("dbo.SMALLINT", 2);
            TypeSize.Add("dbo.INT", 4);
            TypeSize.Add("dbo.BIGINT", 8);
            TypeSize.Add("dbo.ROWVERSION", 8);
            TypeSize.Add("dbo.TIMESTAMP", 8);
            TypeSize.Add("dbo.UNIQUEIDENTIFIER", 16);
            TypeSize.Add("dbo.DATE", 3);
            TypeSize.Add("dbo.TIME", 5);
            TypeSize.Add("dbo.DATETIME", 8);
            TypeSize.Add("dbo.SMALLDATETIME", 4);
            TypeSize.Add("dbo.DATETIMEOFFSET", 8);
            TypeSize.Add("dbo.DATETIME2", 8); // top size
            TypeSize.Add("dbo.DECIMAL", 12); // medium size
            TypeSize.Add("dbo.NUMERIC", 12); // medium size
            TypeSize.Add("dbo.FLOAT", 8);
            TypeSize.Add("dbo.REAL", 8);
            TypeSize.Add("dbo.MONEY", 8);
            TypeSize.Add("dbo.SMALLMONEY", 4);

            DoubleByteTypes.Add("dbo.NCHAR");
            DoubleByteTypes.Add("dbo.NVARCHAR");
        }

        public BloatedClusteredIndexRule() : base()
        {
        }

        public override void Visit(TSqlScript node)
        {
            var elements = new TableDefinitionElementsEnumerator(node);

            TableKeyColumnTypeValidator.CheckOnAllTables(
                elements,
                tbl => elements.Indices(tbl)
                    .OfType<SqlIndexInfo>()
                    .Where(idx => idx.IsClustered && !idx.IsColumnStore)
                    // table variables dont have much options for organizing rows
                    // bloated PK may be used for reason
                    // no nonclustered (except some inline UNIQUEs) indexes of FK might be affected
                    .Where(idx => elements.Tables[tbl].TableType != SqlTableType.TableVariable),
                (tbl, el) => ValidateClusteredIndexCols(tbl, elements, el));
        }

        private static int EstimateKeySize(IDictionary<string, SqlColumnInfo> tableCols, ICollection<SqlColumnReferenceInfo> indexCols)
        {
            int estimatedSize = 0;

            var cols = indexCols.Join(
                tableCols,
                idxCol => idxCol.Name,
                tblCol => tblCol.Key,
                (idxCol, tblCol) => tblCol.Value);

            foreach (var col in cols)
            {
                if (TypeSize.ContainsKey(col.TypeName))
                {
                    estimatedSize += TypeSize[col.TypeName];
                }
                else if (col.TypeSize > 0)
                {
                    estimatedSize = col.TypeSize == int.MaxValue ? int.MaxValue : estimatedSize + col.TypeSize;

                    if (estimatedSize < int.MaxValue && DoubleByteTypes.Contains(col.TypeName))
                    {
                        // double for UNICODE strings
                        estimatedSize += col.TypeSize;
                    }
                }

                if (estimatedSize == int.MaxValue)
                {
                    break;
                }
            }

            return estimatedSize;
        }

        private static bool HasNonClusteredIndex(string tableName, TableDefinitionElementsEnumerator elements)
        {
            if (elements.Tables[tableName].TableType == SqlTableType.TypeTable)
            {
                // table types should be lightweight
                return true;
            }

            // TODO : refactor to avoid quadratic complexity
            // we are iterating through table Indices and then going again
            // to Indices of the same table
            return elements.Indices(tableName)
                .OfType<SqlIndexInfo>()
                .Any(idx => !idx.IsClustered);
        }

        // TODO : not sure about excluding partitioned cols
        private void ValidateClusteredIndexCols(string tableName, TableDefinitionElementsEnumerator elements, SqlTableElement clusteredIndex)
        {
            if (!(clusteredIndex is SqlIndexInfo idx))
            {
                return;
            }

            bool hasNonclusteredIdx = HasNonClusteredIndex(tableName, elements);

            if ((!hasNonclusteredIdx && idx.ColumnsExceptPartitioned.Count > MaxColsPerIndexForSingleIndex)
            || (hasNonclusteredIdx && idx.ColumnsExceptPartitioned.Count > MaxColsPerIndex))
            {
                HandleNodeError(
                    clusteredIndex.Definition,
                    string.Format(TooManyColsTemplate, idx.ColumnsExceptPartitioned.Count.ToString(), tableName));

                return;
            }

            int estimatedKeySize = EstimateKeySize(elements.Tables[tableName].Columns, idx.ColumnsExceptPartitioned);

            if ((!hasNonclusteredIdx && estimatedKeySize > MaxByteSizePerKeyForSingleIndex)
            || (hasNonclusteredIdx && estimatedKeySize > MaxByteSizePerKey))
            {
                HandleNodeError(
                    clusteredIndex.Definition,
                    string.Format(TooBigKeySizeTemplate, estimatedKeySize, tableName, idx.ColumnsExceptPartitioned.Count));
            }
        }
    }
}
