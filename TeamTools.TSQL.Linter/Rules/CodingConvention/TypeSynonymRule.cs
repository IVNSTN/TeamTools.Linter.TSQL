using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Properties;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0788", "TYPE_SYNONYM")]
    internal sealed class TypeSynonymRule : AbstractRule, ISqlServerMetadataConsumer
    {
        private Dictionary<string, string> synonymReplacement;

        public TypeSynonymRule() : base()
        {
        }

        public void LoadMetadata(SqlServerMetadata data)
        {
            synonymReplacement = data.Types
                .Where(t => !string.IsNullOrEmpty(t.Value.AlsoKnownAs) && t.Value.ForceToOriginalName)
                .ToDictionary(t => t.Key, t => t.Value.AlsoKnownAs, StringComparer.OrdinalIgnoreCase);
        }

        public override void Visit(DataTypeReference node)
        {
            if (node.Name is null)
            {
                // e.g. CURSOR
                return;
            }

            if (node.Name.SchemaIdentifier != null
            && !string.Equals(node.Name.SchemaIdentifier.Value, TSqlDomainAttributes.DefaultSchemaName, StringComparison.OrdinalIgnoreCase))
            {
                // user defined type
                return;
            }

            string actualName = node.Name.BaseIdentifier.Value;

            if (string.IsNullOrEmpty(actualName) || !synonymReplacement.TryGetValue(actualName, out string conventionalName))
            {
                // normal or unknown type
                return;
            }

            HandleNodeError(node.Name, string.Format(Strings.ViolationDetails_TypeSynonymRule_ExpectedAnother, conventionalName, actualName));
        }
    }
}
