using Microsoft.SqlServer.TransactSql.ScriptDom;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TeamTools.Common.Linting.Attributes;
using TeamTools.TSQL.Linter;
using TeamTools.TSQL.Linter.Infrastructure;
using TeamTools.TSQL.Linter.Rules;

namespace TeamTools.TSQL.LinterTests
{
    [TestFixture]
    public abstract partial class BaseRuleTest
    {
        protected const string TestCaseSourceName = "TestCasePresets";
        private const string UnitTestsSubfolder = "UnitTests";
        private const string TestSourcesSubfolder = "TestSources";
        private const string RootTestCategory = "Linter.TSQL."; // TODO : this prefix is repeated multiple times across testing codes
        private const string ViolationCounterMatchGroup = "violations";
        private static readonly Regex ExpectedViolationsPattern = MyRegex();
        private MockLinter linterInstance;

        protected BaseRuleTest() : base()
        {
            RuleClass = this.GetType()
                .GetCustomAttributes(typeof(TestOfRuleAttribute), false)
                .OfType<TestOfRuleAttribute>()
                .FirstOrDefault()?
                .RuleClass;
        }

        protected Type RuleClass { get; }

        [SetUp]
        public void SetUp()
        {
            linterInstance = MockLinter.MakeLinter();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Structure", "NUnit1028:The non-test method is public", Justification = "This is an abstraction for descendants")]
        public abstract void TestRule(string scriptPath, int expectedViolationCount);

        protected static IEnumerable<object> GetTestSources(Type testClass)
        {
            var ruleClassAttr = testClass.GetCustomAttributes(typeof(TestOfRuleAttribute), false)
                .OfType<TestOfRuleAttribute>()
                .FirstOrDefault();
            var ruleClassName = ruleClassAttr?.RuleClass.Name;

            if (string.IsNullOrEmpty(ruleClassName))
            {
                yield break;
            }

            var categoryAttr = testClass.GetCustomAttributes(typeof(CategoryAttribute), false)
                .OfType<CategoryAttribute>()
                .FirstOrDefault();
            var ruleCategoryName = categoryAttr?.Name.Replace(RootTestCategory, "");

            if (string.IsNullOrEmpty(ruleCategoryName))
            {
                yield break;
            }

            var additionalCategories = ruleClassAttr.RuleClass.GetCustomAttributes(typeof(RuleGroupAttribute), false)
                .OfType<RuleGroupAttribute>()
                .Select(attr => attr.GroupName);

            string scriptPathFolder = Path.GetFullPath(Path.Combine(
                TestContext.CurrentContext.WorkDirectory,
                UnitTestsSubfolder,
                ruleCategoryName,
                ruleClassName,
                TestSourcesSubfolder));

            foreach (var file in Directory.EnumerateFiles(scriptPathFolder))
            {
                var match = ExpectedViolationsPattern.Match(Path.GetFileNameWithoutExtension(file));
                if (match is null
                || !int.TryParse(match.Groups[ViolationCounterMatchGroup].Value, out int violationCount))
                {
                    Assert.Fail(string.Format("File name does not match expected pattern: {0}", Path.GetFileName(file)));
                    continue;
                }

                yield return new RuleTestCasePreset(
                    testSourceFile: file,
                    expectedViolations: violationCount,
                    additionalCategories: additionalCategories);
            }
        }

        protected void CheckRuleViolations(string scriptPath, int expectedViolationCount)
        {
            if (RuleClass is null)
            {
                throw new InvalidOperationException($"{nameof(RuleClass)} must be defined by {nameof(TestOfRuleAttribute)} class attribute");
            }

            if (string.IsNullOrEmpty(scriptPath))
            {
                throw new ArgumentNullException(nameof(scriptPath));
            }

            if (expectedViolationCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(expectedViolationCount), "cannot be negative");
            }

            if (!File.Exists(scriptPath))
            {
                throw new FileNotFoundException("Not found test case source file", scriptPath);
            }

            Assume.That(
                IsRuleApplicableToGivenCompatibilityLevel(RuleClass, scriptPath, linterInstance.CompatibilityLevel),
                $"Rule {RuleClass.Name} is not applicable to tested compatibility level {linterInstance.CompatibilityLevel}");

            int errCnt = 0;
            var rule = (AbstractRule)Activator.CreateInstance(RuleClass);

            try
            {
                rule.ViolationCallback += (obj, evt) => errCnt++;

                DoAfterRuleInstantiated(rule);
                linterInstance.Lint(scriptPath, rule);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }

            Assert.That(errCnt, Is.EqualTo(expectedViolationCount), "Expected violation count mismatched");
        }

        protected virtual void DoAfterRuleInstantiated(AbstractRule rule)
        {
            // placeholder
        }

        private static bool IsRuleApplicableToGivenCompatibilityLevel(Type ruleClass, string testScriptPath, SqlVersion compatibilityLevel)
        {
            var ruleCompatibilityRange = ruleClass
                .GetCustomAttributes(typeof(CompatibilityLevelAttribute), false)
                .OfType<CompatibilityLevelAttribute>()
                .FirstOrDefault();

            if (ruleCompatibilityRange != null)
            {
                return (ruleCompatibilityRange.MinVersion ?? compatibilityLevel) <= compatibilityLevel
                    && (ruleCompatibilityRange.MaxVersion ?? compatibilityLevel) >= compatibilityLevel;
            }

            using var reader = new StreamReader(testScriptPath);
            string header = reader.ReadLine();
            if (string.IsNullOrEmpty(header))
            {
                return true;
            }

            if (header.StartsWith("-- compatibility level")
            && header.IndexOf(':') > 0)
            {
                string levelValue = header.Substring(header.IndexOf(':') + 1);

                if (header.StartsWith("-- compatibility level min")
                && int.TryParse(levelValue, out int compatibilityLevelMin))
                {
                    return CompatibilityConverter.ToSqlVersion(compatibilityLevelMin) <= compatibilityLevel;
                }
                else
                if (header.StartsWith("-- compatibility level max")
                && int.TryParse(levelValue, out int compatibilityLevelMax))
                {
                    return CompatibilityConverter.ToSqlVersion(compatibilityLevelMax) >= compatibilityLevel;
                }
            }

            return true;
        }

        [GeneratedRegex(@".*raise_(?<violations>[\d]+)_violations", RegexOptions.IgnoreCase | RegexOptions.Compiled, "ru-RU")]
        private static partial Regex MyRegex();
    }
}
