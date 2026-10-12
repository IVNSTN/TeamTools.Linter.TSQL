using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0223", "SELECTED_ITEMS_ALIGN")]
    internal sealed class SelectedItemsAlignedRule : BaseTableFormatControlRule
    {
        private readonly SelectedItemsFormattingValidator validator;

        public SelectedItemsAlignedRule() : base()
        {
            validator = new SelectedItemsFormattingValidator(ViolationHandlerWithMessage);
        }

        public override void Visit(QuerySpecification node) => validator.ValidateFormatting(node, node.SelectElements, LeadingComma);

        private sealed class SelectedItemsFormattingValidator : FormattingValidator<SelectElement, int>
        {
            public SelectedItemsFormattingValidator(Action<TSqlFragment, string> callback) : base(callback)
            { }

            protected override string GetItemName(SelectElement item)
            {
                if (item is SelectSetVariable selset)
                {
                    return selset.Variable.Name;
                }
                else if (item is SelectScalarExpression expr && expr.ColumnName != null)
                {
                    return expr.ColumnName.Value;
                }

                return default;
            }

            protected override int GetItemPos(SelectElement item) => item.StartColumn;

            protected override int OffsetItemPos(int pos) => pos + LeadingCommaOffset;
        }
    }
}
