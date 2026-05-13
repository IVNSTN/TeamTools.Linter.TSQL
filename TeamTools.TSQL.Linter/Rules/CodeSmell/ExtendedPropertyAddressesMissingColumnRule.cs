using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0887", "EXTENDED_PROPERTY_FOR_MISSING_COL")]
    internal sealed partial class ExtendedPropertyAddressesMissingColumnRule : ScriptAnalysisServiceConsumingRule
    {
        public ExtendedPropertyAddressesMissingColumnRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            if (node.Batches.Count < 2)
            {
                // 2 batches needed to create table and add extended property
                return;
            }

            var mainObject = GetService<MainScriptObjectDetector>(node);
            if (string.IsNullOrWhiteSpace(mainObject?.ObjectFullName)
            || !(mainObject.ObjectDefinitionNode is CreateTableStatement))
            {
                // Not a CREATE TABLE script
                return;
            }

            var tableElements = GetService<TableDefinitionElementsEnumerator>(node);

            if (!tableElements.Tables.TryGetValue(mainObject.ObjectFullName, out var tblDef))
            {
                // Something went wrong
                return;
            }

            node.AcceptChildren(new ExtendedPropertyVisitor(mainObject.MainObjectFullName, tblDef.Columns, ViolationHandlerWithMessage));
        }
    }
}
