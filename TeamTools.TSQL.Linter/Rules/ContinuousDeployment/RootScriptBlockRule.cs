using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CD0726", "CODE_IN_SCRIPT_ROOT")]
    internal sealed class RootScriptBlockRule : AbstractRule
    {
        private static readonly List<Type> ForbiddenBlocks = new List<Type>
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

        public RootScriptBlockRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch node)
        {
            // We need top-level statements only thus not diving into nested blocks
            int n = node.Statements.Count;
            for (int i = 0; i < n; i++)
            {
                var stmt = node.Statements[i];
                if (IsForbidden(stmt.GetType()))
                {
                    HandleNodeError(stmt);
                }
            }
        }

        private static bool IsForbidden(Type scriptBlock)
        {
            int n = ForbiddenBlocks.Count;
            for (int i = 0; i < n; i++)
            {
                if (scriptBlock.IsAssignableFrom(ForbiddenBlocks[i]))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
