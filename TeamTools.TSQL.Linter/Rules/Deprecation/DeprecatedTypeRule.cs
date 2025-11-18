using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DE0402", "DEPRECATED_TYPE")]
    internal sealed class DeprecatedTypeRule : AbstractRule, IDeprecationHandler
    {
        private Dictionary<string, string> deprecations;

        public DeprecatedTypeRule() : base()
        {
        }

        public void LoadDeprecations(IDictionary<string, string> values)
        {
            deprecations = new Dictionary<string, string>(values, StringComparer.OrdinalIgnoreCase);
        }

        public override void Visit(DeclareVariableElement node) => DoValidateTypeReference(node.DataType);

        public override void Visit(ColumnDefinition node) => DoValidateTypeReference(node.DataType);

        public override void Visit(ScalarFunctionReturnType node) => DoValidateTypeReference(node.DataType);

        private void DoValidateTypeReference(DataTypeReference dataType)
        {
            if (dataType is null)
            {
                // computed col
                return;
            }

            CheckReferenceForDeprecation(dataType.GetFullName(), dataType);
        }

        private void CheckReferenceForDeprecation(string refName, TSqlFragment node)
        {
            if (!deprecations.TryGetValue(refName, out var reason))
            {
                return;
            }

            HandleNodeError(node, $"{refName}, {reason}");
        }
    }
}
