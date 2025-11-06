using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0746", "DATE_FORMAT_CONTROL")]
    internal sealed class DateFormatControlRule : AbstractRule
    {
        public DateFormatControlRule() : base()
        {
        }

        public override void Visit(GeneralSetCommand node)
        {
            if (node.CommandType == GeneralSetCommandType.DateFirst
            || node.CommandType == GeneralSetCommandType.DateFormat)
            {
                HandleNodeError(node);
            }
        }
    }
}
