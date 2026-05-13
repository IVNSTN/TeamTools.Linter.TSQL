using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0858", "FETCH_FULLY_QUALIFIED")]
    internal sealed class FetchFullyQualifiedRule : AbstractRule
    {
        public FetchFullyQualifiedRule() : base()
        {
        }

        public override void Visit(FetchCursorStatement node)
        {
            if (node.FetchType is null)
            {
                // Direction (NEXT, PRIOR, etc) omitted
                // FROM cannot be omitted if direction is specified
                HandleNodeError(node.Cursor);
            }
        }
    }
}
