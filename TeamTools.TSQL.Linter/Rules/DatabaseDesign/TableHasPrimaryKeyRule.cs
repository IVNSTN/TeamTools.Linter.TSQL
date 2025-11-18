using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;
using TeamTools.TSQL.Linter.Routines.TableDefinitionExtractor;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0941", "TABLE_HAS_PK")]
    internal sealed class TableHasPrimaryKeyRule : ScriptAnalysisServiceConsumingRule
    {
        public TableHasPrimaryKeyRule() : base()
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
                if (!info.Indices(tbl.Key)
                    .OfType<SqlIndexInfo>()
                    .Any(idx => idx.ElementType == SqlTableElementType.PrimaryKey || idx.IsColumnStore))
                {
                    HandleNodeError(tbl.Value.Node, tbl.Key);
                }
            }
        }
    }
}
