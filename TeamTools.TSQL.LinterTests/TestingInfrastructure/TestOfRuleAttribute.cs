using NUnit.Framework;
using System;
using TeamTools.TSQL.Linter.Rules;

namespace TeamTools.TSQL.LinterTests
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class TestOfRuleAttribute : TestOfAttribute
    {
        public TestOfRuleAttribute(Type ruleClass) : base(ruleClass)
        {
            if (ruleClass == null)
            {
                throw new ArgumentNullException(nameof(ruleClass));
            }

            if (ruleClass.IsAssignableFrom(typeof(AbstractRule)))
            {
                throw new ArgumentOutOfRangeException(nameof(ruleClass), "must be subclass of " + nameof(AbstractRule));
            }

            RuleClass = ruleClass;
        }

        public Type RuleClass { get; private set; }
    }
}
