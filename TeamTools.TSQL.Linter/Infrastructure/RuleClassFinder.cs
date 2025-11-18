using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Rules;

namespace TeamTools.TSQL.Linter.Infrastructure
{
    internal class RuleClassFinder : IRuleClassFinder
    {
        private static readonly string[] DefaultSupportedFileTypes = new[] { "SQL" };
        private readonly SqlVersion compatibilityLevel;
        private readonly Type baseType;

        public RuleClassFinder(SqlVersion compatibilityLevel)
        {
            this.compatibilityLevel = compatibilityLevel;
            baseType = typeof(AbstractRule);
        }

        public IEnumerable<RuleClassInfoDto> GetAvailableRuleClasses(IDictionary<string, string> enabledRules)
        {
            // rule has id
            // and no compatibility level limitations or target compatibilityLevel
            // fits those limitations
            foreach (var t in Assembly.GetExecutingAssembly().GetTypes().AsParallel())
            {
                if (t.IsAbstract || !t.IsClass || !baseType.IsAssignableFrom(t))
                {
                    continue;
                }

                RuleIdentityAttribute id = default;
                CompatibilityLevelAttribute compat = default;
                foreach (var attr in t.GetCustomAttributes(false))
                {
                    if (attr is RuleIdentityAttribute attrId)
                    {
                        id = attrId;
                    }
                    else if (attr is CompatibilityLevelAttribute attrCompat)
                    {
                        compat = attrCompat;
                    }
                }

                if (id is null || !enabledRules.ContainsKey(id.FullName))
                {
                    continue;
                }

                if ((compat?.MinVersion ?? compatibilityLevel) <= compatibilityLevel && (compat?.MaxVersion ?? compatibilityLevel) >= compatibilityLevel)
                {
                    yield return new RuleClassInfoDto
                    {
                        RuleClassType = t,
                        SupportedDataTypes = DefaultSupportedFileTypes,
                        RuleFullName = id.FullName,
                        RuleId = id.Id,
                        RuleMnemo = id.Mnemo,
                    };
                }
            }
        }
    }
}
