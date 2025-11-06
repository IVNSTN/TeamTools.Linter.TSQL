using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0211", "EXEC_FORMAT")]
    internal sealed class ExecParamsFormatAsTableRule : AbstractRule
    {
        private readonly SystemProcDetector systemProcDetector = new SystemProcDetector();

        public ExecParamsFormatAsTableRule() : base()
        {
        }

        public override void Visit(ExecutableProcedureReference node)
        {
            if (node.Parameters.Count <= 1)
            {
                return;
            }

            if (node.ProcedureReference.ProcedureVariable == null
            && systemProcDetector.IsSystemProc(node.ProcedureReference.ProcedureReference.Name.BaseIdentifier.Value))
            {
                return;
            }

            // single-line syntax
            if (node.StartLine == node.Parameters.Last().StartLine)
            {
                return;
            }

            var varVisitor = new ParamVisitor();
            node.Accept(varVisitor);

            if (IsCorrectFormat(varVisitor, out string details))
            {
                return;
            }

            HandleNodeError(varVisitor.FirstBadOne, details);
        }

        private static bool IsCorrectFormat(ParamVisitor formatData, out string details)
        {
            if (formatData.ParamPositions.Count > 1)
            {
                details = formatData.ParamPositions.Values.Last();
            }
            else
            {
                details = "";
                return true;
            }

            return false;
        }

        private class ParamVisitor : TSqlFragmentVisitor
        {
            private readonly bool leadingComma;

            public ParamVisitor(bool leadingComma = true) : base()
            {
                this.leadingComma = leadingComma;
            }

            public IDictionary<int, string> ParamPositions { get; } = new Dictionary<int, string>();

            public TSqlFragment FirstBadOne { get; private set; }

            public override void Visit(ExecuteParameter node)
            {
                if (node.Variable == null)
                {
                    return;
                }

                string name = node.Variable.Name;
                int varPos = node.StartColumn;

                if (ParamPositions.Count == 0 && leadingComma)
                {
                    // next vars have ", "
                    varPos += 2;
                }

                if (!ParamPositions.ContainsKey(varPos))
                {
                    ParamPositions.Add(varPos, name);

                    // once it became multiple
                    if (ParamPositions.Count == 2)
                    {
                        FirstBadOne = node;
                    }
                }
                else
                {
                    ParamPositions[varPos] = string.Concat(ParamPositions[varPos], ",", name);
                }
            }
        }
    }
}
