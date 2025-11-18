using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0734", "DECLARE_PER_VAR")]
    internal sealed class DeclarePerVarRule : AbstractRule
    {
        public DeclarePerVarRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch node)
        {
            node.Accept(new DeclareVisitor(ViolationHandler));
        }

        private sealed class DeclareVisitor : VisitorWithCallback
        {
            private static readonly int MaxVarsForSplit = 32;
            private int declareCount;
            private int lastDeclareVarCount;

            public DeclareVisitor(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(DeclareVariableStatement node)
            {
                declareCount++;

                if (declareCount > 1 && node.Declarations.Count <= MaxVarsForSplit
                && lastDeclareVarCount <= MaxVarsForSplit)
                {
                    Callback(node);
                }

                lastDeclareVarCount = node.Declarations.Count;
            }
        }
    }
}
