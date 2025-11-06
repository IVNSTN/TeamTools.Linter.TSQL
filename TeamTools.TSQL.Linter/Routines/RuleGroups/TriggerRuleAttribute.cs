using TeamTools.Common.Linting.Attributes;

namespace TeamTools.TSQL.Linter
{
    public sealed class TriggerRuleAttribute : RuleGroupAttribute
    {
        public TriggerRuleAttribute() : base("Triggers")
        { }
    }
}
