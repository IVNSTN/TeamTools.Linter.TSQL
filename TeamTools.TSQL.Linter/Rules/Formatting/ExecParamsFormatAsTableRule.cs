using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0211", "EXEC_FORMAT")]
    internal sealed class ExecParamsFormatAsTableRule : AbstractRule
    {
        private static readonly bool LeadingComma = true; // TODO : Take from config!

        public ExecParamsFormatAsTableRule() : base()
        {
        }

        public override void Visit(ExecutableProcedureReference node)
        {
            if (node.Parameters.Count <= 1)
            {
                // single item cannot be formatted as table
                return;
            }

            if (node.ProcedureReference.ProcedureVariable is null
            && SystemProcDetector.IsSystemProc(node.ProcedureReference.ProcedureReference.Name.BaseIdentifier.Value))
            {
                return;
            }

            // ignoring single-line calls
            if (node.StartLine == node.Parameters[node.Parameters.Count - 1].StartLine)
            {
                return;
            }

            DoValidate(node.Parameters);
        }

        private static ExecuteParameter DetectViolation(IList<ExecuteParameter> items, bool leadingComma)
        {
            int paramStartCol = -1;
            int valueStartCol = -1;
            int n = items.Count;

            for (int i = 0; i < n; i++)
            {
                var item = items[i];
                if (i == 0)
                {
                    if (item.Variable is null)
                    {
                        // If the first arg is passed by position with no name
                        // then ignoring such call. There is another rule to
                        // forbid such syntax.
                        return default;
                    }

                    paramStartCol = item.Variable.StartColumn;
                    valueStartCol = item.ParameterValue.StartColumn;
                    if (leadingComma)
                    {
                        // next vars be prepended with ", " thus
                        // expected position has to be increased accordingly
                        paramStartCol += 2;
                    }
                }
                else if (paramStartCol != (item.Variable?.StartColumn ?? paramStartCol))
                {
                    // Item position does not match tabular formatting pattern.
                    // Parameter value and '=' sign don't have to be formatted as table.
                    return item;
                }
            }

            return default;
        }

        private void DoValidate(IList<ExecuteParameter> items)
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

            HandleNodeError(badItem, badItem.Variable?.Name);
        }
    }
}
