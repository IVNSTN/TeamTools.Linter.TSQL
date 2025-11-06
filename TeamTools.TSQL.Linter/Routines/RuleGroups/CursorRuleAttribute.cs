using TeamTools.Common.Linting.Attributes;

namespace TeamTools.TSQL.Linter.Rules
{
    public sealed class CursorRuleAttribute : RuleGroupAttribute
    {
        public CursorRuleAttribute() : base("Cursors")
        { }
    }
}
