using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    internal partial class MixOfDdlAndDmlRule
    {
        private sealed class DdlDmlMixVisitor : VisitorWithCallback
        {
            private TSqlFragment lastDDL;
            private TSqlFragment lastDML;

            public DdlDmlMixVisitor(Action<TSqlFragment> callback) : base(callback)
            { }

            // DDL
            public override void Visit(AlterTableStatement node) => DDL(node);

            public override void ExplicitVisit(CreateTableStatement node) => DDL(node);

            public override void ExplicitVisit(DropTableStatement node) => DDL(node);

            public override void ExplicitVisit(CreateIndexStatement node) => DDL(node);

            public override void ExplicitVisit(AlterIndexStatement node) => DDL(node);

            public override void ExplicitVisit(DropIndexStatement node) => DDL(node);

            public override void ExplicitVisit(CreateStatisticsStatement node) => DDL(node);

            public override void ExplicitVisit(DropStatisticsStatement node) => DDL(node);

            public override void ExplicitVisit(CreateSchemaStatement node) => DDL(node);

            public override void ExplicitVisit(DropSchemaStatement node) => DDL(node);

            // DML
            public override void Visit(DataModificationStatement node) => DML(node);

            public override void Visit(SelectStatement node) => DML(node);

            // Detection
            private static bool HasRecompileDirective(IList<OptimizerHint> hints)
            {
                for (int i = hints.Count - 1; i >= 0; i--)
                {
                    if (hints[i].HintKind == OptimizerHintKind.Recompile)
                    {
                        return true;
                    }
                }

                return false;
            }

            private static bool HasSourceQuery(StatementWithCtesAndXmlNamespaces node)
            {
                FromClause from = null;
                DataModificationSpecification dml = null;

                if (node is SelectStatement sel)
                {
                    if (sel.Into != null)
                    {
                        // SELECT INTO is both DDL and DML
                        return true;
                    }

                    from = sel.QueryExpression.GetQuerySpecification()?.FromClause;
                }
                else if (node is InsertStatement ins && ins.InsertSpecification.InsertSource is SelectInsertSource inssel)
                {
                    dml = ins.InsertSpecification;
                    from = inssel.Select.GetQuerySpecification()?.FromClause;
                }
                else if (node is UpdateStatement upd)
                {
                    dml = upd.UpdateSpecification;
                    from = upd.UpdateSpecification.FromClause;
                }
                else if (node is DeleteStatement del)
                {
                    dml = del.DeleteSpecification;
                    from = del.DeleteSpecification.FromClause;
                }
                else
                {
                    // Let's say MERGE is always a candidate for recompile
                    return true;
                }

                if (dml?.Target is NamedTableReference)
                {
                    // Modifiing temporary or persistent table
                    return true;
                }

                if ((from?.TableReferences?.Count ?? 0) == 0)
                {
                    // no FROM
                    return false;
                }

                return HasRealTableReference(from.TableReferences);
            }

            private static bool HasRealTableReference(IList<TableReference> tables)
            {
                for (int i = tables.Count - 1; i >= 0; i--)
                {
                    if (IsRealTableReference(tables[i]))
                    {
                        return true;
                    }
                }

                return false;
            }

            private static bool IsRealTableReference(TableReference tbl)
            {
                if (tbl is JoinTableReference join)
                {
                    return IsRealTableReference(join.FirstTableReference)
                        || IsRealTableReference(join.SecondTableReference);
                }

                if (tbl is JoinParenthesisTableReference jp)
                {
                    return IsRealTableReference(jp.Join);
                }

                if (tbl is VariableTableReference)
                {
                    // table variable cannot be altered and is not schema-bound
                    return false;
                }

                if (tbl is GlobalFunctionTableReference)
                {
                    // e.g. STRING_SPLIT
                    return false;
                }

                if (tbl is OpenJsonTableReference)
                {
                    // OPENJSON is kinda scalar thing
                    return false;
                }

                if (tbl is OpenXmlTableReference)
                {
                    // OPENXML is kinda scalar thing
                    return false;
                }

                return true;
            }

            private void DDL(TSqlFragment node)
            {
                lastDDL = node;
            }

            private void DML(StatementWithCtesAndXmlNamespaces node)
            {
                // If there already was a DDL and a DML statement
                // and the DDL statement was after DML and now we are doing DML
                // again right after DDL then this is the case for implicit recompilation.
                // However OPTION (RECOMPILE) means that recompilation is expected.
                if (lastDML?.StartLine < lastDDL?.StartLine
                && !HasRecompileDirective(node.OptimizerHints))
                {
                    Callback(lastDDL);
                    lastDDL = null;
                    lastDML = null;
                }
                else if (HasSourceQuery(node))
                {
                    lastDML = node;
                }
            }
        }
    }
}
