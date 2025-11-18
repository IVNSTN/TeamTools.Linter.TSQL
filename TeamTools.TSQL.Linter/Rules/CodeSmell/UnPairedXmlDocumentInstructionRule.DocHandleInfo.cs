using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// DocHandleInfo.
    /// </summary>
    internal partial class UnPairedXmlDocumentInstructionRule
    {
        private class DocHandleInfo
        {
            public DocHandleInfo(TSqlFragment node, string variable, bool isOpen)
            {
                this.Node = node;
                this.Variable = variable;
                this.IsOpen = isOpen;
            }

            public TSqlFragment Node { get; }

            public string Variable { get; }

            public bool IsOpen { get; }
        }
    }
}
