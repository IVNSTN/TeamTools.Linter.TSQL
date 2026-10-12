using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0884", "INVALID_EXTENDED_PROPERTY_PARAM")]
    internal sealed partial class InvalidExtendedPropertyParameterRule : AbstractRule
    {
        private readonly ExtendedPropertyValidator validator;

        public InvalidExtendedPropertyParameterRule() : base()
        {
            validator = new ExtendedPropertyValidator(ViolationHandlerWithMessage);
        }

        public override void Visit(ExecuteSpecification node) => node.Accept(validator);
    }
}
