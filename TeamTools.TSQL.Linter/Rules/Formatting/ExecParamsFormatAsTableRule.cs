using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0211", "EXEC_FORMAT")]
    internal sealed class ExecParamsFormatAsTableRule : BaseTableFormatControlRule
    {
        private readonly ExecParamsFormattingValidator validator;

        public ExecParamsFormatAsTableRule() : this(true)
        {
        }

        // for testing purpose thus comma position can be explicitly provided
        public ExecParamsFormatAsTableRule(bool leadingComma) : base(leadingComma)
        {
            validator = new ExecParamsFormattingValidator(ViolationHandlerWithMessage);
        }

        public override void Visit(ExecutableProcedureReference node)
        {
            if (node.ProcedureReference.ProcedureVariable is null
            && SystemProcDetector.IsSystemProc(node.ProcedureReference.ProcedureReference.Name.BaseIdentifier.Value))
            {
                // ignoring system proc calls
                return;
            }

            validator.ValidateFormatting(node, node.Parameters, LeadingComma);
        }

        private sealed class ExecParamsFormattingValidator : FormattingValidator<ExecuteParameter, int>
        {
            public ExecParamsFormattingValidator(Action<TSqlFragment, string> callback) : base(callback)
            { }

            protected override string GetItemName(ExecuteParameter item) => item.Variable?.Name ?? default;

            // if parameter passed by position, not by name, then using the value pos
            protected override int GetItemPos(ExecuteParameter item) => item.Variable?.StartColumn ?? item.ParameterValue.StartColumn;

            protected override int OffsetItemPos(int pos) => pos + LeadingCommaOffset;
        }
    }
}
