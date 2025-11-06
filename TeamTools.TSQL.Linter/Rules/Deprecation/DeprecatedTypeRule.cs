using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DE0402", "DEPRECATED_TYPE")]
    internal sealed class DeprecatedTypeRule : AbstractRule, IDeprecationHandler
    {
        private IDictionary<string, string> deprecations = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public DeprecatedTypeRule() : base()
        {
        }

        public void LoadDeprecations(IDictionary<string, string> values)
        {
            deprecations = values.ToDictionary(
                entry => entry.Key,
                entry => entry.Value,
                StringComparer.OrdinalIgnoreCase);
        }

        public override void Visit(DeclareVariableElement node) => DoValidateTypeReference(node.DataType);

        public override void Visit(ColumnDefinition node) => DoValidateTypeReference(node.DataType);

        public override void Visit(ScalarFunctionReturnType node) => DoValidateTypeReference(node.DataType);

        private void DoValidateTypeReference(DataTypeReference dataType)
        {
            if (dataType?.Name is null)
            {
                // CURSOR and such
                return;
            }

            CheckReferenceForDeprecation(dataType.Name.GetFullName(), dataType);
        }

        private void CheckReferenceForDeprecation(string refName, TSqlFragment node)
        {
            refName = refName.Replace("[", "").Replace("]", "");
            if (!deprecations.ContainsKey(refName))
            {
                return;
            }

            HandleNodeError(node, string.Join(", ", refName, deprecations[refName]));
        }
    }
}
