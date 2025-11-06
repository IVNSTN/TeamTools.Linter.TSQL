using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("AM0718", "OBJECT_ID_WITHOUT_TYPE")]
    internal sealed class ObjectIdNeedsTypeRule : AbstractRule
    {
        private static readonly string FunctionName = "OBJECT_ID";

        public ObjectIdNeedsTypeRule() : base()
        {
        }

        public override void Visit(FunctionCall node)
        {
            if (string.Equals(FunctionName, node.FunctionName.Value, StringComparison.OrdinalIgnoreCase)
            && node.Parameters.Count < 2)
            {
                HandleNodeError(node);
            }
        }
    }
}
