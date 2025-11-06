using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TeamTools.Common.Linting;
using TeamTools.Common.Linting.Infrastructure;
using TeamTools.TSQL.Linter.Routines;
using TeamTools.TSQL.Linter.Rules;

namespace TeamTools.TSQL.Linter.Infrastructure
{
    internal class RuleClassFinder : IRuleClassFinder
    {
        private static readonly List<string> DefaultSupportedFileTypes = new List<string> { "SQL" };
        private readonly SqlVersion compatibilityLevel;

        public RuleClassFinder(SqlVersion compatibilityLevel)
        {
            this.compatibilityLevel = compatibilityLevel;
        }

        public IEnumerable<RuleClassInfoDto> GetAvailableRuleClasses(IEnumerable<string> enabledRuleIds) => RuleMapper.MapEnabledIdsToRules(GetRuleTypes(), enabledRuleIds);

        protected virtual IEnumerable<RuleClassInfoDto> GetRuleTypes()
        {
            // rule has id
            // and no compatibility level limitations or target compatibilityLevel
            // fits those limitations
            var types =
                (from t in Assembly.GetExecutingAssembly().GetTypes()
                 where !t.IsAbstract && typeof(AbstractRule).IsAssignableFrom(t)
                 where t.GetCustomAttributes(typeof(RuleIdentityAttribute), true) != null
                    && t.GetCustomAttributes(typeof(RuleIdentityAttribute), true).Length > 0
                 where (t.GetCustomAttributes(typeof(CompatibilityLevelAttribute), true)?.Length ?? 0) == 0
                    || t.GetAttributeValue((CompatibilityLevelAttribute attr) => (attr.MinVersion ?? compatibilityLevel) <= compatibilityLevel && (attr.MaxVersion ?? compatibilityLevel) >= compatibilityLevel)
                 select new RuleClassInfoDto
                 {
                     RuleClassType = t,
                     RuleFullName = t.GetAttributeValue((RuleIdentityAttribute attr) => attr.FullName),
                     SupportedDataTypes = DefaultSupportedFileTypes,
                     RuleId = t.GetAttributeValue((RuleIdentityAttribute attr) => attr.Id),
                     RuleMnemo = t.GetAttributeValue((RuleIdentityAttribute attr) => attr.Mnemo),
                 }).ToList();

            return types;
        }
    }
}
