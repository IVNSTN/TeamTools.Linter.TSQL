using Microsoft.SqlServer.TransactSql.ScriptDom;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter;
using TeamTools.TSQL.Linter.Routines;
using TeamTools.TSQL.Linter.Rules;

namespace TeamTools.TSQL.LinterTests
{
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1649", Justification = "Reviewed")]
    internal static class MockPluginInfrastructure
    {
        private static readonly string DataType = "SQL";

        internal class MockPlugin : TransactSqlLinter
        {
            private readonly List<string> enabledRules = new List<string>();

            public MockPlugin(LintingConfig config, IReporter reporter, IRuleClassFinder ruleClassFinder) : base(config, ruleClassFinder, reporter)
            {
                if (!Config.SupportedFiles.ContainsKey(DataType))
                {
                    Config.SupportedFiles.Add(DataType, new List<string> { "." + DataType });
                }
            }

            public MockPlugin(string[] rules, IReporter reporter, IRuleClassFinder ruleClassFinder) : base(ruleClassFinder)
            {
                enabledRules.AddRange(rules);
                SetReporter(reporter);
                LoadConfig(default);

                if (!Config.SupportedFiles.ContainsKey(DataType))
                {
                    Config.SupportedFiles.Add(DataType, new List<string> { "." + DataType });
                }

                InitRulesCollection();
            }

            public override void LoadConfig(string configPath)
            {
                Config = new LintingConfig();
                foreach (string rule in enabledRules)
                {
                    Config.Rules.Add(rule, rule);
                }
            }

            public void ResetRuleCollection()
            {
                InitRulesCollection();
            }
        }

        internal class MockFailingRule : AbstractRule
        {
            public MockFailingRule() : base()
            { }

            public override void Visit(TSqlFragment node)
            {
                throw new Exception("i always fail");
            }
        }

        internal class MockRuleWithInterfaces : AbstractRule, IDeprecationHandler, IKeywordDetector, ICommentAnalyzer, IDynamicSqlParser, ISqlServerMetadataConsumer
        {
            public MockRuleWithInterfaces() : base()
            {
            }

            public int LoadDeprecationsCallCount { get; private set; } = 0;

            public int LoadKeywordsCallCount { get; private set; } = 0;

            public int LoadSpecialCommentPrefixesCallCount { get; private set; } = 0;

            public int SetParserCallCount { get; private set; } = 0;

            public int LoadMetadataCallCount { get; private set; } = 0;

            public void LoadDeprecations(IDictionary<string, string> values)
            {
                HandleFileError("LoadDeprecations");
                LoadDeprecationsCallCount++;
            }

            public void LoadKeywords(ICollection<string> values)
            {
                HandleFileError("LoadKeywords");
                LoadKeywordsCallCount++;
            }

            public void LoadSpecialCommentPrefixes(ICollection<string> values)
            {
                HandleFileError("LoadSpecialCommentPrefixes");
                LoadSpecialCommentPrefixesCallCount++;
            }

            public void SetParser(TSqlParser parser)
            {
                HandleFileError("SetParser");
                SetParserCallCount++;
            }

            public void LoadMetadata(SqlServerMetadata data)
            {
                HandleFileError("SetParser");
                LoadMetadataCallCount++;
            }
        }

        internal class StubContext : ILintingContext
        {
            public StubContext(string filePath, string contents, IReporter reporter) : base()
            {
                this.FilePath = filePath;
                this.FileContents = new StringReader(contents);
                this.Reporter = reporter;
            }

            public string FilePath { get; }

            public TextReader FileContents { get; }

            public IReporter Reporter { get; }

            public void ReportViolation(RuleViolation violation)
            {
                Reporter.ReportViolation(violation);
            }
        }

        internal class MockReporter : IReporter
        {
            public IList<string> Errors { get; } = new List<string>();

            public IList<RuleViolation> Violations { get; } = new List<RuleViolation>();

            public void Report(string error)
            {
                Errors.Add(error);
            }

            public void ReportFailure(string error)
            {
                Errors.Add(error);
            }

            public void ReportViolation(RuleViolation violation)
            {
                Errors.Add(violation.RuleId);
                Violations.Add(violation);
            }
        }

        internal class MockConfigIdentityLoader : LintingConfigLoader
        {
            protected override void FillConfig(LintingConfig config, JToken json)
            {
                var rulesConf = json.SelectTokens("..rules").ToList();

                foreach (var configValue in rulesConf.Children())
                {
                    var prop = (JProperty)configValue;
                    // loading all, even disabled rules
                    config.Rules.Add(prop.Name, prop.Name);
                }
            }
        }

        internal class MockResourceMsgLoader : LintingConfigLoader
        {
            protected override void FillConfig(LintingConfig config, JToken json)
            {
                var rulesConf = json.SelectTokens("..messages").ToList();

                foreach (var configValue in rulesConf.Children())
                {
                    var prop = (JProperty)configValue;
                    config.Rules.Add(prop.Name, prop.Value.ToString());
                }
            }
        }

        internal class MockRuleClassFinder : IRuleClassFinder
        {
            private readonly Type ruleClass;

            public MockRuleClassFinder(Type ruleClass)
            {
                this.ruleClass = ruleClass;
            }

            public IEnumerable<RuleClassInfoDto> GetAvailableRuleClasses(IDictionary<string, string> enabledRuleIds)
            {
                if (ruleClass is null)
                {
                    yield break;
                }

                // TODO : compare ruleClass.ID with enabledRuleIds keys?
                foreach (var r in enabledRuleIds)
                {
                    yield return new RuleClassInfoDto
                    {
                        RuleClassType = this.ruleClass,
                        RuleFullName = r.Key,
                        RuleId = r.Key,
                        SupportedDataTypes = new[] { DataType },
                    };
                }
            }
        }
    }
}
