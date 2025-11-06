using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Interfaces;

namespace TeamTools.TSQL.Linter.Infrastructure
{
    internal sealed class RuleCollectionHandler : BaseRuleCollectionHandler<ISqlRule, TSqlFragment>
    {
        public RuleCollectionHandler(IReporter reporter, IRuleFactory<ISqlRule> rulesFactory, IRuleClassFinder ruleClassFinder, IFileParser<TSqlFragment> parser, BaseLintingConfig config) : base(reporter, rulesFactory, ruleClassFinder, parser, config)
        {
        }
    }
}
