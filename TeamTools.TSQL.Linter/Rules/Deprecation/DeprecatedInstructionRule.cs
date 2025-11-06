using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DE0403", "DEPRECATED_INSTRUCTION")]
    internal sealed class DeprecatedInstructionRule : AbstractRule
    {
        public DeprecatedInstructionRule() : base()
        {
        }

        public override void Visit(GrantStatement node) => ValidatePermissions(node.Permissions, "GRANT ALL");

        public override void Visit(RevokeStatement node) => ValidatePermissions(node.Permissions, "REVOKE ALL");

        public override void Visit(DenyStatement node) => ValidatePermissions(node.Permissions, "DENY ALL");

        public override void Visit(ComputeClause node) => HandleNodeError(node, "COMPUTE BY");

        public override void Visit(WriteTextStatement node) => HandleNodeError(node, "WRITETEXT");

        public override void Visit(SetTextSizeStatement node) => HandleNodeError(node, "TEXTSIZE");

        public override void Visit(UpdateTextStatement node) => HandleNodeError(node, "UPDATETEXT");

        public override void Visit(ReadTextStatement node) => HandleNodeError(node, "READTEXT");

        public override void Visit(SetUserStatement node) => HandleNodeError(node, "SETUSER");

        public override void Visit(SetRowCountStatement node) => HandleNodeError(node);

        public override void Visit(SetOffsetsStatement node) => HandleNodeError(node);

        public override void Visit(RaiseErrorLegacyStatement node) => HandleNodeError(node, "RAISERROR legacy syntax");

        public override void Visit(SecurityStatementBody80 node) => HandleNodeError(node, "SECURITY legacy syntax");

        public override void Visit(SecurityUserClause80 node) => HandleNodeError(node, "SECURITY legacy syntax");

        public override void Visit(BackwardsCompatibleDropIndexClause node) => HandleNodeError(node, "DROP INDEX legacy syntax");

        public override void Visit(BrowseForClause node) => HandleNodeError(node, "FOR BROWSE");

        public override void Visit(BuiltInFunctionTableReference node) => HandleNodeError(node, ":: system function call");

        public override void Visit(GroupByClause node)
        {
            if (node.All)
            {
                HandleNodeError(node, "GROUP BY ALL");
            }
        }

        public override void Visit(XmlForClause node)
        {
            if (node.Options.Any(opt => opt.OptionKind == XmlForClauseOptions.XmlData))
            {
                HandleNodeError(node, "XMLDATA");
            }
        }

        private void ValidatePermissions(IList<Permission> permissions, string details)
        {
            var grantAll = permissions
                .Where(perm => perm.Identifiers.Any(id => id.Value.Equals("ALL", StringComparison.OrdinalIgnoreCase)))
                .ToList();

            foreach (var perm in grantAll)
            {
                HandleNodeError(perm, details);
            }
        }
    }
}
