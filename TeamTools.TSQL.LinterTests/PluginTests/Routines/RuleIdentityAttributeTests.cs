using NUnit.Framework;
using System;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.LinterTests
{
    [Category("Linter.TSQL.Routines")]
    [TestOf(typeof(RuleIdentityAttribute))]
    public sealed class RuleIdentityAttributeTests
    {
        [Test]
        public void TestRuleIdentityAcceptsString()
        {
            Assert.DoesNotThrow(() => new RuleIdentityAttribute("01", "test"));
        }

        [Test]
        public void TestRuleIdentityFailsOnEmptyArgument()
        {
            Assert.Throws(typeof(ArgumentNullException), () => new RuleIdentityAttribute("01", ""), "empty mnemo");
            Assert.Throws(typeof(ArgumentNullException), () => new RuleIdentityAttribute("", "asdf"), "empty id");
        }
    }
}
