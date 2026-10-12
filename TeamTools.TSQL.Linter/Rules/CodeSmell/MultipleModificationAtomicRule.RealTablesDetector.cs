using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    internal partial class MultipleModificationAtomicRule
    {
        internal sealed class RealTablesDetector : TSqlFragmentVisitor
        {
            private readonly string updatedAlias;

            private bool seenTableVar = false;
            private bool seenTempTable = false;
            private bool seenOtherTables = false;
            private bool seenUpdatedAlias = false;

            public RealTablesDetector(string updatedAlias) : base()
            {
                this.updatedAlias = updatedAlias;
            }

            private bool NoRealTables => (seenTableVar || seenTempTable) && !seenOtherTables;

            private bool OnlyVarTables => seenTableVar && !seenOtherTables && !seenTempTable;

            public static bool IsNoRealTableDml(DataModificationStatement node, bool tempTablesAreNotReal = true)
            {
                TSqlFragment fragmentToAnalyzeTableReferences = GetNarrowedFragmentToAnalyze(node, out string updatedAlias);

                var realTablesDetector = new RealTablesDetector(updatedAlias);
                fragmentToAnalyzeTableReferences.Accept(realTablesDetector);
                return tempTablesAreNotReal ? realTablesDetector.NoRealTables : realTablesDetector.OnlyVarTables;
            }

            // QueryDerivedTable means subquery with alias - it is not a real table, nested objects must be checked instead
            public override void Visit(TableReferenceWithAlias node)
            {
                if (seenUpdatedAlias)
                {
                    // already found whats actually modified
                    return;
                }

                if (node.Alias != null && !string.IsNullOrEmpty(updatedAlias)
                && string.Equals(updatedAlias, node.Alias.Value, StringComparison.OrdinalIgnoreCase))
                {
                    seenUpdatedAlias = true;
                }

                if (node is VariableTableReference)
                {
                    // Table variables do not participate in user-defined transactions
                    seenTableVar = true;

                    if (seenUpdatedAlias)
                    {
                        seenOtherTables = false;
                    }

                    return;
                }

                if (node is NamedTableReference nm
                && nm.SchemaObject.IsTempTable())
                {
                    // Not including temp table into transaction is a common scenario
                    seenTempTable = true;

                    if (seenUpdatedAlias)
                    {
                        seenOtherTables = false;
                    }

                    return;
                }

                seenOtherTables |= !(node is QueryDerivedTable);
            }

            private static TSqlFragment GetNarrowedFragmentToAnalyze(TSqlFragment node, out string updatedAlias)
            {
                updatedAlias = default;
                var fragmentToAnalyzeTableReferences = node;

                // In simple DML only the target table reference can be analyzed, not the whole query body
                if (node is InsertStatement ins)
                {
                    fragmentToAnalyzeTableReferences = ins.InsertSpecification.Target;
                }
                else if (node is MergeStatement merge)
                {
                    fragmentToAnalyzeTableReferences = merge.MergeSpecification.Target;
                }
                else if (node is UpdateStatement upd)
                {
                    fragmentToAnalyzeTableReferences = upd.UpdateSpecification;
                }
                else if (node is DeleteStatement del)
                {
                    fragmentToAnalyzeTableReferences = del.DeleteSpecification;
                }

                if (fragmentToAnalyzeTableReferences is UpdateDeleteSpecificationBase spec)
                {
                    if (spec.FromClause is null)
                    {
                        fragmentToAnalyzeTableReferences = spec.Target;
                    }
                    else if (spec.FromClause.TableReferences.Count == 1
                    && spec.FromClause.TableReferences[0] is TableReferenceWithAlias tbl)
                    {
                        fragmentToAnalyzeTableReferences = tbl;
                    }
                    else if (spec.Target is NamedTableReference nm)
                    {
                        if (nm.SchemaObject.SchemaIdentifier is null)
                        {
                            // update/delete target referenced via alias
                            updatedAlias = nm.SchemaObject.BaseIdentifier.Value;
                        }
                        else
                        {
                            // update/delete target is full name, not alias
                            fragmentToAnalyzeTableReferences = spec.Target;
                        }
                    }
                }

                return fragmentToAnalyzeTableReferences;
            }
        }
    }
}
