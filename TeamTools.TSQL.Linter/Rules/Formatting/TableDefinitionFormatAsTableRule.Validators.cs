using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    internal partial class TableDefinitionFormatAsTableRule
    {
        private sealed class ColumnsFormattingValidator : FormattingValidator<ColumnDefinition, ValueTuple<int, int>>
        {
            public ColumnsFormattingValidator(Action<TSqlFragment, string> callback) : base(callback)
            { }

            protected override string GetItemName(ColumnDefinition item) => item.ColumnIdentifier.Value;

            protected override (int, int) GetItemPos(ColumnDefinition item) => (item.ColumnIdentifier.StartColumn, GetDataTypePosition(item));

            protected override (int, int) OffsetItemPos((int, int) pos) => (pos.Item1 + LeadingCommaOffset, pos.Item2);

            protected override bool IsValidItemPos(ColumnDefinition item, (int, int) canonicalPos)
            {
                // column data type must be on the same line as the col name
                return base.IsValidItemPos(item, canonicalPos)
                    && (item.DataType is null
                        || item.DataType.StartLine == item.ColumnIdentifier.StartLine);
            }

            private static int GetDataTypePosition(ColumnDefinition item)
            {
                if (item.DataType != null)
                {
                    return item.DataType.StartColumn;
                }

                // computed col does not have DataType property
                // looking for "AS" keyword
                for (int i = item.ColumnIdentifier.FirstTokenIndex, last = item.ComputedColumnExpression.FirstTokenIndex; i < last; i++)
                {
                    var token = item.ScriptTokenStream[i];

                    if (token.TokenType == TSqlTokenType.As)
                    {
                        return token.Column;
                    }
                }

                // broken syntax
                return 0;
            }
        }

        private sealed class TableConstraintsFormattingValidator : FormattingValidator<ConstraintDefinition, int>
        {
            public TableConstraintsFormattingValidator(Action<TSqlFragment, string> callback) : base(callback)
            { }

            protected override string GetItemName(ConstraintDefinition item) => item.ConstraintIdentifier?.Value;

            protected override int GetItemPos(ConstraintDefinition item) => item.StartColumn;

            // A constraint cannot be the first item in table definition
            protected override int OffsetItemPos(int pos) => pos;

            protected override bool IsValidItemPos(ConstraintDefinition item, int canonicalPos)
            {
                // constraint name must be on the same line as the constraint definition start
                return base.IsValidItemPos(item, canonicalPos)
                    && (item.ConstraintIdentifier is null || item.ConstraintIdentifier.StartLine == item.StartLine);
            }
        }

        private sealed class TableIndexesFormattingValidator : FormattingValidator<IndexDefinition, int>
        {
            public TableIndexesFormattingValidator(Action<TSqlFragment, string> callback) : base(callback)
            { }

            protected override string GetItemName(IndexDefinition item) => item.Name.Value;

            protected override int GetItemPos(IndexDefinition item) => item.StartColumn;

            // An index cannot be the first item in table definition
            protected override int OffsetItemPos(int pos) => pos;

            protected override bool IsValidItemPos(IndexDefinition item, int canonicalPos)
            {
                // index name must be on the same line as the constraint definition start
                return base.IsValidItemPos(item, canonicalPos)
                    && item.Name.StartLine == item.StartLine;
            }
        }
    }
}
