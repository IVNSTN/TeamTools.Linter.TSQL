using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DE0401", "DEPRECATED_UNIT")]
    internal class DeprecatedUnitReferenceRule : AbstractRule, IDeprecationHandler
    {
        private readonly IDictionary<string, string> deprecations = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public DeprecatedUnitReferenceRule() : base()
        {
        }

        public void LoadDeprecations(IDictionary<string, string> values)
        {
            deprecations.Clear();
            foreach (var v in values)
            {
                deprecations.Add(v.Key, v.Value);
            }
        }

        public override void Visit(FunctionCall node)
        {
            // because node.FunctionName.Value contains single-word name only; no schema and so on
            var funcRef = new FunctionReferenceVisitor(
                node.Parameters != null && node.Parameters.Count > 0 // to avoid catching nested identifiers
                ? node.Parameters[0].FirstTokenIndex
                : node.LastTokenIndex);
            node.Accept(funcRef);

            string refName = funcRef.Name;
            if (!refName.Contains(TSqlDomainAttributes.NamePartSeparator))
            {
                refName = string.Concat(TSqlDomainAttributes.DefaultSchemaPrefix, refName);
            }
            else
            {
                var refNameParts = refName.Split(TSqlDomainAttributes.NamePartSeparator);
                if (refNameParts.Length > 2)
                {
                    refName = string.Join(TSqlDomainAttributes.NamePartSeparator, refNameParts.TakeLast(2));
                }
            }

            CheckReferenceForDeprecation(refName, node);
        }

        public override void Visit(NamedTableReference node)
            => CheckReferenceForDeprecation(node.SchemaObject?.GetFullName(), node);

        public override void Visit(ExecutableProcedureReference node)
            => CheckReferenceForDeprecation(node.ProcedureReference.ProcedureReference?.Name.GetFullName(), node);

        protected void CheckReferenceForDeprecation(string refName, TSqlFragment node)
        {
            if (string.IsNullOrEmpty(refName))
            {
                return;
            }

            refName = refName.Replace("[", "").Replace("]", "");
            if (!deprecations.ContainsKey(refName))
            {
                return;
            }

            HandleNodeError(node, string.Concat(refName, ", ", deprecations[refName]));
        }
    }
}
