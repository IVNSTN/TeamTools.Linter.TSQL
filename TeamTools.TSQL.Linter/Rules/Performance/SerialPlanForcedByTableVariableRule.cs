using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    // TODO : Not sure if MERGE should be supported by this rule
    // TODO : DELETE @tbl, UPDATE @tbl via alias
    // Table variable modification does not support parallelization
    [RuleIdentity("PF0873", "SERIAL_PLAN_FORCED_TABLE_VAR")]
    internal sealed class SerialPlanForcedByTableVariableRule : AbstractRule
    {
        public override void Visit(OutputIntoClause node) => DetectTableVariableModification(node.IntoTable);

        public override void Visit(InsertSpecification node)
        {
            if (node.InsertSource is ValuesInsertSource)
            {
                // Item by item INSERT-VALUES does not need parallelization
                return;
            }

            if (node.InsertSource is SelectInsertSource sel
            && sel.Select is QuerySpecification q
            && IsSimpleSource(q.FromClause))
            {
                return;
            }

            DetectTableVariableModification(node.Target);
        }

        public override void Visit(UpdateDeleteSpecificationBase node)
        {
            if (IsSimpleSource(node.FromClause))
            {
                return;
            }

            DetectTableVariableModification(node.Target);
        }

        // Natively compiled modules cannot use temp tables thus table variables are the only options
        protected override void ValidateBatch(TSqlBatch batch)
        {
            // CREATE PROC/TRIGGER/FUNC must be the first statement in a batch
            var firstStmt = batch.Statements[0];
            if (firstStmt is ProcedureStatementBody proc)
            {
                if (proc.Options.HasOption(ProcedureOptionKind.NativeCompilation))
                {
                    return;
                }
            }
            else if (firstStmt is TriggerStatementBody trg)
            {
                if (trg.Options.HasOption(TriggerOptionKind.NativeCompile))
                {
                    return;
                }
            }
            else if (firstStmt is FunctionStatementBody fn)
            {
                if (fn.Options.HasOption(FunctionOptionKind.NativeCompilation))
                {
                    return;
                }
            }

            // If not natively compiled then lets detect violations
            batch.AcceptChildren(this);
        }

        private static bool IsSimpleSource(FromClause from)
        {
            if (from?.TableReferences is null
            || from.TableReferences.Count == 0)
            {
                // Select without source
                return true;
            }

            if (from.TableReferences.Count > 1)
            {
                return false;
            }

            var src = from.TableReferences[0];

            if (src is VariableTableReference)
            {
                // Select from the same or another table variable
                return true;
            }

            if (src is OpenJsonTableReference)
            {
                // OPENJSON is kinda scalar thing
                return true;
            }

            if (src is OpenXmlTableReference)
            {
                // OPENXML is kinda scalar thing
                return true;
            }

            if (src is GlobalFunctionTableReference)
            {
                // e.g. STRING_SPLIT
                return true;
            }

            if (src is NamedTableReference tbl && tbl.SchemaObject.SchemaIdentifier is null)
            {
                // Moving data from a temp table, INSERTED and DELETED system tables
                // is unlikely to be parallelized in SELECT part
                string tblName = tbl.SchemaObject.BaseIdentifier.Value;

                if (tblName.StartsWith(TSqlDomainAttributes.TempTablePrefix)
                || TSqlDomainAttributes.IsTriggerSystemTable(tblName))
                {
                    return true;
                }
            }

            return false;
        }

        private void DetectTableVariableModification(TableReference target)
        {
            if (target is VariableTableReference varRef)
            {
                HandleNodeError(varRef, varRef.Variable.Name);
            }
        }
    }
}
