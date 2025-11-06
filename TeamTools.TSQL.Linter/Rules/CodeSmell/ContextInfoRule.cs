using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0522", "CONTEXT_INFO")]
    internal sealed class ContextInfoRule : AbstractRule
    {
        public ContextInfoRule() : base()
        {
        }

        public override void Visit(FunctionCall node)
        {
            if (string.Equals(node.FunctionName.Value, "CONTEXT_INFO", StringComparison.OrdinalIgnoreCase))
            {
                HandleNodeError(node);
            }
        }

        public override void Visit(GeneralSetCommand node)
        {
            if (node.CommandType == GeneralSetCommandType.ContextInfo)
            {
                HandleNodeError(node);
            }
        }
    }
}
