using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;
using TeamTools.TSQL.Linter.Routines.TableDefinitionExtractor;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0776", "SPARSE_COL_IN_TMP")]
    internal sealed class SparseColInTempTableRule : ScriptAnalysisServiceConsumingRule
    {
        public SparseColInTempTableRule() : base()
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
                if (tbl.Value.TableType != SqlTableType.TempTable
                && tbl.Value.TableType != SqlTableType.TableVariable)
                {
                    continue;
                }

                var sparseCol = tbl.Value.Columns.FirstOrDefault(col => col.Value.IsSparse);

                HandleNodeErrorIfAny(sparseCol.Value?.Node, tbl.Key);
            }
        }
    }
}
