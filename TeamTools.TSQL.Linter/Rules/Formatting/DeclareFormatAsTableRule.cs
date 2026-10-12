using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0210", "DECLARE_FORMAT")]
    internal sealed class DeclareFormatAsTableRule : BaseTableFormatControlRule
    {
        private readonly DeclareFormattingValidator validator;

        public DeclareFormatAsTableRule() : base()
        {
            validator = new DeclareFormattingValidator(ViolationHandlerWithMessage);
        }

        public override void Visit(ProcedureStatementBody node) => validator.ValidateFormatting(node, node.Parameters, LeadingComma);

        public override void Visit(DeclareVariableStatement node) => validator.ValidateFormatting(node, node.Declarations, LeadingComma);

        private sealed class DeclareFormattingValidator : FormattingValidator<DeclareVariableElement, ValueTuple<int, int>>
        {
            public DeclareFormattingValidator(Action<TSqlFragment, string> callback) : base(callback)
            { }

            protected override string GetItemName(DeclareVariableElement item) => item.VariableName.Value;

            // TODO : "=" and the init value of declared element - too?
            protected override (int, int) GetItemPos(DeclareVariableElement item) => (item.VariableName.StartColumn, item.DataType.StartColumn);

            protected override (int, int) OffsetItemPos((int, int) pos) => (pos.Item1 + LeadingCommaOffset, pos.Item2);
        }
    }
}
