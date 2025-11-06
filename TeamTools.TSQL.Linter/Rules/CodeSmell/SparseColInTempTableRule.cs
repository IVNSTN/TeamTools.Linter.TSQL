using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;
using TeamTools.TSQL.Linter.Routines.TableDefinitionExtractor;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0776", "SPARSE_COL_IN_TMP")]
    internal sealed class SparseColInTempTableRule : AbstractRule
    {
        public SparseColInTempTableRule() : base()
        {
        }

        public override void Visit(TSqlScript node)
        {
            var info = new TableDefinitionElementsEnumerator(node);
            var tempTablesWithSparseCols = info.Tables.Keys
                .Where(tbl => info.Tables[tbl].TableType.In(SqlTableType.TempTable, SqlTableType.TableVariable))
                .ToDictionary(tbl => tbl, tbl => info.Tables[tbl].Columns.Select(col => col.Value).FirstOrDefault(col => col.IsSparse));

            foreach (var badTbl in tempTablesWithSparseCols.Where(t => t.Value != null))
            {
                HandleNodeError(badTbl.Value.Node, badTbl.Value.Name);
            }
        }
    }
}
