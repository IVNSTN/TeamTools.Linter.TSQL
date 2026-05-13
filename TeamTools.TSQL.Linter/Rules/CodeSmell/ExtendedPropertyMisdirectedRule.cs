using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0886", "EXTENDED_PROPERTY_MISDIRECTED")]
    internal sealed partial class ExtendedPropertyMisdirectedRule : ScriptAnalysisServiceConsumingRule
    {
        public ExtendedPropertyMisdirectedRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            if (node.Batches.Count < 2)
            {
                // CREATE [TABLE] only
                return;
            }

            var mainObject = GetService<MainScriptObjectDetector>(node);
            if (mainObject?.MainObjectFullName is null)
            {
                // no main object could be determined
                return;
            }

            string mainObjectType = GetMainObjectType(mainObject.ObjectDefinitionNode);

            if (string.IsNullOrEmpty(mainObjectType))
            {
                // unsupported object type
                return;
            }

            node.AcceptChildren(new ExtendedPropertyVisitor(mainObject.MainObjectFullName, mainObjectType, ViolationHandlerWithMessage));
        }

        // TODO : support more object types?
        private static string GetMainObjectType(TSqlFragment node)
        {
            if (node is CreateTableStatement)
            {
                return "TABLE";
            }

            if (node is CreateTypeTableStatement)
            {
                // There is also 'TABLE_TYPE' but it does not seem to work as expected
                return "TYPE";
            }

            // Keep after CreateTypeTableStatement because it is a descendant of CreateTypeStatement
            if (node is CreateTypeStatement)
            {
                return "TYPE";
            }

            return default;
        }
    }
}
