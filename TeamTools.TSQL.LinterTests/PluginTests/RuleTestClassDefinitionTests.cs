using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TeamTools.TSQL.LinterTests
{
    [Category("Linter.TSQL.PluginTests")]
    internal sealed class RuleTestClassDefinitionTests
    {
        private const string TestCaseSourceName = "TestCasePresets";
        private const string RootTestCategory = "Linter.TSQL.";
        private const string TestRuleExecutionMethodName = "TestRule";

        private IEnumerable<Type> testClasses;

        [OneTimeSetUp]
        public void SetUp()
        {
            testClasses = (
                from assm in AppDomain.CurrentDomain.GetAssemblies()
                from assmType in assm.GetTypes()
                where assmType.IsSubclassOf(typeof(BaseRuleTest))
                    && !assmType.IsAbstract
                    && !assmType.Name.StartsWith("Mock")
                select assmType)
                .ToList();
        }

        [Test]
        public void Test_RuleTestHasLinkedRule() => ForAll(CheckLinkedRule);

        [Test]
        public void Test_RuleTestHasCategory() => ForAll(CheckCategory);

        [Test]
        public void Test_RuleTestMethodsAreWellDefined() => ForAll(CheckMethods);

        private void ForAll(Action<Type> next) => testClasses.ToList().ForEach(next);

        private void CheckLinkedRule(Type testClass)
        {
            var ruleClassAttr = testClass.GetCustomAttributes(typeof(TestOfRuleAttribute), false).FirstOrDefault();
            Assert.That(ruleClassAttr, Is.Not.Null, $"{testClass.Name} has no rule class link");

            var ruleClass = (ruleClassAttr as TestOfRuleAttribute).RuleClass;
            Assert.That(ruleClass, Is.Not.Null, $"ruleClass undefined for {testClass.Name}");
        }

        private void CheckCategory(Type testClass)
        {
            var categoryAttr = testClass.GetCustomAttributes(typeof(CategoryAttribute), false).FirstOrDefault();
            Assert.That(categoryAttr, Is.Not.Null, $"{testClass.Name} has no category");

            var ruleCategory = (categoryAttr as CategoryAttribute).Name.Replace(RootTestCategory, "");
            // TODO : take replaced string part from config or static field
            Assert.That(ruleCategory.Replace("Linter.TSQL", "").Replace(".", ""), Is.Not.Empty, "Empty category");
        }

        private void CheckMethods(Type testClass)
        {
            // runner method definition
            var testCaseSource = testClass.GetMethod(TestCaseSourceName, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            Assert.That(testCaseSource, Is.Not.Null, $"Expected static member '{TestCaseSourceName}' is not implemented on derived test class {testClass.Name}");
            Assert.That(testCaseSource.ReturnType, Is.EqualTo(typeof(IEnumerable<object>)), string.Format("Return type of {0}() does not match expected IEnumerable<object>", TestCaseSourceName));

            // check if test execution method is marked with TestCaseSource attribute
            var testMethod = testClass.GetMethod(TestRuleExecutionMethodName);
            Assert.That(testMethod, Is.Not.Null, $"Expected public member '{TestRuleExecutionMethodName}' is not implemented on derived test class {testClass.Name}");
            var testMethodAttr = testMethod.GetCustomAttributes(typeof(TestCaseSourceAttribute), false).FirstOrDefault();
            Assert.That(testMethodAttr, Is.Not.Null, $"Expected TestCaseSource attribute on {TestRuleExecutionMethodName} method of {testClass.Name}");

            // check if TestCaseSource attribute is linked to the provisioning method
            Assert.That(
                (testMethodAttr as TestCaseSourceAttribute).SourceName?.Equals(TestCaseSourceName)
                ?? (testMethodAttr as TestCaseSourceAttribute).SourceType?.Name.Equals(nameof(testCaseSource))
                ?? false,
                Is.True,
                "TestCaseSource value mismatches real test case source object name");
        }
    }
}
