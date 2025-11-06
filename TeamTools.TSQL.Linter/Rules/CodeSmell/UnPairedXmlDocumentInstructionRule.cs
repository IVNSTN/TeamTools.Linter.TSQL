using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0921", "UNPAIRED_XMLDOC_STATEMENT")]
    internal sealed class UnPairedXmlDocumentInstructionRule : AbstractRule
    {
        public UnPairedXmlDocumentInstructionRule() : base()
        {
        }

        public override void Visit(TSqlBatch node)
        {
            ValidatePairedCalls(node);
        }

        private void ValidatePairedCalls(TSqlFragment node)
        {
            var callVisitor = new CallVisitor();
            node.AcceptChildren(callVisitor);

            if (!callVisitor.Calls.Any())
            {
                return;
            }

            var openedVars = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var call in callVisitor.Calls)
            {
                if (call.IsOpen)
                {
                    if (openedVars.Contains(call.Variable, StringComparer.OrdinalIgnoreCase))
                    {
                        HandleNodeError(call.Node, call.Variable);
                        continue;
                    }

                    openedVars.Add(call.Variable);
                }
                else
                {
                    if (openedVars.Count == 0)
                    {
                        HandleNodeError(call.Node, call.Variable);
                        continue;
                    }

                    if (openedVars.Contains(call.Variable))
                    {
                        openedVars.Remove(call.Variable);
                        continue;
                    }

                    HandleNodeError(call.Node, call.Variable);
                }
            }

            foreach (var loosVar in openedVars)
            {
                HandleNodeError(node, loosVar);
            }
        }

        private class CallInfo
        {
            public CallInfo(TSqlFragment node, string variable, bool isOpen)
            {
                this.Node = node;
                this.Variable = variable;
                this.IsOpen = isOpen;
            }

            public TSqlFragment Node { get; private set; }

            public string Variable { get; private set; }

            public bool IsOpen { get; private set; }
        }

        private class CallVisitor : TSqlFragmentVisitor
        {
            private static readonly string OpenDocProcName = "sp_xml_preparedocument";

            private static readonly string CloseDocProcName = "sp_xml_removedocument";

            public IList<CallInfo> Calls { get; private set; } = new List<CallInfo>();

            public override void Visit(ExecutableProcedureReference node)
            {
                if (node.Parameters.Count == 0)
                {
                    return;
                }

                string procName = node.ProcedureReference.ProcedureReference?.Name.BaseIdentifier.Value ?? "";
                bool isOpen;
                string varName = null;

                if (procName.Equals(OpenDocProcName))
                {
                    isOpen = true;

                    if (node.Parameters[0].ParameterValue is VariableReference varRef)
                    {
                        varName = varRef.Name;
                    }
                }
                else
                if (procName.Equals(CloseDocProcName))
                {
                    isOpen = false;

                    if (node.Parameters[0].ParameterValue is VariableReference varRef)
                    {
                        varName = varRef.Name;
                    }
                }
                else
                {
                    return;
                }

                if (varName == null)
                {
                    return;
                }

                Calls.Add(new CallInfo(node, varName, isOpen));
            }
        }
    }
}
