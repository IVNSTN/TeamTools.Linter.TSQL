using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("HD0212", "SYNONYM_VAR_LINKS")]
    internal sealed class SynonymVariableReferenceRule : AbstractRule
    {
        public SynonymVariableReferenceRule() : base()
        {
        }

        public override void Visit(CreateSynonymStatement node)
        {
            ValidateReferenceIsVariable(node.ForName.DatabaseIdentifier, (nd, msg) => HandleNodeError(nd, $"db name: {msg}"));
            ValidateReferenceIsVariable(node.ForName.ServerIdentifier, (nd, msg) => HandleNodeError(nd, $"server name: {msg}"));
        }

        private static void ValidateReferenceIsVariable(Identifier reference, Action<TSqlFragment, string> callback)
        {
            if (reference is null || reference.Value.StartsWith("$("))
            {
                return;
            }

            callback(reference, reference.Value);
        }
    }
}
