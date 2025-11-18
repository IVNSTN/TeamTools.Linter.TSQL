using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0182", "ROWVERSION_NOT_NULL")]
    internal sealed class RowVersionColumnNullableRule : AbstractRule
    {
        private static readonly HashSet<string> RowVersionAliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ROWVERSION",
            "TIMESTAMP",
        };

        public RowVersionColumnNullableRule() : base()
        {
        }

        public override void Visit(ColumnDefinition node)
        {
            // CURSOR does not have type name
            var typeName = node.DataType?.Name?.BaseIdentifier.Value;

            if (string.IsNullOrEmpty(typeName) || !RowVersionAliases.Contains(typeName))
            {
                return;
            }

            var csnull = node.Constraints
                .OfType<NullableConstraintDefinition>()
                .FirstOrDefault();

            if (csnull != null && !csnull.Nullable)
            {
                return;
            }

            HandleNodeError(node);
        }
    }
}
