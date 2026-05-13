using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Text;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("SI0453", "EXTRACT_WINDOW_CLAUSE")]
    [CompatibilityLevel(SqlVersion.Sql160)]
    internal sealed partial class ExtractWindowClauseRule : AbstractRule
    {
        public ExtractWindowClauseRule() : base()
        {
        }

        public override void Visit(QuerySpecification node)
            => node.AcceptChildren(new OverClauseVisitor(ViolationHandler));
    }
}
