using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0175", "RAISERROR_VALID_NUMBER")]
    internal class RaisErrorNumberValueRule : AbstractRule
    {
        private const int MinCorrectErrorNumberValue = 50000;
        private const int MaxCorrectErrorNumberValue = 9999999;

        public RaisErrorNumberValueRule() : base()
        {
        }

        public override void Visit(ThrowStatement node)
        {
            ValidateErrorNumber(node.ErrorNumber);
        }

        public override void Visit(RaiseErrorStatement node)
        {
            ValidateErrorNumber(node.FirstParameter);
        }

        protected void ValidateErrorNumber(TSqlFragment node)
        {
            if ((null == node) || !(node is IntegerLiteral intLiteral))
            {
                return;
            }

            if (!int.TryParse(intLiteral.Value, out int errnumValue))
            {
                return;
            }

            if (errnumValue >= MinCorrectErrorNumberValue && errnumValue <= MaxCorrectErrorNumberValue)
            {
                return;
            }

            HandleNodeError(node, errnumValue.ToString());
        }
    }
}
