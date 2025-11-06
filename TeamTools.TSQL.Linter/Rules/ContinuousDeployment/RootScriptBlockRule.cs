using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CD0726", "CODE_IN_SCRIPT_ROOT")]
    internal sealed class RootScriptBlockRule : AbstractRule
    {
        private readonly ICollection<Type> forbiddenBlocks;

        public RootScriptBlockRule() : base()
        {
            forbiddenBlocks = new List<Type>
            {
                typeof(BeginEndAtomicBlockStatement),
                typeof(BeginEndBlockStatement),
                typeof(IfStatement),
                typeof(WhileStatement),
                typeof(SetVariableStatement),
                typeof(SelectStatement),
                typeof(DeclareVariableStatement),
                typeof(DeclareCursorStatement),
                typeof(TryCatchStatement),
            };
        }

        public override void Visit(TSqlBatch node)
        {
            foreach (var stmt in node.Statements)
            {
                if (forbiddenBlocks.Any(tp => stmt.GetType().IsAssignableFrom(tp)))
                {
                    HandleNodeError(stmt);
                }
            }
        }
    }
}
