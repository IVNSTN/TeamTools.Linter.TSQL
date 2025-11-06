using TeamTools.Common.Linting.Attributes;

namespace TeamTools.TSQL.Linter
{
    public sealed class InMemoryRuleAttribute : RuleGroupAttribute
    {
        public InMemoryRuleAttribute() : base("InMemory")
        { }
    }
}
