using TeamTools.Common.Linting.Attributes;

namespace TeamTools.TSQL.Linter
{
    public sealed class SecurityRuleAttribute : RuleGroupAttribute
    {
        public SecurityRuleAttribute() : base("Security")
        { }
    }
}
