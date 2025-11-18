using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0270", "VAR_TYPE_ALIGNED")]
    internal sealed class VariableTypeAlignedRule : AbstractRule
    {
        private static readonly int MaxViolationsPerDeclare = 3;

        public VariableTypeAlignedRule() : base()
        {
        }

        public override void Visit(DeclareVariableStatement node) => ValidateVariableTypePosition(node.Declarations);

        public override void Visit(ProcedureStatementBody node) => ValidateVariableTypePosition(node.Parameters);

        public override void Visit(FunctionStatementBody node) => ValidateVariableTypePosition(node.Parameters);

        private static void DoValidateTypePosition<T>(IList<T> declarations, int violationLimit, Action<TSqlFragment> callback)
        where T : DeclareVariableElement
        {
            int typeStartCol = declarations[0].DataType.StartColumn;

            int n = declarations.Count;
            int violationCount = 0;

            for (int i = 0; i < n && violationCount < violationLimit; i++)
            {
                var dtp = declarations[i].DataType;
                if (dtp.StartColumn != typeStartCol)
                {
                    callback(dtp);
                    violationCount++;
                }
            }
        }

        private void ValidateVariableTypePosition<T>(IList<T> declarations)
        where T : DeclareVariableElement
        {
            if (declarations is null || declarations.Count < 2)
            {
                // nothing to align
                return;
            }

            var last = declarations[declarations.Count - 1];
            var first = declarations[0];

            if (first.StartLine == first.ScriptTokenStream[last.LastTokenIndex].Line)
            {
                // one-line blocks are ignored
                return;
            }

            DoValidateTypePosition(declarations, MaxViolationsPerDeclare, ViolationHandler);
        }
    }
}
