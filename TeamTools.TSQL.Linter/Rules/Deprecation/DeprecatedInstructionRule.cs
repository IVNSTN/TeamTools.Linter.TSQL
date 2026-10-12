using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Properties;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DE0403", "DEPRECATED_INSTRUCTION")]
    internal sealed class DeprecatedInstructionRule : AbstractRule
    {
        private static readonly string InstructionLegacySyntaxViolationTemplate = Strings.ViolationDetails_DeprecatedInstructionRule_LegacySyntax;

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

        public override void Visit(RaiseErrorLegacyStatement node) => HandleNodeError(node, LegacySyntax("RAISERROR"));

        public override void Visit(BackwardsCompatibleDropIndexClause node) => HandleNodeError(node, LegacySyntax("DROP"));

        public override void Visit(BrowseForClause node) => HandleNodeError(node, "FOR BROWSE");

        public override void Visit(BuiltInFunctionTableReference node) => HandleNodeError(node, Strings.ViolationDetails_DeprecatedInstructionRule_DoubleColonCall);

        public override void Visit(SetErrorLevelStatement node) => HandleNodeError(node, "SET ERRLVL");

        // This is for "timestamp" column magic definition with no explicit name and type defined
        public override void Visit(ColumnDefinition node)
        {
            if (node.DataType is null && node.ComputedColumnExpression is null
            && node.ColumnIdentifier.QuoteType == QuoteType.NotQuoted
            && string.Equals(node.ColumnIdentifier.Value, "TIMESTAMP", StringComparison.OrdinalIgnoreCase))
            {
                HandleNodeError(node.ColumnIdentifier, "TIMESTAMP column");
            }
        }

        public override void Visit(GroupByClause node)
        {
            if (node.All)
            {
                HandleNodeError(node, "GROUP BY ALL");
            }
        }

        public override void Visit(XmlForClause node)
        {
            if (node.Options.HasOption(XmlForClauseOptions.XmlData))
            {
                HandleNodeError(node, "XMLDATA");
            }
        }

        private static string LegacySyntax(string instruction)
            => string.Format(InstructionLegacySyntaxViolationTemplate, instruction);

        private static Identifier DetectForAll(Permission perm)
        {
            for (int i = perm.Identifiers.Count - 1; i >= 0; i--)
            {
                var id = perm.Identifiers[i];
                if (id.Value.Equals("ALL", StringComparison.OrdinalIgnoreCase))
                {
                    return id;
                }
            }

            return default;
        }

        private void ValidatePermissions(IList<Permission> permissions, string details)
        {
            for (int i = permissions.Count - 1; i >= 0; i--)
            {
                var option = DetectForAll(permissions[i]);
                if (option != null)
                {
                    HandleNodeError(option, details);
                }
            }
        }
    }
}
