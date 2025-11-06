using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0747", "GENERAL_SET_CONTROL")]
    internal sealed class GeneralSetOptionRule : AbstractRule
    {
        private readonly ICollection<GeneralSetCommandType> ignoredTypes = new List<GeneralSetCommandType>
        {
            GeneralSetCommandType.None,
            GeneralSetCommandType.DateFormat,
            GeneralSetCommandType.DateFirst,
            GeneralSetCommandType.ContextInfo,
        };

        public GeneralSetOptionRule() : base()
        {
        }

        public override void Visit(GeneralSetCommand node)
        {
            // Ignored command types are handled by separate rules
            if (!ignoredTypes.Contains(node.CommandType))
            {
                HandleNodeError(node);
            }
        }
    }
}
