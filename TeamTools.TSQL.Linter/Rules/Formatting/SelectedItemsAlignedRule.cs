using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Linq;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0223", "SELECTED_ITEMS_ALIGN")]
    internal sealed class SelectedItemsAlignedRule : AbstractRule
    {
        private static readonly int MaxViolationsPerSelect = 1;
        private readonly bool leadingComma;

        public SelectedItemsAlignedRule(bool leadingComma) : base()
        {
            this.leadingComma = leadingComma;
        }

        public SelectedItemsAlignedRule() : this(true)
        {
        }

        public override void Visit(QuerySpecification node)
        {
            // ignoring one-liners
            if (node.StartLine == node.ScriptTokenStream[node.LastTokenIndex].Line)
            {
                return;
            }

            if (node.SelectElements.Count < 2)
            {
                return;
            }

            // Note, in some cases 0 item may be differently aligned because of TOP, DISTINCT and so on
            int firstItemIndex = 0;
            int offset = node.SelectElements[firstItemIndex].StartColumn;
            if (leadingComma)
            {
                // the very first element does not have ", " in front of it
                offset += 2;
            }

            var badElements = node.SelectElements
                .Skip(1)
                .Where(item => item.StartColumn != offset)
                .Take(MaxViolationsPerSelect);

            foreach (var element in badElements)
            {
                HandleNodeError(element);
            }
        }
    }
}
