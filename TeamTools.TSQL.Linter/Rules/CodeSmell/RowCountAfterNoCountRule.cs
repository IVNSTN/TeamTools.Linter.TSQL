using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    // Docs: https://learn.microsoft.com/en-us/sql/t-sql/functions/rowcount-transact-sql
    [RuleIdentity("CS0126", "BAD_ROWCOUNT_CHECK")]
    internal sealed class RowCountAfterNoCountRule : AbstractRule
    {
        public RowCountAfterNoCountRule() : base()
        {
        }

        public override void Visit(TSqlBatch node)
        {
            var noCounts = new List<KeyValuePair<int, TSqlStatement>>();
            var rowCounts = new List<KeyValuePair<int, TSqlStatement>>();

            foreach (var st in node.Statements)
            {
                // TODO : USE, SET<option>, DEALLOCATE CURSOR, CLOSE CURSOR, PRINT, RAISERROR, BEGIN TRANSACTION, or COMMIT TRANSACTION, TRY-CATCH, DECLARE reset the ROWCOUNT value to 0.
                // SET VAR, RETURN, READTEXT, DECLARE CURSOR, FETCH, select without from
                // reset ROWCOUNT value to 1.
                // TODO : natively compiled procs don't change ROWCOUNT after DML
                var setVisitor = new SetOptionsVisitor();
                st.Accept(setVisitor);
                if (setVisitor.DetectedOptions.Count > 0)
                {
                    AppendTo(noCounts, st.LastTokenIndex, st);
                    continue;
                }

                var rowCountVisitor = new RowCountVisitor();
                st.Accept(rowCountVisitor);
                if (rowCountVisitor.HasRowCount)
                {
                    AppendTo(rowCounts, st.LastTokenIndex, st);
                    continue;
                }

                var changeVisitor = new RowCountChangerVisitor();
                st.Accept(changeVisitor);
                if (changeVisitor.AffectsRowcount && (noCounts.Count > 0))
                {
                    // TODO : not sure if this approach is valid
                    noCounts.RemoveAt(noCounts.Count - 1);
                }
            }

            // FIXME: if we are in the beginning of a proc/batch then
            // @@ROWCOUNT = 0 - same as after SET NOCOUNT ON
            if (noCounts.Count == 0 || rowCounts.Count == 0)
            {
                return;
            }

            // FIXME : why first? where is token index intersection?
            // if token index does not matter then scalar values
            // instead of lists may be preferred in the code above
            HandleNodeError(noCounts.First().Value);
        }

        private static void AppendTo(IList<KeyValuePair<int, TSqlStatement>> dst, int tokenIndex, TSqlStatement node)
            => dst.Add(new KeyValuePair<int, TSqlStatement>(tokenIndex, node));

        private class RowCountVisitor : TSqlFragmentVisitor
        {
            private static readonly string RowCountVariable = "@@ROWCOUNT";

            public bool HasRowCount { get; private set; }

            public override void Visit(GlobalVariableExpression node)
            {
                if (node.Name.Equals(RowCountVariable, StringComparison.OrdinalIgnoreCase))
                {
                    HasRowCount = true;
                }
            }
        }

        private class RowCountChangerVisitor : TSqlFragmentVisitor
        {
            public bool AffectsRowcount { get; private set; }

            // This covers SELECTs and DML statements
            public override void Visit(StatementWithCtesAndXmlNamespaces node) => AffectsRowcount = true;

            public override void Visit(ExecuteStatement node) => AffectsRowcount = true;

            public override void Visit(ReceiveStatement node) => AffectsRowcount = true;
        }
    }
}
