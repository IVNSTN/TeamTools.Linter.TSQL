using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0210", "DECLARE_FORMAT")]
    internal sealed class DeclareFormatAsTableRule : AbstractRule
    {
        public DeclareFormatAsTableRule() : base()
        {
        }

        public override void Visit(ProcedureStatementBody node)
        {
            if (node.Parameters.Count <= 1)
            {
                return;
            }

            var varVisitor = new VariableVisitor();
            foreach (DeclareVariableElement param in node.Parameters)
            {
                varVisitor.Visit(param);
            }

            if (IsCorrectFormat(varVisitor, out string details))
            {
                return;
            }

            HandleNodeError(varVisitor.FirstBadOne, details);
        }

        public override void Visit(DeclareVariableStatement node)
        {
            var varVisitor = new VariableVisitor();
            node.AcceptChildren(varVisitor);

            if (IsCorrectFormat(varVisitor, out string details))
            {
                return;
            }

            HandleNodeError(varVisitor.FirstBadOne, details);
        }

        private static bool IsCorrectFormat(VariableVisitor formatData, out string details)
        {
            if (formatData.VariablePositions.Count > 1)
            {
                details = formatData.VariablePositions.Values.Last();
            }
            else if (formatData.DatatypePositions.Count > 1)
            {
                details = formatData.DatatypePositions.Values.Last();
            }
            else
            {
                details = "";
                return true;
            }

            return false;
        }

        private class VariableVisitor : TSqlFragmentVisitor
        {
            private readonly bool leadingComma;

            public VariableVisitor(bool leadingComma = true) : base()
            {
                this.leadingComma = leadingComma;
            }

            public IDictionary<int, string> VariablePositions { get; } = new Dictionary<int, string>();

            public IDictionary<int, string> DatatypePositions { get; } = new Dictionary<int, string>();

            public TSqlFragment FirstBadOne { get; private set; }

            public override void Visit(DeclareVariableElement node)
            {
                string name = node.VariableName.Value.ToLower();
                int varPos = node.StartColumn;
                int typePos = node.DataType.StartColumn;

                if (VariablePositions.Count == 0 && leadingComma)
                {
                    // next vars have ", "
                    varPos += 2;
                }

                if (!VariablePositions.ContainsKey(varPos))
                {
                    VariablePositions.Add(varPos, name);

                    if (VariablePositions.Count == 2 && FirstBadOne is null)
                    {
                        FirstBadOne = node;
                    }
                }
                else
                {
                    VariablePositions[varPos] = string.Concat(VariablePositions[varPos], ",", name);
                }

                if (!DatatypePositions.ContainsKey(typePos))
                {
                    DatatypePositions.Add(typePos, name);

                    if (DatatypePositions.Count == 2 && FirstBadOne is null)
                    {
                        FirstBadOne = node;
                    }
                }
                else
                {
                    DatatypePositions[typePos] = string.Concat(DatatypePositions[typePos], ",", name);
                }
            }
        }
    }
}
