using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Linq;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0524", "DATA_ENCRYPTED")]
    [CompatibilityLevel(SqlVersion.Sql130)]
    internal sealed class DataEncryptedRule : AbstractRule
    {
        public DataEncryptedRule() : base()
        {
        }

        public override void Visit(ColumnDefinition node) => HandleNodeErrorIfAny(node.Encryption);

        public override void Visit(BeginDialogStatement node)
        {
            var enc = node.Options
                .Where(opt => opt.OptionKind == DialogOptionKind.Encryption)
                .OfType<OnOffDialogOption>()
                .FirstOrDefault(opt => opt.OptionState == OptionState.On);

            HandleNodeErrorIfAny(enc);
        }
    }
}
