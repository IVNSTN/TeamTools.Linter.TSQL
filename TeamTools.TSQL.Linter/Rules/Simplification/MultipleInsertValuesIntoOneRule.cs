using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("SI0859", "MULTIPLE_INSERT_VALUES_COLLAPSE")]
    internal sealed class MultipleInsertValuesIntoOneRule : AbstractRule
    {
        // Too long statements dont look fine
        private static readonly int MaxRowsTogether = 40;

        // Sometimes inserts are divided into separate statements:
        // with short list of target cols and values and with the long one.
        private static readonly int MaxColumnNumberDifference = 3;

        public MultipleInsertValuesIntoOneRule() : base()
        {
        }

        // ScriptDom does not fire Visit for StatementList itself nevertheless it is a descendant of TSqlFragment
        public override void Visit(BeginEndBlockStatement node) => Validate(node.StatementList);

        public override void Visit(TryCatchStatement node) => Validate(node.TryStatements);

        protected override void ValidateBatch(TSqlBatch node)
        {
            Validate(node.Statements);

            // Visit nested blocks
            node.Accept(this);
        }

        private static string GetTableName(TableReference tbl)
        {
            if (tbl is VariableTableReference var)
            {
                return var.Variable.Name;
            }

            if (tbl is NamedTableReference name)
            {
                return name.SchemaObject.GetFullName();
            }

            return default;
        }

        private void Validate(StatementList node)
        {
            if ((node.Statements?.Count ?? 0) == 0)
            {
                return;
            }

            Validate(node.Statements);
        }

        private void Validate(IList<TSqlStatement> statements)
        {
            string lastInsertTarget = null;
            int lastInsertValuesCount = 0;

            for (int i = 0, n = statements.Count; i < n; i++)
            {
                if (statements[i] is InsertStatement ins
                && ins.InsertSpecification.InsertSource is ValuesInsertSource val
                && val.RowValues.Count > 0
                && val.RowValues.Count < MaxRowsTogether
                && Math.Abs(lastInsertValuesCount - val.RowValues[0].ColumnValues.Count) <= MaxColumnNumberDifference)
                {
                    string targetName = GetTableName(ins.InsertSpecification.Target);

                    if (!string.IsNullOrEmpty(lastInsertTarget)
                    && !string.IsNullOrEmpty(targetName)
                    && string.Equals(lastInsertTarget, targetName, StringComparison.OrdinalIgnoreCase))
                    {
                        HandleNodeError(ins.InsertSpecification.Target, targetName);
                    }
                    else
                    {
                        lastInsertTarget = targetName;
                    }

                    lastInsertValuesCount = val.RowValues[0].ColumnValues.Count;
                }
                else
                {
                    // reset until next sequence of inserts found
                    lastInsertTarget = null;
                }
            }
        }
    }
}
