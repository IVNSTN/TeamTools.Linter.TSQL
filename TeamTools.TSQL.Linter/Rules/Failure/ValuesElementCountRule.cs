using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0199", "VALUES_ELEMENT_NUMBER")]
    internal sealed class ValuesElementCountRule : AbstractRule
    {
        public ValuesElementCountRule() : base()
        {
        }

        public override void Visit(ValuesInsertSource node) => ValidateRowValues(node.RowValues);

        public override void Visit(InlineDerivedTable node) => ValidateRowValues(node.RowValues);

        private void ValidateRowValues(IList<RowValue> rowValues)
        {
            if (null == rowValues)
            {
                return;
            }

            if (rowValues.Count <= 1)
            {
                return;
            }

            int expectedCount = rowValues[0].ColumnValues.Count;
            int n = rowValues.Count;

            for (int i = 1; i < n; i++)
            {
                if (rowValues[i].ColumnValues.Count != expectedCount)
                {
                    HandleNodeError(rowValues[i]);
                }
            }
        }
    }
}
