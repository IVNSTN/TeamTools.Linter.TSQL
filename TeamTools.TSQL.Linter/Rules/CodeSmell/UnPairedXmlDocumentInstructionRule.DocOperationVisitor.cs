using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Doc operation visitor.
    /// </summary>
    internal partial class UnPairedXmlDocumentInstructionRule
    {
        private class DocOperationVisitor : TSqlFragmentVisitor
        {
            private static readonly string OpenDocProcName = "sp_xml_preparedocument";

            private static readonly string CloseDocProcName = "sp_xml_removedocument";

            public IList<DocHandleInfo> Calls { get; } = new List<DocHandleInfo>();

            public override void Visit(ExecutableProcedureReference node)
            {
                if (node.Parameters.Count == 0)
                {
                    return;
                }

                string procName = node.ProcedureReference.ProcedureReference?.Name.BaseIdentifier.Value ?? "";
                bool isOpen;

                if (procName.Equals(OpenDocProcName, StringComparison.OrdinalIgnoreCase))
                {
                    isOpen = true;
                }
                else if (procName.Equals(CloseDocProcName, StringComparison.OrdinalIgnoreCase))
                {
                    isOpen = false;
                }
                else
                {
                    return;
                }

                if (node.Parameters[0].ParameterValue is VariableReference varRef
                && !string.IsNullOrEmpty(varRef.Name))
                {
                    Calls.Add(new DocHandleInfo(node, varRef.Name, isOpen));
                }
            }
        }
    }
}
