using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public override void Visit(DeclareVariableStatement node)
            => ValidateVariableTypePosition(node.Declarations);

        public override void Visit(ProcedureStatementBody node)
            => ValidateVariableTypePosition(node.Parameters?.AsEnumerable<DeclareVariableElement>().ToList());

        public override void Visit(FunctionStatementBody node)
            => ValidateVariableTypePosition(node.Parameters?.AsEnumerable<DeclareVariableElement>().ToList());

        private static void DoValidateTypePosition(IList<DeclareVariableElement> declarations, int violationLimit, Action<TSqlFragment> callback)
        {
            int typeStartCol = declarations[0].DataType.StartColumn;

            var badFormat = declarations
                .Where(d => d.DataType.StartColumn != typeStartCol)
                .Take(violationLimit);

            foreach (var element in badFormat)
            {
                callback(element.DataType);
            }
        }

        private void ValidateVariableTypePosition(IList<DeclareVariableElement> declarations)
        {
            if (declarations is null || declarations.Count < 2)
            {
                // nothing to align
                return;
            }

            var last = declarations[declarations.Count - 1];

            if (declarations[0].StartLine == declarations[0].ScriptTokenStream[last.LastTokenIndex].Line)
            {
                // one-line blocks are ignored
                return;
            }

            DoValidateTypePosition(declarations, MaxViolationsPerDeclare, HandleNodeError);
        }
    }
}
