using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;
using TeamTools.TSQL.Linter.Routines.TableDefinitionExtractor;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0904", "INDEX_REFERS_UNKNOWN_COL")]
    [IndexRule]
    internal sealed class IndexReferencingUnknownColumnRule : ScriptAnalysisServiceConsumingRule
    {
        public IndexReferencingUnknownColumnRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            var idxVisitor = GetService<TableDefinitionElementsEnumerator>(node);

            if (idxVisitor.Tables.Count == 0)
            {
                return;
            }

            ValidateIndexes(idxVisitor);
        }

        private static string AggregateColNames(IList<ColumnReferenceExpression> cols)
        {
            var colNames = cols
                .ExtractNames()
                .Distinct(StringComparer.OrdinalIgnoreCase);

            return string.Join(",", colNames);
        }

        private void ValidateIndexes(TableDefinitionElementsEnumerator idxVisitor)
        {
            foreach (var tbl in idxVisitor.Tables.Keys)
            {
                var tblCols = idxVisitor.Tables[tbl].Columns;

                foreach (var idx in idxVisitor.Indices(tbl))
                {
                    if (idx.Definition is CreateIndexStatement idxCreate)
                    {
                        DetectInvalidReference(tblCols, idxCreate.Columns);
                        DetectInvalidReference(tblCols, idxCreate.IncludeColumns);
                    }
                    else if (idx.Definition is IndexDefinition idxDef)
                    {
                        DetectInvalidReference(tblCols, idxDef.Columns);
                        DetectInvalidReference(tblCols, idxDef.IncludeColumns);
                    }
                    else if (idx.Definition is UniqueConstraintDefinition cstr)
                    {
                        DetectInvalidReference(tblCols, cstr.Columns);
                    }
                }
            }
        }

        private void DetectInvalidReference(IDictionary<string, SqlColumnInfo> cols, IList<ColumnReferenceExpression> refs)
        {
            if (cols?.Count == 0 || refs?.Count == 0)
            {
                return;
            }

            for (int i = 0, n = refs.Count; i < n; i++)
            {
                var col = refs[i];
                var refColName = col.MultiPartIdentifier.GetLastIdentifier()?.Value;
                if (!string.IsNullOrEmpty(refColName) && !cols.ContainsKey(refColName))
                {
                    HandleNodeError(col, refColName);
                }
            }
        }

        private void DetectInvalidReference(IDictionary<string, SqlColumnInfo> cols, IList<ColumnWithSortOrder> refs)
        {
            if (cols?.Count == 0 || refs?.Count == 0)
            {
                return;
            }

            for (int i = 0, n = refs.Count; i < n; i++)
            {
                var col = refs[i];
                var refColName = col.Column.MultiPartIdentifier.GetLastIdentifier()?.Value;
                if (!string.IsNullOrEmpty(refColName) && !cols.ContainsKey(refColName))
                {
                    HandleNodeError(col, refColName);
                }
            }
        }
    }
}
