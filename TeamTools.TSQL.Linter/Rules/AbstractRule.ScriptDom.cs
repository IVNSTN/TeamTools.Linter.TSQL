using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.TSQL.Linter.Interfaces;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// ScriptDom integration.
    /// </summary>
    public partial class AbstractRule : TSqlFragmentVisitor, ISqlRule
    {
        public void Validate(TSqlFragment node)
        {
            if (node is TSqlScript script)
            {
                ValidateScript(script);
            }
            else
            {
                // default behavior is same as before
                node.Accept(this);
            }
        }

        protected virtual void ValidateScript(TSqlScript script)
        {
            int n = script.Batches.Count;
            for (int i = 0; i < n; i++)
            {
                // default behavior is same as before
                ValidateBatch(script.Batches[i]);
            }
        }

        protected virtual void ValidateBatch(TSqlBatch batch)
        {
            // default behavior is same as before
            for (int i = 0, n = batch.Statements.Count; i < n; i++)
            {
                batch.Statements[i].Accept(this);
            }
        }
    }
}
