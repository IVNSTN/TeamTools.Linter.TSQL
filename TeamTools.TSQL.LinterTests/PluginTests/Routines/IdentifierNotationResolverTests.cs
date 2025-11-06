using NUnit.Framework;
using System.Collections.Generic;
using TeamTools.TSQL.Linter.Interfaces;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.LinterTests
{
    [Category("Linter.TSQL.Routines")]
    [TestOf(typeof(IdentifierNotationResolver))]
    public sealed class IdentifierNotationResolverTests
    {
        [Test]
        public void TestIdentifierNotationResolverResolvesNotations()
        {
            Dictionary<string, NamingNotationKind> expected = new Dictionary<string, NamingNotationKind>
            {
                { "PascalCaseExample", NamingNotationKind.PascalCase },
                { "camelCaseExample", NamingNotationKind.CamelCase },
                { "snake_case_example", NamingNotationKind.SnakeLowerCase },
                { "UPPER_SNAKE_EXAMPLE", NamingNotationKind.SnakeUpperCase },
                { "kebab-case-example", NamingNotationKind.KebabCase },
                { "bad-CaseIdent_example", NamingNotationKind.Unknown },
            };

            var resolver = new IdentifierNotationResolver();

            foreach (var example in expected)
            {
                Assert.That(resolver.Resolve(example.Key), Is.EqualTo(example.Value), example.Key);
            }
        }
    }
}
