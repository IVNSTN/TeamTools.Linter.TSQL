using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;
using TeamTools.TSQL.Linter.Routines.TableDefinitionExtractor;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("PF0867", "INDEX_FK")]
    [IndexRule]
    internal sealed class ForeignKeyIndexRule : ScriptAnalysisServiceConsumingRule
    {
        public ForeignKeyIndexRule() : base()
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
                var indices = info.Indices(tbl.Key).OfType<SqlIndexInfo>().ToList();

                foreach (var fk in info.ForeignKeys(tbl.Key))
                {
                    if (!AreFkColumnsIndexed(fk.Columns, indices))
                    {
                        HandleNodeError(fk.Definition);
                    }
                }
            }
        }

        private static bool AreFkColumnsIndexed(IList<SqlColumnReferenceInfo> keyColumns, IList<SqlIndexInfo> indices)
        {
            for (int i = indices.Count - 1; i >= 0; i--)
            {
                if (DoColumnsMatch(keyColumns, indices[i].Columns))
                {
                    return true;
                }
            }

            // no match found
            return false;
        }

        private static bool DoColumnsMatch(IList<SqlColumnReferenceInfo> keyColumns, IList<SqlColumnReferenceInfo> idxColumns)
        {
            int keyColCount = keyColumns.Count;

            if (idxColumns.Count < keyColCount)
            {
                // Not enough columns indexed to cover FK
                return false;
            }

            int i = 0;
            while (i < keyColCount
            && string.Equals(keyColumns[i].Name, idxColumns[i].Name, StringComparison.OrdinalIgnoreCase))
            {
                ++i;
            }

            return i == keyColCount;
        }
    }
}
