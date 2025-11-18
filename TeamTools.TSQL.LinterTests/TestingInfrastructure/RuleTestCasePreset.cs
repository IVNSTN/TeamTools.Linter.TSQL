using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace TeamTools.TSQL.LinterTests
{
    public sealed class RuleTestCasePreset : TestCaseData
    {
        private static readonly string RuleCategoryPrefix = "Linter.TSQL.";

        public RuleTestCasePreset(string testSourceFile, int expectedViolations, IEnumerable<string> additionalCategories = default) : base(testSourceFile, expectedViolations)
        {
            if (string.IsNullOrWhiteSpace(testSourceFile))
            {
                throw new ArgumentNullException(nameof(testSourceFile));
            }

            if (expectedViolations < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(expectedViolations), "cannot be negative");
            }

            // TODO : or get rid of metadata duplication?
            if (!testSourceFile.EndsWith(string.Format("raise_{0}_violations.sql", expectedViolations)))
            {
                throw new ArgumentException("Filename suffix mismatches given expectedViolations value: " + testSourceFile, nameof(testSourceFile));
            }

            SetName(string.Concat("{c}.", Path.GetFileNameWithoutExtension(testSourceFile).Replace(".", "_")));

            // TODO: too much magic
            Properties.Add("_CodeFilePath", testSourceFile.Replace(@"\bin\debug\net8.0\", @"\", StringComparison.OrdinalIgnoreCase));
            Properties.Add("_LineNumber", 1);

            if (additionalCategories != null)
            {
                foreach (var cat in additionalCategories)
                {
                    SetCategory(RuleCategoryPrefix + cat);
                }
            }
        }
    }
}
