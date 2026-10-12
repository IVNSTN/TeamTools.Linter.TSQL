using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0471", "TABLE_DEFINITION_FORMAT")]
    internal sealed partial class TableDefinitionFormatAsTableRule : BaseTableFormatControlRule
    {
        private readonly ColumnsFormattingValidator colValidator;
        private readonly TableConstraintsFormattingValidator cstrValidator;
        private readonly TableIndexesFormattingValidator idxValidator;

        public TableDefinitionFormatAsTableRule() : this(true)
        {
        }

        // for testing purpose thus comma position can be explicitly provided
        public TableDefinitionFormatAsTableRule(bool leadingComma) : base(leadingComma)
        {
            colValidator = new ColumnsFormattingValidator(ViolationHandlerWithMessage);
            cstrValidator = new TableConstraintsFormattingValidator(ViolationHandlerWithMessage);
            idxValidator = new TableIndexesFormattingValidator(ViolationHandlerWithMessage);
        }

        public override void Visit(CreateTableStatement node) => DoValidate(node.Definition);

        public override void Visit(CreateTypeTableStatement node) => DoValidate(node.Definition);

        public override void Visit(DeclareTableVariableStatement node) => DoValidate(node.Body.Definition);

        private void DoValidate(TableDefinition node)
        {
            if (node is null)
            {
                // filetable
                return;
            }

            var canonicalPos = colValidator.GetCanonicalItemPos(node, node.ColumnDefinitions, LeadingComma);

            colValidator.ValidateFormatting(node, node.ColumnDefinitions, LeadingComma);
            cstrValidator.ValidateFormatting(node, node.TableConstraints, canonicalPos.Item1);
            idxValidator.ValidateFormatting(node, node.Indexes, canonicalPos.Item1);
        }
    }
}
