using Microsoft.SqlServer.TransactSql.ScriptDom;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using TeamTools.TSQL.Linter.Rules;

namespace TeamTools.TSQL.LinterTests
{
    [Category("Linter.TSQL.Formatting")]
    [TestOfRule(typeof(TableDefinitionFormatAsTableRule))]
    public sealed class TableDefinitionFormatAsTableRuleTests : BaseRuleTest
    {
        [TestCaseSource(nameof(TestCasePresets))]
        public override void TestRule(string scriptPath, int expectedViolationCount)
        {
            CheckRuleViolations(scriptPath, expectedViolationCount);
        }

        [TestCaseSource(nameof(TestCaseAlternativePresets))]
        public void TestWithTrailingComma(string scriptPath, int expectedViolationCount)
        {
            CheckRuleViolations(scriptPath, expectedViolationCount, new TableDefinitionFormatAsTableRule(false));
        }

        private static IEnumerable<object> TestCasePresets()
        {
            return GetTestSources(typeof(TableDefinitionFormatAsTableRuleTests));
        }

        private static IEnumerable<object> TestCaseAlternativePresets()
        {
            return GetTestSources(typeof(TableDefinitionFormatAsTableRuleTests), "TestSourcesAlt");
        }
    }
}
