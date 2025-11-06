using TeamTools.Common.Linting.Attributes;

namespace TeamTools.TSQL.Linter
{
    public sealed class IndexRuleAttribute : RuleGroupAttribute
    {
        public IndexRuleAttribute() : base("Indexes")
        { }
    }
}
