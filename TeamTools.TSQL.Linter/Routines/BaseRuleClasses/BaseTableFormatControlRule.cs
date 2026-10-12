using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.TSQL.Linter.Rules;

namespace TeamTools.TSQL.Linter.Routines
{
    internal abstract class BaseTableFormatControlRule : AbstractRule, ICustomOptionsConsumer
    {
        protected const int LeadingCommaOffset = 2;
        private bool leadingComma;

        protected BaseTableFormatControlRule(bool leadingComma) : base()
        {
            this.leadingComma = leadingComma;
        }

        protected BaseTableFormatControlRule() : this(true)
        {
        }

        protected bool LeadingComma => leadingComma;

        public void LoadCustomOptions(IDictionary<string, string> options)
        {
            if (options.TryGetValue("list-comma-position", out string commaPosition))
            {
                leadingComma = commaPosition.Equals("leading", System.StringComparison.OrdinalIgnoreCase);
            }
        }

        protected abstract class FormattingValidator<TItem, TPos> : TSqlFragmentVisitor
        where TItem : TSqlFragment
        where TPos : struct
        {
            protected FormattingValidator(Action<TSqlFragment, string> callback)
            {
                Callback = callback;
            }

            protected Action<TSqlFragment, string> Callback { get; }

            public TPos GetCanonicalItemPos<TListItem>(TSqlFragment node, IList<TListItem> items, bool useLeadingComma)
            where TListItem : TItem
            {
                TPos canonicalPos = GetItemPos(items[0]);
                bool actuallyLeadingComma = VerifyActualCommaPosition(node, items, useLeadingComma);

                if (!actuallyLeadingComma)
                {
                    return canonicalPos;
                }

                // next items are supposed to be prepended with ", " thus
                // expected position has to be increased accordingly
                return OffsetItemPos(canonicalPos);
            }

            public void ValidateFormatting<TNode, TListItem>(TNode node, IList<TListItem> items, TPos canonicalPos, bool skipFirstItem = false)
            where TNode : TSqlFragment
            where TListItem : TItem
            {
                if (CanBeIgnored(node, items, false))
                {
                    return;
                }

                var badItem = DetectViolation(items, canonicalPos, skipFirstItem);

                if (badItem is null)
                {
                    return;
                }

                Callback(badItem, GetItemName(badItem));
            }

            public void ValidateFormatting<TNode, TListItem>(TNode node, IList<TListItem> items, bool useLeadingComma)
            where TNode : TSqlFragment
            where TListItem : TItem
            {
                if (CanBeIgnored(node, items))
                {
                    return;
                }

                var canonicalPos = GetCanonicalItemPos(node, items, useLeadingComma);

                ValidateFormatting(node, items, canonicalPos, true);
            }

            // It returns a user-friendly name of a formatting violating item in a list.
            protected abstract string GetItemName(TItem item);

            // It returns list item offset position.
            protected abstract TPos GetItemPos(TItem item);

            // It offsets a position with LeadingCommaOffset value for the very first list item if LeadingComma enabled.
            protected abstract TPos OffsetItemPos(TPos pos);

            protected virtual bool IsValidItemPos(TItem item, TPos canonicalPos)
            {
                TPos anotherItemPos = GetItemPos(item);
                return anotherItemPos.Equals(canonicalPos);
            }

            private static bool CanBeIgnored<TListItem>(TSqlFragment node, IList<TListItem> items, bool skipSingleItem = true)
            where TListItem : TItem
            {
                if (node is null || items is null)
                {
                    // to prevent possible null-reference error in case of empty node body
                    return true;
                }

                if (items.Count < 2 && skipSingleItem)
                {
                    // single item cannot be formatted as table
                    return true;
                }

                if (node.StartLine == node.ScriptTokenStream[node.LastTokenIndex].Line)
                {
                    // ignoring one-line definition
                    return true;
                }

                return false;
            }

            private static bool VerifyActualCommaPosition<TListItem>(TSqlFragment node, IList<TListItem> items, bool defaultValue)
            where TListItem : TItem
            {
                var firstItem = items[0];
                int lastToken;
                int nextLineForLeadingComma;

                if (items.Count > 1)
                {
                    var secondItem = items[1];
                    lastToken = secondItem.FirstTokenIndex;

                    if (secondItem.StartLine == firstItem.StartLine)
                    {
                        // invalid formatting anyways
                        return defaultValue;
                    }

                    nextLineForLeadingComma = secondItem.StartLine;
                }
                else
                {
                    lastToken = node.LastTokenIndex;
                    nextLineForLeadingComma = firstItem.ScriptTokenStream[firstItem.LastTokenIndex].Line + 1;
                }

                // looking for actual "," position
                for (int i = firstItem.LastTokenIndex + 1; i < lastToken; i++)
                {
                    var token = node.ScriptTokenStream[i];
                    if (!ScriptDomExtension.IsSkippableTokens(token.TokenType))
                    {
                        if (token.Text.Equals(","))
                        {
                            // If the comma between 1st and 2nd list items is on the line with the 2nd one
                            // then this is leading-comma formatting.
                            return token.Line == nextLineForLeadingComma;
                        }

                        break;
                    }
                }

                return defaultValue;
            }

            // It runs through item list and detects wrong item positioning (offset).
            private TItem DetectViolation<TListItem>(IList<TListItem> items, TPos canonicalPos, bool skipFirstItem)
            where TListItem : TItem
            {
                // skipping first item if it is used as a standard
                for (int i = skipFirstItem ? 1 : 0, n = items.Count; i < n; i++)
                {
                    var item = items[i];
                    if (!IsValidItemPos(item, canonicalPos))
                    {
                        // item position does not match tabular formatting pattern
                        return item;
                    }
                }

                return default;
            }
        }
    }
}
