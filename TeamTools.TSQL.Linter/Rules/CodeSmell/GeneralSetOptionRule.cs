using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0747", "GENERAL_SET_CONTROL")]
    internal sealed class GeneralSetOptionRule : AbstractRule
    {
        private static readonly Dictionary<GeneralSetCommandType, string> SetOptionNames = new Dictionary<GeneralSetCommandType, string>
        {
            { GeneralSetCommandType.ContextInfo, "CONTEXT_INFO" },
            { GeneralSetCommandType.DateFormat, "DATEFORMAT" },
            { GeneralSetCommandType.DateFirst, "DATEFIRST" },
            { GeneralSetCommandType.DeadlockPriority, "DEADLOCK_PRIORITY" },
            { GeneralSetCommandType.Language, "LANGUAGE" },
            { GeneralSetCommandType.LockTimeout, "LOCK_TIMEOUT" },
            { GeneralSetCommandType.QueryGovernorCostLimit, "QUERY_GOVERNOR_COST_LIMIT" },
        };

        public GeneralSetOptionRule() : base()
        {
        }

        public override void Visit(GeneralSetCommand node)
        {
            // Ignored command types are handled by separate rules
            if (!IsIgnorable(node.CommandType))
            {
                HandleNodeError(node, SetOptionNames[node.CommandType]);
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
