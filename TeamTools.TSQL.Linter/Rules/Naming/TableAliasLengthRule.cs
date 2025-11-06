using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
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
            if (node.ForClause != null && node.ForClause is XmlForClause forXml)
            {
                if (forXml.Options.Any(o => o.OptionKind == XmlForClauseOptions.Auto))
                {
                    // FOR XML AUTO uses aliases as XML node names thus changing them would affect XML structure
                    // so ignoring such queries
                    return;
                }
            }

            ValidateAliases(node);
        }

        private void ValidateAliases(TSqlFragment node)
        {
            var aliasVisitor = new BadAliasVisitor(Force, HandleNodeError);
            node.AcceptChildren(aliasVisitor);
        }

        private class BadAliasVisitor : VisitorWithCallback
        {
            private static readonly int MinLength = 1;
            private readonly bool force;
            private readonly IDictionary<string, string> exceptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            private readonly IDictionary<int, int> skippedTokens = new Dictionary<int, int>();

            public BadAliasVisitor(bool force, Action<TSqlFragment> callback) : base(callback)
            {
                this.force = force;

                exceptions.Add("deleted", "d");
                exceptions.Add("inserted", "i");
                exceptions.Add("@deleted", "d");
                exceptions.Add("@inserted", "i");
                exceptions.Add("#deleted", "d");
                exceptions.Add("#inserted", "i");
            }

            public override void Visit(QueryExpression node)
            {
                // no nesting allowed
                skippedTokens.TryAdd(node.FirstTokenIndex, node.LastTokenIndex);
            }

            public override void Visit(TableReferenceWithAlias node)
            {
                if (skippedTokens.Any(skippedRange => node.FirstTokenIndex >= skippedRange.Key && node.LastTokenIndex <= skippedRange.Value))
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
            private bool IsSpecialTableAlias(TableReferenceWithAlias node)
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
                    && exceptions.ContainsKey(tblName)
                    && exceptions[tblName].Equals(node.Alias.Value, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
