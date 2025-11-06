using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using TeamTools.Common.Linting;

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

            var rulesConf = json.SelectTokens("..options").ToList();

            foreach (var ruleConf in rulesConf.Children())
            {
                var prop = (JProperty)ruleConf;

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
                        throw new ArgumentOutOfRangeException(string.Format("Unknown compatibility level: {0}", prop.Value.Value<string>()));
                    }
                }
            }
        }

        private static void ActivateRules(LintingConfig config, JToken json)
        {
            Debug.WriteLine("ActivateRules");
            Debug.Assert(config != null, "config not set");
            Debug.Assert(json != null, "json not set");

            var rulesConf = json.SelectTokens("..rules").ToList();

            foreach (var configValue in rulesConf.Children())
            {
                var prop = (JProperty)configValue;
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

            var deprecatedList = json.SelectTokens("..deprecation").ToList();

            foreach (var configValue in deprecatedList.Children())
            {
                var prop = (JProperty)configValue;

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

            var specialCommentPrefixes = json.SelectTokens("..options.special-comment-prefixes").ToList();

            foreach (var configValue in specialCommentPrefixes.Children())
            {
                var prop = (JValue)configValue;

                if (!string.IsNullOrWhiteSpace(prop.Value.ToString()))
                {
                    config.SpecialCommentPrefixes.Add(prop.Value.ToString().Trim());
                }
            }
        }

        private static void LoadWhitelist(LintingConfig config, JToken json)
        {
            Debug.WriteLine("Whitelist");
            Debug.Assert(config != null, "config not set");
            Debug.Assert(json != null, "json not set");

            var whitelist = json.SelectTokens("..whitelist").ToList();

            foreach (var configValue in whitelist.Children())
            {
                var prop = (JProperty)configValue;
                string filePattern = prop.Name;
                if (string.IsNullOrWhiteSpace(filePattern))
                {
                    continue;
                }

                var fileRegex = MakeWhitelistPatternRegex(filePattern);
                config.Whitelist.Add(fileRegex, new List<string>());

                var disabledRulePatterns = prop.Value.ToList();
                foreach (var rule in disabledRulePatterns)
                {
                    string rulePattern = ((JValue)rule).Value.ToString();
                    if (!string.IsNullOrWhiteSpace(rulePattern))
                    {
                        config.Whitelist[fileRegex].Add(rulePattern);
                    }
                }
            }
        }

        private static Regex MakeWhitelistPatternRegex(string pattern)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(pattern), "pattern is empty");

            var sanitizedPattern = Regex.Escape(pattern.Replace("*", "#many#").Replace("?", "#single#"));
            sanitizedPattern = sanitizedPattern.Replace("\\#single\\#", "[.]").Replace("\\#many\\#", "(.*)");
            return new Regex(sanitizedPattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }
    }
}
