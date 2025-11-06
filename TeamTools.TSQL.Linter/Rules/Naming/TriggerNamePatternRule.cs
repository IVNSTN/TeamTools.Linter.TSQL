using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("NM0962", "TRIGGER_NAME_PATTERN")]
    [TriggerRule]
    internal sealed class TriggerNamePatternRule : AbstractRule
    {
        private const RegexOptions MatchRegexOptions =
            RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.Multiline;

        private const int MinSuffixLength = 3;
        private const int MaxSuffixLength = 24;

        // TODO : letter casing should be checked to conform naming notation
        private static readonly Regex TriggerNamePattern = new Regex(
            @"^_(?<purpose>[a-zA-Z](\w{1,22})[a-zA-Z])$",
            MatchRegexOptions);

        private static readonly Regex InsteadOfTriggerNamePattern = new Regex(
            @"^_instead_(?<purpose>[a-zA-Z](\w{1,22})[a-zA-Z])$",
            MatchRegexOptions);

        private static readonly ICollection<string> HungarianPrefixes;

        static TriggerNamePatternRule()
        {
            HungarianPrefixes = new SortedSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "TR_",
                "TRG_",
                "TI_",
                "TU_",
                "TD_",
                "TIU_",
                "TUD_",
                "TIUD_",
                "IUD_",
                "IU_",
                "UD_",
                "I_",
                "U_",
                "D_",
            };
        }

        public TriggerNamePatternRule() : base()
        {
        }

        public override void Visit(TriggerStatementBody node)
        {
            if (node.TriggerObject.TriggerScope != TriggerScope.Normal)
            {
                // DDL trigger does not have target table definition
                return;
            }

            string triggerName = node.Name.BaseIdentifier.Value;
            string tableSchema = node.TriggerObject.Name.SchemaIdentifier?.Value ?? TSqlDomainAttributes.DefaultSchemaName;
            string tableName = node.TriggerObject.Name.BaseIdentifier.Value;

            ValidateTriggerName(triggerName, tableSchema, tableName, node.TriggerType, node.Name);
        }

        private void ValidateTriggerName(string triggerName, string tableSchema, string tableName, TriggerType triggerType, TSqlFragment node)
        {
            if (HungarianPrefixes.Any(prefix => triggerName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            {
                HandleNodeError(node, "unexpected hungarian prefix");
                return;
            }

            if (triggerName.StartsWith(tableSchema, StringComparison.OrdinalIgnoreCase)
            && !triggerName.StartsWith(tableName, StringComparison.OrdinalIgnoreCase))
            {
                HandleNodeError(node, "unexpected schema prefix");
                return;
            }

            if (!triggerName.StartsWith(tableName, StringComparison.OrdinalIgnoreCase))
            {
                HandleNodeError(node, "missing table name prefix");
                return;
            }

            string triggerSuffix = triggerName.Substring(tableName.Length);
            int maxLength = MaxSuffixLength + (triggerType == TriggerType.InsteadOf ? "_instead_".Length : 0);

            if (triggerSuffix.Length > maxLength)
            {
                HandleNodeError(node, string.Format("suffix is longer {0}", maxLength));
                return;
            }

            if (triggerSuffix.Length < MinSuffixLength)
            {
                HandleNodeError(node, string.Format("suffix is shorter {0}", MinSuffixLength));
                return;
            }

            if (triggerType == TriggerType.InsteadOf)
            {
                if (InsteadOfTriggerNamePattern.IsMatch(triggerSuffix))
                {
                    return;
                }
            }
            else if (TriggerNamePattern.IsMatch(triggerSuffix))
            {
                return;
            }

            HandleNodeError(node, triggerName);
        }
    }
}
