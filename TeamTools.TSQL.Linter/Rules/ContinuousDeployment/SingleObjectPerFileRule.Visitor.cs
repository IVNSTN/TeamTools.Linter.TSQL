using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Diagnostics.CodeAnalysis;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    internal partial class SingleObjectPerFileRule
    {
        [ExcludeFromCodeCoverage]
        private sealed class CreateVisitor : VisitorWithCallback
        {
            public CreateVisitor(Action<TSqlFragment> callback) : base(callback)
            { }

            public int CreateCount { get; private set; } = 0;

            // Explicit - because such programmabilities might contain some other object creation
            // which is valid in case of current rule.
            public override void ExplicitVisit(CreateProcedureStatement node) => CreateDetected(node);

            public override void ExplicitVisit(CreateOrAlterProcedureStatement node) => CreateDetected(node);

            public override void ExplicitVisit(AlterProcedureStatement node) => CreateDetected(node);

            public override void ExplicitVisit(CreateTriggerStatement node) => CreateDetected(node);

            public override void ExplicitVisit(CreateOrAlterTriggerStatement node) => CreateDetected(node);

            public override void ExplicitVisit(AlterTriggerStatement node) => CreateDetected(node);

            public override void Visit(ViewStatementBody node) => CreateDetected(node);

            public override void Visit(FunctionStatementBody node) => CreateDetected(node);

            // ALTER TABLE ADD <something>, CREATE INDEX are fine to be in the same script with table creation
            public override void Visit(CreateTableStatement node) => CreateDetected(node);

            public override void Visit(CreateTypeStatement node) => CreateDetected(node);

            public override void Visit(CreateSynonymStatement node) => CreateDetected(node);

            public override void Visit(CreateSchemaStatement node) => CreateDetected(node);

            public override void Visit(CreateServiceStatement node) => CreateDetected(node);

            public override void Visit(CreateQueueStatement node) => CreateDetected(node);

            public override void Visit(CreateMessageTypeStatement node) => CreateDetected(node);

            public override void Visit(CreateContractStatement node) => CreateDetected(node);

            public override void Visit(CreateDefaultStatement node) => CreateDetected(node);

            public override void Visit(CreateRuleStatement node) => CreateDetected(node);

            public override void Visit(CreateRoleStatement node) => CreateDetected(node);

            public override void Visit(CreateUserStatement node) => CreateDetected(node);

            public override void Visit(CreateLoginStatement node) => CreateDetected(node);

            public override void Visit(CreatePartitionSchemeStatement node) => CreateDetected(node);

            public override void Visit(CreatePartitionFunctionStatement node) => CreateDetected(node);

            public override void Visit(CreateAssemblyStatement node) => CreateDetected(node);

            public override void Visit(CreateSequenceStatement node) => CreateDetected(node);

            private void CreateDetected(TSqlFragment node)
            {
                if (++CreateCount > 1)
                {
                    Callback(node);
                }
            }
        }
    }
}
