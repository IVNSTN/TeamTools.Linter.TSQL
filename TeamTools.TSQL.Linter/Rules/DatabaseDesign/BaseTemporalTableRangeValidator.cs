using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    internal abstract partial class BaseTemporalTableRangeValidator : AbstractRule
    {
        public override void Visit(CreateTableStatement node)
        {
            if (node.Definition is null)
            {
                // e.g. filetable
                return;
            }

            if (node.Definition.SystemTimePeriod is null)
            {
                // PERIOD not defined
                return;
            }

            var startCol = node.Definition.SystemTimePeriod.StartTimeColumn.Value;
            var endCol = node.Definition.SystemTimePeriod.EndTimeColumn.Value;

            ValidateHistoryColumns(node.Definition.ColumnDefinitions, startCol, endCol);
        }

        protected virtual void ValidateHistoryColumns(IList<ColumnDefinition> cols, string startCol, string endCol)
        {
            for (int i = cols.Count - 1; i >= 0; i--)
            {
                var col = cols[i];
                if (DoesColumnDefinePeriodRangeIncorrectly(col, startCol, endCol))
                {
                    HandleNodeError(col.DataType, col.ColumnIdentifier.Value);
                }
            }
        }

        protected virtual bool DoesColumnDefinePeriodRangeIncorrectly(ColumnDefinition col, string startCol, string endCol)
        {
            return DoesColumnDefineTemporalRange(col, startCol, endCol)
                && !DoesColumnHaveCorrectDefinition(col);
        }

        protected virtual bool DoesColumnHaveCorrectDefinition(ColumnDefinition col)
        {
            return IsDataTypeAlright(col.DataType) || IsDefaultAlright(col);
        }

        protected virtual bool DoesColumnDefineTemporalRange(ColumnDefinition col, string startCol, string endCol)
        {
            return col.GeneratedAlways != null
                || col.ColumnIdentifier.Value.Equals(startCol, StringComparison.OrdinalIgnoreCase)
                || col.ColumnIdentifier.Value.Equals(endCol, StringComparison.OrdinalIgnoreCase);
        }
    }
}
