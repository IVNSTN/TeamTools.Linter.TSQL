using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
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

        protected override void ValidateBatch(TSqlBatch node)
        {
            List<Tuple<int, TSqlStatement>> noCounts = null;
            List<Tuple<int, TSqlStatement>> rowCounts = null;

            var setVisitor = new SetOptionsVisitor();
            var rowCountVisitor = new RowCountVisitor();
            var changeVisitor = new RowCountChangerVisitor();

            int n = node.Statements.Count;
            for (int i = 0; i < n; i++)
            {
                var st = node.Statements[i];

                setVisitor.Reset();
                rowCountVisitor.Reset();
                changeVisitor.Reset();

                // TODO : USE, SET<option>, DEALLOCATE CURSOR, CLOSE CURSOR, PRINT, RAISERROR, BEGIN TRANSACTION, or COMMIT TRANSACTION, TRY-CATCH, DECLARE reset the ROWCOUNT value to 0.
                // SET VAR, RETURN, READTEXT, DECLARE CURSOR, FETCH, select without from
                // reset ROWCOUNT value to 1.
                // TODO : natively compiled procs don't change ROWCOUNT after DML
                st.Accept(setVisitor);
                if (setVisitor.DetectedOptions.Count > 0)
                {
                    if (noCounts is null)
                    {
                        noCounts = new List<Tuple<int, TSqlStatement>>();
                    }

                    AppendTo(noCounts, st.LastTokenIndex, st);
                    // TODO : i dont understand this iteration break
                    continue;
                }

                st.Accept(rowCountVisitor);
                if (rowCountVisitor.HasRowCount)
                {
                    if (rowCounts is null)
                    {
                        rowCounts = new List<Tuple<int, TSqlStatement>>();
                    }

                    AppendTo(rowCounts, st.LastTokenIndex, st);
                    // TODO : i dont understand this iteration break
                    continue;
                }

                st.Accept(changeVisitor);
                if (changeVisitor.AffectsRowcount && noCounts != null && noCounts.Count > 0)
                {
                    // TODO : not sure if this approach is valid
                    noCounts.RemoveAt(noCounts.Count - 1);
                }
            }

            // FIXME: if we are in the beginning of a proc/batch then
            // @@ROWCOUNT = 0 - same as after SET NOCOUNT ON
            if (noCounts is null || noCounts.Count == 0
            || rowCounts is null || rowCounts.Count == 0)
            {
                return;
            }

            // FIXME : why first? where is token index intersection?
            // if token index does not matter then scalar values
            // instead of lists may be preferred in the code above
            HandleNodeError(noCounts[0].Item2);
        }

        private static void AppendTo(List<Tuple<int, TSqlStatement>> dst, int tokenIndex, TSqlStatement node)
            => dst.Add(new Tuple<int, TSqlStatement>(tokenIndex, node));

        private sealed class RowCountVisitor : TSqlFragmentVisitor
        {
            private static readonly string RowCountVariable = "@@ROWCOUNT";

            public bool HasRowCount { get; private set; }

            public void Reset() => HasRowCount = false;

            public override void Visit(GlobalVariableExpression node)
            {
                if (node.Name.Equals(RowCountVariable, StringComparison.OrdinalIgnoreCase))
                {
                    HasRowCount = true;
                }
            }
        }

        private sealed class RowCountChangerVisitor : TSqlFragmentVisitor
        {
            public bool AffectsRowcount { get; private set; }

            public void Reset() => AffectsRowcount = false;

            // This covers SELECTs and DML statements
            public override void Visit(StatementWithCtesAndXmlNamespaces node) => AffectsRowcount = true;

            public override void Visit(ExecuteStatement node) => AffectsRowcount = true;

            public override void Visit(ReceiveStatement node) => AffectsRowcount = true;
        }
    }
}
