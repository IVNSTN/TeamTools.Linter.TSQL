using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0110", "OLD_SCHOOL_JOIN")]
    internal sealed class JoinOptionsRule : AbstractRule
    {
        public JoinOptionsRule() : base()
        {
        }

        public override void Visit(UpdateDeleteSpecificationBase node)
        {
            // no other tables then
            if (node.FromClause is null)
            {
                return;
            }

            if (!(node.Target is NamedTableReference nm))
            {
                return;
            }

            var aliasVisitor = new AliasVisitor();
            node.FromClause.Accept(aliasVisitor);

            if (IsAliasTarget(aliasVisitor, nm))
            {
                return;
            }

            HandleNodeError(node);
        }

        public override void Visit(SelectStatement node)
        {
            var selVisitor = new SelectVisitor();
            node.QueryExpression.Accept(selVisitor);

            if (selVisitor.IsOk)
            {
                return;
            }

            HandleNodeError(node);
        }

        private static bool IsAliasTarget(AliasVisitor src, NamedTableReference target)
        {
            string tableName = target.SchemaObject.BaseIdentifier.Value;

            // alias has been given and used as target name
            if (src.Aliases.Count > 0)
            {
                return src.Aliases.Contains(tableName);
            }

            // no aliases, hovewer only one target given and no other table mentioned in FROM
            if (src.Tables.Count == 1)
            {
                return src.Tables.Contains(tableName);
            }

            return false;
        }

        private sealed class SelectVisitor : TSqlFragmentVisitor
        {
            public bool IsOk { get; private set; } = false;

            public override void Visit(FromClause node)
            {
                if (node.TableReferences is null)
                {
                    return;
                }

                CountJoinsAndTables(node.TableReferences, out int joins, out int src);

                IsOk = (src == 1 && joins == 0) || (joins > 0 && src == 0);
            }

            public override void Visit(QuerySpecification node)
            {
                if (node.FromClause is null)
                {
                    IsOk = true;
                }
            }

            private static void CountJoinsAndTables(IList<TableReference> refs, out int joins, out int src)
            {
                joins = 0;
                src = 0;

                int n = refs.Count;
                for (int i = 0; i < n; i++)
                {
                    if (refs[i] is JoinTableReference)
                    {
                        joins++;
                    }
                    else
                    {
                        // table written in old style as `table_a, table_b`, JOIN syntax not used
                        src++;
                    }
                }
            }
        }

        private sealed class AliasVisitor : TSqlFragmentVisitor
        {
            public ICollection<string> Aliases { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            public ICollection<string> Tables { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            public override void Visit(TableReferenceWithAlias node)
            {
                if (node.Alias is null)
                {
                    return;
                }

                Aliases.Add(node.Alias.Value);
            }

            public override void Visit(NamedTableReference node)
            {
                if (node.Alias is null)
                {
                    Tables.Add(node.SchemaObject.BaseIdentifier.Value);
                }
            }
        }
    }
}
