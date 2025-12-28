using Microsoft.SqlServer.TransactSql.ScriptDom;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter;
using TeamTools.TSQL.Linter.Infrastructure;
using TeamTools.TSQL.Linter.Routines;
using TeamTools.TSQL.Linter.Rules;
using static TeamTools.TSQL.LinterTests.MockPluginInfrastructure;

namespace TeamTools.TSQL.LinterTests
{
    [Category("Linter.TSQL.PluginTests")]
    internal sealed class PluginTests
    {
        private static string RuleIdSeparator => RuleIdentityAttribute.IdSeparator;

#if Windows
    private const string BaseSrcPath = @"c:\src\";
#elif Linux
    private const string BaseSrcPath = @"/usr/local/src/";
#else
    private const string BaseSrcPath = @"~/src/";
#endif

        [Test]
        public void TestPluginRunDeliversRuleViolations()
        {
            var reporter = new MockReporter();
            var context = new StubContext(
                "dummy.sql",
                @"
                    select 1
                    goto b
                    b: exec my_proc
                ",
                reporter);

            // TODO : take default compatibility level from outside
            var ruleFinder = new RuleClassFinder(SqlVersion.Sql150);

            var plugin = new MockPlugin(
                Array.Empty<string>(),
                reporter,
                ruleFinder);
            plugin.PerformAction(context);
            Assert.That(reporter.Errors, Is.Empty, "no rules - no errors");

            plugin = new MockPlugin(
                new string[]
                {
                    $"CV0201{RuleIdSeparator}KEYWORD_UPPER",
                    $"CS0105{RuleIdSeparator}GOTO",
                    "DUMMY-RULE",
                },
                reporter,
                ruleFinder);
            plugin.PerformAction(context);

            Assert.That(reporter.Errors, Has.Count.EqualTo(5));
            Assert.That(reporter.Errors.Count(e => e.Equals("CV0201")), Is.EqualTo(3), "rule 1");
            Assert.That(reporter.Errors.Count(e => e.Equals("CS0105")), Is.EqualTo(2), "rule 2");
        }

        [Test]
        public void TestPluginRunDeliversParsingErrors()
        {
            var reporter = new MockReporter();
            var plugin = new MockPlugin(
                new string[]
                {
                    $"CV0201{RuleIdSeparator}KEYWORD_UPPER",
                    "DUMMY-RULE",
                },
                reporter,
                new MockRuleClassFinder(typeof(KeywordUppercaseRule)));

            var context = new StubContext(
            "dummy.sql",
            @"
                select from where
            ",
            reporter);
            plugin.PerformAction(context);
            Assert.That(reporter.Errors, Has.Count.EqualTo(1), "failed parsing");
        }

        [Test]
        public void TestRuleViolationDetails()
        {
            var reporter = new MockReporter();
            var context = new StubContext(
            "dummy-file-name.sql",
            @"
                select 1
            ",
            reporter);

            string testedRuleId = $"CV0201{RuleIdSeparator}KEYWORD_UPPER";
            var plugin = new MockPlugin(
                new string[] { testedRuleId },
                reporter,
                new MockRuleClassFinder(typeof(KeywordUppercaseRule)));
            plugin.PerformAction(context);

            Assert.That(reporter.Violations, Has.Count.EqualTo(1), "violation count");
            Assert.That(reporter.Violations[0].Column, Is.EqualTo(17), "column");
            Assert.That(reporter.Violations[0].Line, Is.EqualTo(2), "line");
            Assert.That(reporter.Violations[0].RuleId, Is.EqualTo(testedRuleId));
            Assert.That(reporter.Violations[0].Text, Is.EqualTo(testedRuleId));
        }

        [Test]
        public void TestLoadConfigFailsIfFileNotFound()
        {
            Assert.Throws<FileLoadException>(() => new TransactSqlLinter().LoadConfig("asdf"), "fails to load config from output folder");
        }

        [Test]
        public void TestErrorInRuleCheck()
        {
            var reporter = new MockReporter();
            var context = new StubContext(
            "dummy-file-name.sql",
            @"
                select 1
            ",
            reporter);

            var plugin = new MockPlugin(
                new string[] { "FAILING_RULE" },
                reporter,
                new MockRuleClassFinder(typeof(MockFailingRule)));
            plugin.PerformAction(context);

            Assert.That(reporter.Violations, Has.Count.EqualTo(1));
            Assert.That(reporter.Violations[0].Line, Is.EqualTo(0));
            Assert.That(reporter.Violations[0].Text, Is.EqualTo("Failed checking rule FAILING_RULE: i always fail"));
        }

        [Test]
        public void TestAllRulesHaveIdentity()
        {
            var rules = ListAllRuleClasses();

            Assert.That(rules, Is.Not.Empty, "no rule classes found");

            string rulesWithNoId = string.Join(
                Environment.NewLine,
                rules
                .Where(r => r.GetCustomAttributes(typeof(RuleIdentityAttribute), true)?.Length == 0)
                .Select(r => r.Name)
                .ToList());

            Assert.That(rulesWithNoId, Is.EqualTo(""), rulesWithNoId);
        }

        [Test]
        public void TestAllRuleIdsMentionedInDefaultConfig()
        {
            var ruleIds = (
                from assmType in ListAllRuleClasses()
                where assmType.GetCustomAttributes(typeof(RuleIdentityAttribute), true)?.Length > 0
                select (assmType.GetCustomAttributes(typeof(RuleIdentityAttribute), true).FirstOrDefault() as RuleIdentityAttribute).FullName)
                .ToList();

            Assert.That(ruleIds, Is.Not.Empty, "no rule classes found");

            var loader = new MockConfigIdentityLoader();
            string defaultConfPath = Path.Join(TestContext.CurrentContext.TestDirectory, "DefaultConfig.json");
            Assert.That(File.Exists(defaultConfPath), Is.True, "Config not found: " + defaultConfPath);
            var config = loader.LoadConfig(defaultConfPath);

            Assert.That(config, Is.Not.Null, "config null");
            Assert.That(config.Rules, Is.Not.Empty, "rules from conf");

            var rulesNotInConfig = ruleIds.Except(config.Rules.Keys).ToList();

            Assert.That(rulesNotInConfig, Is.Empty, string.Join(",", rulesNotInConfig));
        }

        [Test]
        public void TestAllRulesHaveMessages()
        {
            var ruleIds = (
                from assmType in ListAllRuleClasses()
                where assmType.GetCustomAttributes(typeof(RuleIdentityAttribute), true)?.Length > 0
                select (assmType.GetCustomAttributes(typeof(RuleIdentityAttribute), true).FirstOrDefault() as RuleIdentityAttribute).FullName)
                .ToList();

            Assert.That(ruleIds, Is.Not.Empty, "no rule classes found");

            var loader = new MockResourceMsgLoader();
            string messagesResPath = GetResourceFilePath("ViolationMessages.json");
            Assert.That(File.Exists(messagesResPath), Is.True, "Resource not found: " + messagesResPath);
            var config = loader.LoadConfig(messagesResPath);

            Assert.That(config, Is.Not.Null, "config null");
            Assert.That(config.Rules, Is.Not.Empty, "rules from conf");

            var rulesWithoutMsg = ruleIds.Except(config.Rules.Keys).ToList();

            Assert.That(rulesWithoutMsg, Is.Empty, string.Join(",", rulesWithoutMsg));
        }

        [Test]
        public void TestRuleFactoryLoadsExtraData()
        {
            var reporter = new MockReporter();
            var loader = new LintingConfigLoader();

            var config = loader.LoadConfig(new StringReader(@"
            {
                ""rules"":
                {
                    ""TEST-123"": ""error""
                },
                ""whitelist"":
                {
                    ""dbo.test.sql"": [""FAILING-*""],
                    ""tsqlt.test*.sql"": [""FAILING-*"", ""APPLIED-RULE""]
                }
            }"));
            var factory = new RuleFactory(config, TSqlParserFactory.Make(130), default);
            var ruleInstance = factory.MakeRule(typeof(MockRuleWithInterfaces), default);

            Assert.That(ruleInstance, Is.Not.Null, "rule undef");
            Assert.That(ruleInstance is MockRuleWithInterfaces, Is.True, "rule class");

            var rule = (MockRuleWithInterfaces)ruleInstance;

            Assert.That(rule.LoadDeprecationsCallCount, Is.EqualTo(1), "LoadDeprecationsCallCount");
            Assert.That(rule.LoadKeywordsCallCount, Is.EqualTo(1), "LoadKeywordsCallCount");
            Assert.That(rule.LoadSpecialCommentPrefixesCallCount, Is.EqualTo(1), "LoadSpecialCommentPrefixesCallCount");
            Assert.That(rule.SetParserCallCount, Is.EqualTo(1), "SetParserCallCount");
            Assert.That(rule.LoadMetadataCallCount, Is.EqualTo(1), "LoadMetadataCallCount");
        }

        [Test]
        public void TestPluginSupportsMinimalFileTypeList()
        {
            var plugin = new MockPlugin(
                new string[] { "FAILING-RULE", "APPLIED-RULE" },
                new MockReporter(),
                new MockRuleClassFinder(typeof(MockFailingRule)));

            var files = plugin.Config.SupportedFileTypes.ToList();

            Assert.That(files, Has.Count.EqualTo(2));
            Assert.That(files, Does.Contain(".sql"));
            Assert.That(files, Does.Contain(".tsql"));
        }

        [Test]
        public void TestWhitelistBypassesRuleCheck()
        {
            var config = new LintingConfigLoader().LoadConfig(new StringReader(@"
            {
                ""rules"":
                {
                    ""FAILING-RULE"": ""error"",
                    ""APPLIED-RULE"": ""off""
                },
                ""whitelist"":
                {
                    ""dbo.test.sql"": [""FAILING-RULE""],
                    ""tsqlt.test*.sql"": [""FAILING-RULE"", ""APPLIED-RULE""]
                }
            }"));

            var reporter = new MockReporter();
            var context = new StubContext(BaseSrcPath + "dbo.test.sql", "select 1", reporter);
            var contextForFilePattern = new StubContext(BaseSrcPath + "tsqlt.test_my_test.sql", "select 1", reporter);

            var plugin = new MockPlugin(
                config,
                reporter,
                new MockRuleClassFinder(typeof(MockFailingRule)));

            // FAILING-RULE is ignored for both files
            plugin.PerformAction(context);
            Assert.That(reporter.Violations, Is.Empty, "dbo.test.sql");

            plugin.PerformAction(contextForFilePattern);
            Assert.That(reporter.Violations, Is.Empty, "tsqlt.test_my_test.sql");
        }

        [Test]
        public void TestRuleNotInWhitelistStillApplied()
        {
            var config = new LintingConfigLoader().LoadConfig(new StringReader(@"
            {
                ""rules"":
                {
                    ""FAILING-RULE"": ""error"",
                    ""APPLIED-RULE"": ""error""
                },
                ""whitelist"":
                {
                    ""dbo.test.sql"": [""FAILING-RULE""],
                    ""tsqlt.test*.sql"": [""FAILING-RULE"", ""APPLIED-RULE""]
                }
            }"));

            var reporter = new MockReporter();
            var context = new StubContext(BaseSrcPath + "dbo.test.sql", "select 1", reporter);

            var plugin = new MockPlugin(
                config,
                reporter,
                new MockRuleClassFinder(typeof(MockFailingRule)));

            plugin.PerformAction(context);
            Assert.That(reporter.Violations, Has.Count.EqualTo(1));

            // for "tsqlt" file pattern this rule is still ignored
            var contextForFilePattern = new StubContext(BaseSrcPath + "tsqlt.test_my_test.sql", "select 1", reporter);
            reporter.Violations.Clear();
            plugin.PerformAction(contextForFilePattern);
            Assert.That(reporter.Violations, Is.Empty);
        }

        [Test]
        public void TestEachRuleHasUniqueIdCode()
        {
            var ids = ListAllRuleClasses()
                .SelectMany(t => t.GetCustomAttributes(typeof(RuleIdentityAttribute), false).Cast<RuleIdentityAttribute>())
                .Select(id => id.Id);

            // TODO : Assert.DoesNotThrow(() => ids.ToDictionary(id => id, id => id));
        }

        [Test]
        public void TestEachRuleHasUniqueMemoCode()
        {
            var ids = ListAllRuleClasses()
                .SelectMany(t => t.GetCustomAttributes(typeof(RuleIdentityAttribute), false).Cast<RuleIdentityAttribute>())
                .Select(id => id.Mnemo);

            // TODO : Assert.DoesNotThrow(() => ids.ToDictionary(id => id, id => id));
        }

        private static List<Type> ListAllRuleClasses()
        {
            return (
                from assm in AppDomain.CurrentDomain.GetAssemblies()
                from assmType in assm.GetTypes()
                where assmType.IsSubclassOf(typeof(AbstractRule))
                    && !assmType.IsAbstract
                    && !assmType.Name.StartsWith("Mock")
                select assmType)
                .ToList();
        }

        private static string GetResourceFilePath(string fileName)
        {
            return Path.Join(TestContext.CurrentContext.TestDirectory, "Resources", fileName);
        }
    }
}
