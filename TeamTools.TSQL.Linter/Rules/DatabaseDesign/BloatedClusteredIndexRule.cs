using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Properties;
using TeamTools.TSQL.Linter.Routines;
using TeamTools.TSQL.Linter.Routines.TableDefinitionExtractor;

namespace TeamTools.TSQL.Linter.Rules
{
    // TODO : ignore SPARSE columns?
    [RuleIdentity("DD0999", "BLOATED_CLUSTERED_IDX")]
    [IndexRule]
    internal sealed class BloatedClusteredIndexRule : ScriptAnalysisServiceConsumingRule
    {
        private static readonly int MaxColsPerIndex = 5;
        // I had to peek some number. Stands for SYSNAME
        private static readonly int MaxByteSizePerKey = 128;
        // If there are no non-clustered indices on a table then a little more feels to be not very bad
        private static readonly int MaxColsPerIndexForSingleIndex = 8;
        private static readonly int MaxByteSizePerKeyForSingleIndex = 512;
        private static readonly string TooManyColsTemplate = Strings.ViolationDetails_BloatedClusteredIndexRule_TooManyCols;
        private static readonly string TooBigKeySizeTemplate = Strings.ViolationDetails_BloatedClusteredIndexRule_TooBig;

        private static readonly Dictionary<string, int> TypeSize;

        private static readonly HashSet<string> DoubleByteTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
           TSqlDomainAttributes.Types.NChar,
           TSqlDomainAttributes.Types.NVarchar,
        };

        static BloatedClusteredIndexRule()
        {
            // TODO : implement more accurate estimation
            // with appropriate NULL, BIT and variable sized columns
            // TODO : consolidate all the metadata in resource file
            // TODO : treat numeric types more accurate
            TypeSize = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { TSqlDomainAttributes.Types.Bit, 1 },
                { TSqlDomainAttributes.Types.TinyInt, 1 },
                { TSqlDomainAttributes.Types.SmallInt, 2 },
                { TSqlDomainAttributes.Types.Int, 4 },
                { TSqlDomainAttributes.Types.BigInt, 8 },
                { "ROWVERSION", 8 },
                { "TIMESTAMP", 8 },
                { "UNIQUEIDENTIFIER", 16 },
                { "DATE", 3 },
                { "TIME", 5 },
                { "DATETIME", 8 },
                { "SMALLDATETIME", 4 },
                { "DATETIMEOFFSET", 8 },
                { "DATETIME2", 8 }, // top size
                { "DECIMAL", 12 }, // medium size
                { "NUMERIC", 12 }, // medium size
                { "FLOAT", 8 },
                { "REAL", 8 },
                { "MONEY", 8 },
                { "SMALLMONEY", 4 },
            };
        }

        public BloatedClusteredIndexRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            var elements = GetService<TableDefinitionElementsEnumerator>(node);

            if (elements.Tables.Count == 0)
            {
                return;
            }

            var validate = new Action<string, SqlTableElement>((tableName, el) => ValidateClusteredIndexCols(tableName, el, elements));

            TableKeyColumnTypeValidator.CheckOnAllTables(
                elements,
                tbl => elements.Indices(tbl)
                    .OfType<SqlIndexInfo>()
                    .Where(idx => idx.IsClustered && !idx.IsColumnStore
                        // table variables dont have much options for organizing rows
                        // bloated PK may be used for reason
                        // no nonclustered (except some inline UNIQUEs) indexes of FK might be affected
                        && elements.Tables[tbl].TableType != SqlTableType.TableVariable),
                validate);
        }

        private static int EstimateKeySize(IDictionary<string, SqlColumnInfo> tableCols, IList<SqlColumnReferenceInfo> indexCols)
        {
            int estimatedSize = 0;

            var cols = indexCols.Join(
                tableCols,
                idxCol => idxCol.Name,
                tblCol => tblCol.Key,
                (idxCol, tblCol) => tblCol.Value);

            foreach (var col in cols)
            {
                if (TypeSize.TryGetValue(col.TypeName, out var colTypeSize))
                {
                    estimatedSize += colTypeSize;
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
        private void ValidateClusteredIndexCols(string tableName, SqlTableElement clusteredIndex, TableDefinitionElementsEnumerator elements)
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
