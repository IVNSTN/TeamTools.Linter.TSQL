using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("PF0878", "SINGLE_USER_MODE_ZONE_FORCED")]
    internal sealed class SingleUserModeZoneForcedRule : AbstractRule
    {
        public SingleUserModeZoneForcedRule() : base()
        {
        }

        public override void Visit(ProcedureReference node)
        {
            if (node.Name is null)
            {
                // EXEC @var
                return;
            }

            if (node.Name.BaseIdentifier.Value.Equals("sp_getapplock", StringComparison.OrdinalIgnoreCase)
            && (node.Name.SchemaIdentifier is null
                || node.Name.SchemaIdentifier.Value.Equals(TSqlDomainAttributes.SystemSchemaName, StringComparison.OrdinalIgnoreCase)))
            {
                HandleNodeError(node, "APP LOCK");
            }
        }

        public override void Visit(TableHint node)
        {
            if (node.HintKind == TableHintKind.TabLockX)
            {
                HandleNodeError(node, "TABLOCKX");
            }
        }

        public override void Visit(NamedTableReference node)
        {
            DetectBadHintCombination(node.TableHints);
        }

        public override void Visit(StatementWithCtesAndXmlNamespaces node)
        {
            for (int i = node.OptimizerHints.Count - 1; i >= 0; i--)
            {
                if (node.OptimizerHints[i] is TableHintsOptimizerHint hints)
                {
                    DetectBadHintCombination(hints.TableHints);
                }
            }
        }

        // TODO : ForceScan + X-Lock? HoldLock / Serializable?
        // TabLock + [UpdLock | XLock]
        private void DetectBadHintCombination(IList<TableHint> hints)
        {
            TableHint tabLock = null;
            TableHint badHint = null;

            for (int i = hints.Count - 1; i >= 0; i--)
            {
                var hint = hints[i];

                // Ignoring TABLOCKX here because there is a dedicated visitor method above
                if (hint.HintKind == TableHintKind.TabLock)
                {
                    tabLock = hint;
                }
                else if (hint.HintKind == TableHintKind.UpdLock
                || hint.HintKind == TableHintKind.XLock)
                {
                    badHint = hint;
                }
            }

            if (tabLock != null && badHint != null)
            {
                HandleNodeError(tabLock, badHint.HintKind.ToString());
            }
        }
    }
}
