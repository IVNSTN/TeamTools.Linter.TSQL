using Microsoft.SqlServer.TransactSql.ScriptDom;
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
            for (int i = 0, n = node.Options.Count; i < n; i++)
            {
                var opt = node.Options[i];
                if (opt.OptionKind == DialogOptionKind.Encryption
                && opt is OnOffDialogOption optState
                && optState.OptionState == OptionState.On)
                {
                    HandleNodeError(opt);
                }
            }
        }
    }
}
