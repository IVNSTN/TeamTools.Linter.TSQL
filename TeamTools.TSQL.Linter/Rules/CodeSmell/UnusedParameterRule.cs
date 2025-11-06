using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0919", "UNUSED_PARAMETER")]
    internal class UnusedParameterRule : UnusedVariableRule
    {
        public UnusedParameterRule() : base()
        {
        }

        public override void Visit(TSqlBatch node)
        {
            // just hiding ancestor implementation
        }

        public override void Visit(FunctionStatementBody node)
        {
            if (node.StatementList == null && node.MethodSpecifier?.AssemblyName != null)
            {
                // CLR methods do not have statements and this is ok
                return;
            }

            ValidateBatch(node);
        }

        public override void Visit(ProcedureStatementBody node)
        {
            if (node.StatementList == null && node.MethodSpecifier?.AssemblyName != null)
            {
                // CLR methods do not have statements and this is ok
                return;
            }

            ValidateBatch(node);
        }

        private void ValidateBatch(TSqlFragment node)
        {
            var scalarVisitor = new ParamVariableVisitor();
            node.AcceptChildren(scalarVisitor);

            KeyValuePair<TSqlFragment, string> scalarsUnused = AggregateUnusedVars(scalarVisitor.VariableUsage);

            if (!string.IsNullOrEmpty(scalarsUnused.Value))
            {
                HandleNodeError(scalarsUnused.Key, scalarsUnused.Value);
            }
        }

        private class ParamVariableVisitor : ScalarVariableVisitor
        {
            public override void Visit(DeclareVariableElement node)
            {
                if (node is ProcedureParameter)
                {
                    base.Visit(node);
                }
            }

            public override void Visit(ProcedureParameter node)
            {
                // just hiding ancestor implementation which makes proc params ignored
            }
        }
    }
}
