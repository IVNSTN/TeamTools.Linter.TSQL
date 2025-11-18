using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Doc handle state analysis.
    /// </summary>
    internal partial class UnPairedXmlDocumentInstructionRule
    {
        private void ValidatePairedCalls(TSqlFragment node)
        {
            var callVisitor = new DocOperationVisitor();
            node.AcceptChildren(callVisitor);

            if (callVisitor.Calls.Count == 0)
            {
                return;
            }

            var openedVars = new Dictionary<string, TSqlFragment>(StringComparer.OrdinalIgnoreCase);

            int n = callVisitor.Calls.Count;
            for (int i = 0; i < n; i++)
            {
                var call = callVisitor.Calls[i];

                if (call.IsOpen)
                {
                    if (!openedVars.TryAdd(call.Variable, call.Node))
                    {
                        // doc is already open
                        HandleNodeError(call.Node, call.Variable);
                    }
                }
                else if (!openedVars.Remove(call.Variable))
                {
                    // doc is not opened to be closed
                    HandleNodeError(call.Node, call.Variable);
                }
            }

            foreach (var loosVar in openedVars)
            {
                // doc was opened and never closed
                HandleNodeError(loosVar.Value, loosVar.Key);
            }
        }
    }
}
