using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Infrastructure;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.LinterTests
{
    [Category("Linter.TSQL.Routines")]
    [TestOf(typeof(LintingConfigLoader))]
    public sealed class LintingConfigLoaderTests
    {
        private LintingConfigLoader loader;

        private static string RuleIdSeparator => RuleIdentityAttribute.IdSeparator;

        [SetUp]
        public void SetUp()
        {
            loader = new LintingConfigLoader();
        }

        [Test]
        public void TestConfigLoaderReturnsNullOnInvalidInput()
        {
            var config = loader.LoadConfig("unknown-file");
            Assert.That(config, Is.Null, "unknown file");

            config = loader.LoadConfig(new StringReader(@"asdf"));
            Assert.That(config, Is.Null, "bad json");

            config = loader.LoadConfig((TextReader)null);
            Assert.That(config, Is.Null, "null config 1");
            config = loader.LoadConfig((string)null);
            Assert.That(config, Is.Null, "null config 2");
        }

        [Test]
        public void TestConfigLoaderFailsOnInvalidValues()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
            () => loader.LoadConfig(new StringReader(
            @"
                {
                    ""options"": {
                        ""compatibility-level"": 500
                    }
                }")), "bad compatibility level");

            Assert.Throws<ArgumentOutOfRangeException>(
                () => loader.LoadConfig(
                new StringReader(
                @"
                {
                    ""options"": {
                        ""compatibility-level"": 3
                    }
                }")), "bad compatibility level");
        }

        [Test]
        public void TestResourceLoaderReadsAllValidElements()
        {
            var config = new LintingConfig();
            config.Rules.Add("foo", "dummy"); // messages loader does not add new rules
            var resourceLoader = new SqlLinterResourceLoader(config);
            resourceLoader.LoadConfig(new StringReader(
            @"
            {
                ""messages"":
                {
                    ""foo"": ""foo error description""
                },
                ""keywords"":
                [
                    ""RETURN""
                ],
                ""globalVariables"":
                [
                    {
                        ""Name"": ""@@ROWCOUNT"",
                        ""ResultType"" : ""INT""
                    }
                ],
                ""functions"":
                {
                    ""DATEPART"":
                    {
                        ""ResultType"": ""INT"",
                        ""ParamCount"": 2,
                        ""Params"":
                        {
                            ""0"": {
                                ""DataType"": ""DATE_TIME_PART""
                            },
                            ""1"": {
                                ""DataType"": ""DATETIME""
                            }
                        }
                    }
                },
                ""enums"":
                {
                    ""DATE_TIME_PART"":
                    {
                        ""MS"": {""Alias"": ""MILLISECOND""},
                        ""MILLISECOND"": {""Requires"": ""TIME""}
                    }
                }
            }"));

            // messages
            Assert.That(config.Rules["foo"], Is.EqualTo("foo error description"), "foo descr");

            // keywords
            Assert.That(config.SqlServerMetadata.Keywords, Does.Contain("RETURN"), "keywords parsed");

            // global vars
            Assert.That(config.SqlServerMetadata.GlobalVariables, Does.ContainKey("@@ROWCOUNT"), "globalVariables parsed");
            Assert.That(config.SqlServerMetadata.GlobalVariables["@@ROWCOUNT"], Is.EqualTo("INT"), "globalVariable type parsed");

            // enums
            Assert.That(config.SqlServerMetadata.Enums.ContainsKey(TSqlDomainAttributes.DateTimePartEnum), Is.True, "enums parsed");
            var enumInfo = config.SqlServerMetadata.Enums[TSqlDomainAttributes.DateTimePartEnum];
            Assert.That(enumInfo, Is.Not.Empty, "enums items parsed");
            Assert.That(enumInfo.Any(e => e.Name == "MS"), Is.True, "enums item name parsed");
            Assert.That(enumInfo.Where(e => e.Name == "MS").First().Properties, Does.ContainKey("Alias"), "enums item properties");
            Assert.That(enumInfo.Where(e => e.Name == "MS").First().Properties["Alias"], Is.EqualTo("MILLISECOND"), "enums item alais name parsed");

            // functions
            Assert.That(config.SqlServerMetadata.Functions, Does.ContainKey("DATEPART"), "functions parsed");
            Assert.That(config.SqlServerMetadata.Functions["DATEPART"].DataType, Is.EqualTo("INT"), "function type parsed");
            Assert.That(config.SqlServerMetadata.Functions["DATEPART"].ParamCount, Is.EqualTo(2), "function param count parsed");
            Assert.That(config.SqlServerMetadata.Functions["DATEPART"].ParamDefinition.ContainsKey("1"), Is.True, "function param definitions parsed");
            Assert.That(config.SqlServerMetadata.Functions["DATEPART"].ParamDefinition["1"], Is.EqualTo("DATETIME"), "function param types parsed");
        }

        [Test]
        public void TestResourceFileContainsMessages()
        {
            string testedRuleId = $"CS0101{RuleIdSeparator}SELECT_INTO";
            var config = new LintingConfig();
            config.Rules.Add(testedRuleId, "dummy"); // messages loader does not add new rules
            var resourceLoader = new SqlLinterResourceLoader(config);
            string resPath = GetResourceFilePath("ViolationMessages.json");
            using var res = new StreamReader(resPath);
            resourceLoader.LoadConfig(res);

            Assert.That(config.Rules[testedRuleId], Is.Not.EqualTo("dummy"));
        }

        [Test]
        public void TestResourceFileContainsKeywords()
        {
            var config = new LintingConfig();
            Assert.That(config.SqlServerMetadata.Keywords.Contains("BEGIN"), Is.False, "garbage");

            var resourceLoader = new SqlLinterResourceLoader(config);
            string resPath = GetResourceFilePath("SqlServerMetadata.json");
            using var res = new StreamReader(resPath);
            resourceLoader.LoadConfig(res);

            Assert.That(config.SqlServerMetadata.Keywords, Does.Contain("BEGIN"), "keywords not loaded");
        }

        [Test]
        public void TestResourceFileContainsGlobalVars()
        {
            var config = new LintingConfig();
            Assert.That(config.SqlServerMetadata.GlobalVariables.ContainsKey("@@ROWCOUNT"), Is.False, "garbage");

            var resourceLoader = new SqlLinterResourceLoader(config);
            string resPath = GetResourceFilePath("SqlServerMetadata.json");
            using var res = new StreamReader(resPath);
            resourceLoader.LoadConfig(res);

            Assert.That(config.SqlServerMetadata.GlobalVariables.ContainsKey("@@ROWCOUNT"), Is.True, "global vars not loaded");
        }

        [Test]
        public void TestResourceFileContainsFunctions()
        {
            var config = new LintingConfig();
            Assert.That(config.SqlServerMetadata.Functions.ContainsKey("SYSDATETIME"), Is.False, "garbage");

            var resourceLoader = new SqlLinterResourceLoader(config);
            string resPath = GetResourceFilePath("SqlServerMetadata.json");
            using var res = new StreamReader(resPath);
            resourceLoader.LoadConfig(res);

            Assert.That(config.SqlServerMetadata.Functions.ContainsKey("SYSDATETIME"), Is.True, "functions not loaded");
        }

        [Test]
        public void TestResourceFileContainsEnums()
        {
            var config = new LintingConfig();
            Assert.That(config.SqlServerMetadata.Functions.ContainsKey(TSqlDomainAttributes.DateTimePartEnum), Is.False, "garbage");

            var resourceLoader = new SqlLinterResourceLoader(config);
            string resPath = GetResourceFilePath("SqlServerMetadata.json");
            using var res = new StreamReader(resPath);
            resourceLoader.LoadConfig(res);

            Assert.That(config.SqlServerMetadata.Enums.ContainsKey(TSqlDomainAttributes.DateTimePartEnum), Is.True, "enums not loaded");
        }

        [Test]
        public void TestConfigLoaderReadsAllValidElements()
        {
            var config = loader.LoadConfig(new StringReader(
            @"
            {
                ""rules"":
                {
                    ""foo"": ""error"",
                    ""dummy"": ""off"",
                    ""bar"": ""warning""
                },
                ""options"": {
                    ""compatibility-level"": 130,
                    ""special-comment-prefixes"":
                    [
                        ""DEBUG""
                    ]
                },
                ""dummy"": ""dummy"",
                ""deprecation"":
                {
                    ""zar"": ""qar""
                },
                ""keywords"":
                [
                    ""RETURN""
                ],
                ""whitelist"":
                {
                    ""dbo.test.sql"": [""RULEID-123"", ""ANOTHERRULE-ID""],
                    ""admin.secret_sp.sql"": [""RULEID-*""]
                }
            }"));

            Assert.That(config, Is.Not.Null, "good json");
            Assert.That(config.Rules, Does.ContainKey("foo"), "has rule foo");
            Assert.That(config.Rules, Does.ContainKey("bar"), "has rule bar");
            Assert.That(config.Rules, Has.Count.EqualTo(2), "rules count");
            Assert.That(config.CompatibilityLevel, Is.EqualTo(130), "compatibility level");
            Assert.That(config.SpecialCommentPrefixes, Does.Contain("DEBUG"), "special comments parsed");
            Assert.That(config.Deprecations, Does.ContainKey("zar"), "deprecations parsed");
            Assert.That(config.Whitelist.Any(wh => wh.Key.IsMatch("dbo.test.sql")), Is.True, "whitelist parsed");
        }

        [Test]
        public void TestTryParseJsonReturnsNullOnInvalidInput()
        {
            Newtonsoft.Json.Linq.JToken token;

            Assert.That(LintingConfigLoader.TryParseJson("asdf", out token), Is.False);
            Assert.That(token, Is.Null, "bad json");

            Assert.That(LintingConfigLoader.TryParseJson("", out token), Is.False);
            Assert.That(token, Is.Null, "empty string");

            Assert.That(LintingConfigLoader.TryParseJson(null, out token), Is.False);
            Assert.That(token, Is.Null, "null");
        }

        [Test]
        public void TestTryParseJsonSucceeds()
        {
            Newtonsoft.Json.Linq.JToken token;

            Assert.That(
                LintingConfigLoader.TryParseJson(
                @"
                    {
                        ""parent"":
                        {
                            ""child1"": [1, 2, 3],
                            ""child2"": ""test""
                        },
                        ""foo"": ""bar"",
                        ""flag"": true
                   }
                ", out token),
                Is.True);

            Assert.That(token, Is.Not.Null, "good json");
        }

        private static string GetResourceFilePath(string fileName)
        {
            return Path.Join(TestContext.CurrentContext.TestDirectory, "Resources", fileName);
        }
    }
}
