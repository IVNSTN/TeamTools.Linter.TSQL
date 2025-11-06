using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0161", "UNREACHABLE_CODE")]
    internal sealed class UnreachableCodeRule : AbstractRule
    {
        public UnreachableCodeRule() : base()
        {
        }

        public override void Visit(TSqlBatch node)
        {
            IDictionary<int, bool> checkedNodes = new Dictionary<int, bool>();
            var blockVisitor = new RecursiveQuitBlockVisitor(
                checkedNodes,
                (i, quitType) => HandleNodeError(i),
                new QuitBlockParserState());
            node.Accept(blockVisitor);
        }
    }
}
