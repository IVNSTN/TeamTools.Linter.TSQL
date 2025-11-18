using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("NM0207", "ALIAS_LENGTH")]
    internal sealed class TableAliasLengthRule : AbstractRule
    {
        public TableAliasLengthRule(bool force) : base()
        {
            Force = force;
        }

        public TableAliasLengthRule() : this(false)
        {
        }

        public bool Force { get; }

        public override void Visit(DataModificationStatement node) => ValidateAliases(node);

        public override void Visit(QueryExpression node)
        {
            if (node.ForClause != null && node.ForClause is XmlForClause forXml
            && forXml.Options.HasOption(XmlForClauseOptions.Auto))
            {
                // FOR XML AUTO uses aliases as XML node names thus changing them would affect XML structure
                // so ignoring such queries
                return;
            }

            ValidateAliases(node);
        }

        private void ValidateAliases(TSqlFragment node)
        {
            var aliasVisitor = new BadAliasVisitor(Force, ViolationHandler);
            node.AcceptChildren(aliasVisitor);
        }

        private sealed class BadAliasVisitor : VisitorWithCallback
        {
            private static readonly int MinLength = 1;
            private static readonly Dictionary<string, string> Exceptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "deleted", "d" },
                { "inserted", "i" },
                { "@deleted", "d" },
                { "@inserted", "i" },
                { "#deleted", "d" },
                { "#inserted", "i" },
            };

            private readonly bool force;
            private int lastVisitedTokenIndex = -1;

            public BadAliasVisitor(bool force, Action<TSqlFragment> callback) : base(callback)
            {
                this.force = force;
            }

            public override void Visit(QueryExpression node)
            {
                // no nesting allowed
                lastVisitedTokenIndex = node.LastTokenIndex;
            }

            public override void Visit(TableReferenceWithAlias node)
            {
                if (node.FirstTokenIndex <= lastVisitedTokenIndex)
                {
                    return;
                }

                if (node.Alias is null || node.Alias.Value.Length > MinLength)
                {
                    return;
                }

                if (force || !IsSpecialTableAlias(node))
                {
                    Callback(node.Alias);
                }
            }

            // Allow simple aliases for "inserted" and "deleted" tables
            private static bool IsSpecialTableAlias(TableReferenceWithAlias node)
            {
                string tblName = default;

                if ((node is NamedTableReference ins) && (ins.SchemaObject != null))
                {
                    tblName = ins.SchemaObject.GetFullName();
                }
                else if (node is VariableTableReference tvar)
                {
                    tblName = tvar.Variable.Name;
                }

                return !string.IsNullOrEmpty(tblName)
                    && Exceptions.TryGetValue(tblName, out string conventionalAlias)
                    && conventionalAlias.Equals(node.Alias.Value, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
