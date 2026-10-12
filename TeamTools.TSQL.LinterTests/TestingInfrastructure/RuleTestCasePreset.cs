using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace TeamTools.TSQL.LinterTests
{
    public sealed class RuleTestCasePreset : TestCaseData
    {
        private static readonly string RootTestCategoryPrefix = TestCaseAttributes.RootTestCategoryPrefix;

        // The project source folder location provided via MSBuild attrubite in the csproj file
        private static readonly string ProjectPath = Assembly.GetExecutingAssembly()
                .GetCustomAttributes<AssemblyMetadataAttribute>()
                .FirstOrDefault(a => a.Key.Equals("ProjectPath", StringComparison.OrdinalIgnoreCase))
                .Value
                .TrimEnd(Path.DirectorySeparatorChar);

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

            if (!testSourceFile.EndsWith(string.Format("raise_{0}_violations.sql", expectedViolations)))
            {
                throw new ArgumentException("Filename suffix mismatches given expectedViolations value: " + testSourceFile, nameof(testSourceFile));
            }

            // Name the test case in test explorer as the .SQL test case file is named
            SetName(string.Concat("{c}.", Path.GetFileNameWithoutExtension(testSourceFile).Replace(".", "_")));

            // Replacing link from the build output location to the source file location
            // so if user double clicks in Test Explorer on a test case the original .SQL file is opened
            // not the copy created in bin/debug/whatever folder with the build process.
            string testSourceOriginPath = BinOutputPathToSourcePath(testSourceFile);

            Properties.Add("_CodeFilePath", testSourceOriginPath);
            Properties.Add("_LineNumber", 1);

            // Attaching categories defined on a test class to each and every test case
            if (additionalCategories != null)
            {
                foreach (var cat in additionalCategories)
                {
                    SetCategory(RootTestCategoryPrefix + cat);
                }
            }
        }

        private static string BinOutputPathToSourcePath(string binPath)
        {
#if NET472
            return binPath.Replace(TestContext.CurrentContext.TestDirectory, ProjectPath);
#else
            return binPath.Replace(TestContext.CurrentContext.TestDirectory, ProjectPath, StringComparison.OrdinalIgnoreCase);
#endif
        }
    }
}
