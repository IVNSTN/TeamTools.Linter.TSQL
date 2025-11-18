using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;
using TeamTools.TSQL.Linter.Routines.TableDefinitionExtractor;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0184", "PK_COLUMN_NOT_NULL")]
    internal sealed class PrimaryKeyColNullableRule : ScriptAnalysisServiceConsumingRule
    {
        public PrimaryKeyColNullableRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            var info = GetService<TableDefinitionElementsEnumerator>(node);

            if (info.Tables.Count == 0)
            {
                return;
            }

            foreach (var tbl in info.Tables)
            {
                var pk = info.Keys(tbl.Key)
                    .OfType<SqlIndexInfo>()
                    .FirstOrDefault(idx => idx.ElementType == SqlTableElementType.PrimaryKey);

                if (pk != null)
                {
                    var nullableCol = FindNullableColInPrimaryKey(tbl.Value.Columns, pk.Columns);
                    if (nullableCol != null)
                    {
                        HandleNodeError(nullableCol.Node.ColumnIdentifier, nullableCol.Name);
                    }
                }
            }
        }

        private static SqlColumnInfo FindNullableColInPrimaryKey(IDictionary<string, SqlColumnInfo> tblCols, List<SqlColumnReferenceInfo> idxCols)
        {
            int n = idxCols.Count;
            for (int i = 0; i < n; i++)
            {
                if (tblCols.TryGetValue(idxCols[i].Name, out var colInfo)
                && colInfo.IsNullable)
                {
                    return colInfo;
                }
            }

            return default;
        }
    }
}
