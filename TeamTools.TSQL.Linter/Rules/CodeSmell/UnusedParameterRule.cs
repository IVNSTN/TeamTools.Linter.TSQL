using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0919", "UNUSED_PARAMETER")]
    internal class UnusedParameterRule : UnusedVariableRule
    {
        public UnusedParameterRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            // CREATE PROC/FUNC must be the first statement in a batch
            var firstStmt = batch.Statements[0];
            if (firstStmt is ProcedureStatementBodyBase procOrFunc)
            {
                ValidateProc(procOrFunc);
            }
        }

        private void ValidateProc(ProcedureStatementBodyBase node)
        {
            if (node.StatementList is null && node.MethodSpecifier?.AssemblyName != null)
            {
                // CLR methods do not have statements and this is ok
                return;
            }

            if (node.Parameters.Count == 0)
            {
                return;
            }

            ValidateParamsUsage(node);
        }

        private void ValidateParamsUsage(TSqlFragment node)
        {
            var scalarVisitor = new ParamVariableVisitor();
            node.AcceptChildren(scalarVisitor);

            if (scalarVisitor.VariableUsage.Count == 0)
            {
                return;
            }

            foreach (var unused in scalarVisitor.VariableUsage)
            {
                HandleNodeError(unused.Value, unused.Key);
            }
        }

        private sealed class ParamVariableVisitor : SomeVariableVisitor
        {
            public override void Visit(DeclareVariableElement node)
            {
                if (node is ProcedureParameter)
                {
                    base.Visit(node);
                }
            }

            // just hiding ancestor implementation which makes proc params ignored
            public override void Visit(ProcedureParameter node) { }

            // just hiding ancestor implementation
            public override void Visit(DeclareTableVariableBody node) { }
        }
    }
}
