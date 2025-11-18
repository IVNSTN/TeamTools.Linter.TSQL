using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Properties;

namespace TeamTools.TSQL.Linter.Routines
{
    internal class LintingConfigLoader : BaseJsonConfigLoader<LintingConfig>
    {
        protected override void FillConfig(LintingConfig config, JToken json)
        {
            Debug.Assert(config != null, "config not set");
            Debug.Assert(json != null, "json not set");

            // TODO : inject as chain of responsibility?
            LoadOptions(config, json);
            LoadSpecialCommentPrefixes(config, json);
            LoadDeprecations(config, json);
            LoadWhitelist(config, json);
            ActivateRules(config, json);
        }

        protected override LintingConfig MakeConfig()
        {
            return new LintingConfig();
        }

        private static void LoadOptions(LintingConfig config, JToken json)
        {
            Debug.WriteLine("LoadOptions");
            Debug.Assert(config != null, "config not set");
            Debug.Assert(json != null, "json not set");

            var rulesConf = json.SelectTokens("..options")
                .Children()
                .OfType<JProperty>();

            foreach (var prop in rulesConf)
            {
                if (prop.Name.Equals("compatibility-level", StringComparison.OrdinalIgnoreCase))
                {
                    Debug.Assert(prop.Value != null && prop.Value is JValue, "prop.Value is not a scalar value");
                    int compat = prop.Value.Value<int>();
                    // TODO : get rid of magic numbers
                    if (compat >= 100 && compat < 200)
                    {
                        config.CompatibilityLevel = compat;
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException(string.Format(Strings.PluginMsg_UnknownCompatibilityLevel, prop.Value.Value<string>()));
                    }
                }
            }
        }

        private static void ActivateRules(LintingConfig config, JToken json)
        {
            Debug.WriteLine("ActivateRules");
            Debug.Assert(config != null, "config not set");
            Debug.Assert(json != null, "json not set");

            var rulesConf = json.SelectTokens("..rules")
                .Children()
                .OfType<JProperty>();

            foreach (var prop in rulesConf)
            {
                var severity = SeverityConverter.ConvertFromString(prop.Value.ToString());
                string ruleId = prop.Name;

                if (severity != Severity.None)
                {
                    config.Rules.Add(ruleId, ruleId);
                    config.RuleSeverity.Add(ruleId, severity);
                }
            }
        }

        private static void LoadDeprecations(LintingConfig config, JToken json)
        {
            Debug.WriteLine("InitDeprecations");
            Debug.Assert(config != null, "config not set");
            Debug.Assert(json != null, "json not set");

            var deprecatedList = json.SelectTokens("..deprecation")
                .Children()
                .OfType<JProperty>();

            foreach (var prop in deprecatedList)
            {
                if (!string.IsNullOrWhiteSpace(prop.Name))
                {
                    config.Deprecations.Add(prop.Name, prop.Value.ToString());
                }
            }
        }

        private static void LoadSpecialCommentPrefixes(LintingConfig config, JToken json)
        {
            Debug.WriteLine("SpecialCommentPrefixes");
            Debug.Assert(config != null, "config not set");
            Debug.Assert(json != null, "json not set");

            var specialCommentPrefixes = json.SelectTokens("..options.special-comment-prefixes")
                .Children()
                .OfType<JValue>();

            foreach (var prop in specialCommentPrefixes)
            {
                var prefix = prop.Value.ToString();
                if (!string.IsNullOrWhiteSpace(prefix))
                {
                    config.SpecialCommentPrefixes.Add(prefix);
                }
            }
        }

        private static void LoadWhitelist(LintingConfig config, JToken json)
        {
            Debug.WriteLine("Whitelist");
            Debug.Assert(config != null, "config not set");
            Debug.Assert(json != null, "json not set");

            var whitelist = json.SelectTokens("..whitelist")
                .Children()
                .OfType<JProperty>();

            foreach (var prop in whitelist)
            {
                string filePattern = prop.Name;
                if (string.IsNullOrWhiteSpace(filePattern))
                {
                    continue;
                }

                var whitelistMatchRule = MakeWhitelistPatternRegex(filePattern);
                var ignoredRules = new List<string>();
                config.Whitelist.Add(whitelistMatchRule, ignoredRules);

                var disabledRulePatterns = prop.Value.ToArray();
                int ruleCount = disabledRulePatterns.Length;
                for (int i = 0; i < ruleCount; i++)
                {
                    string ruleId = ((JValue)disabledRulePatterns[i]).Value.ToString();
                    if (!string.IsNullOrWhiteSpace(ruleId))
                    {
                        ignoredRules.Add(ruleId);
                    }
                }
            }
        }

        private static WhiteListElement MakeWhitelistPatternRegex(string pattern)
        {
            const char MultipleSymbolWildcard = '*';
            const char SingleSymbolWildcard = '?';

            Debug.Assert(!string.IsNullOrWhiteSpace(pattern), "pattern is empty");

            if (!pattern.Contains(MultipleSymbolWildcard) && !pattern.Contains(SingleSymbolWildcard))
            {
                return new WhiteListExactMatchElement(pattern);
            }

            var nameParts = pattern.Split('.');
            var firstNamePart = nameParts[0];
            if (nameParts.Length > 1 && !string.IsNullOrEmpty(firstNamePart)
            && !firstNamePart.Contains(MultipleSymbolWildcard) && !firstNamePart.Contains(SingleSymbolWildcard))
            {
                return new WhiteListRegexWithPrefixElement(firstNamePart, pattern);
            }

            return new WhiteListRegexElementMatch(pattern);
        }
    }
}
