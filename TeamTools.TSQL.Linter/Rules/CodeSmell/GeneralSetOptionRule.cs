using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0747", "GENERAL_SET_CONTROL")]
    internal sealed class GeneralSetOptionRule : AbstractRule
    {
        public GeneralSetOptionRule() : base()
        {
        }

        public override void Visit(GeneralSetCommand node)
        {
            // Ignored command types are handled by separate rules
            if (!IsIgnorable(node.CommandType))
            {
                HandleNodeError(node);
            }
        }

        private static bool IsIgnorable(GeneralSetCommandType cmdType)
        {
            return cmdType == GeneralSetCommandType.None
                || cmdType == GeneralSetCommandType.DateFormat
                || cmdType == GeneralSetCommandType.DateFirst
                || cmdType == GeneralSetCommandType.ContextInfo;
        }
    }
}
