using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0210", "DECLARE_FORMAT")]
    internal sealed class DeclareFormatAsTableRule : AbstractRule
    {
        private static readonly bool LeadingComma = true; // TODO : Take from config!

        public DeclareFormatAsTableRule() : base()
        {
        }

        public override void Visit(ProcedureStatementBody node) => DoValidate(node.Parameters);

        public override void Visit(DeclareVariableStatement node) => DoValidate(node.Declarations);

        private static DeclareVariableElement DetectViolation<T>(IList<T> items, bool leadingComma)
        where T : DeclareVariableElement
        {
            int nameStartCol = -1;
            int typeStartCol = -1;
            int n = items.Count;

            for (int i = 0; i < n; i++)
            {
                var item = items[i];
                if (i == 0)
                {
                    nameStartCol = item.VariableName.StartColumn;
                    typeStartCol = item.DataType.StartColumn;
                    if (leadingComma)
                    {
                        // next vars be prepended with ", " thus
                        // expected position has to be increased accordingly
                        nameStartCol += 2;
                    }
                }
                else if (nameStartCol != item.VariableName.StartColumn
                    || typeStartCol != item.DataType.StartColumn)
                {
                    // item position does not match tabular formatting pattern
                    return item;
                }
            }

            return default;
        }

        private void DoValidate<T>(IList<T> items)
        where T : DeclareVariableElement
        {
            if (items.Count <= 1)
            {
                // single item cannot be formatted as table
                return;
            }

            var badItem = DetectViolation(items, LeadingComma);

            if (badItem is null)
            {
                return;
            }

            HandleNodeError(badItem, badItem.VariableName.Value);
        }
    }
}
