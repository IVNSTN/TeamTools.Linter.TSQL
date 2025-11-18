using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Validating space between words in CREATE, DROP and ALTER statements.
    /// </summary>
    internal partial class SingleSpaceInTwoWordInstructionRule
    {
        public override void Visit(DropTableStatement node)
            => ValidateSpaceBetween(node, node.FirstTokenIndex, node.Objects[0].FirstTokenIndex - 1, "DROP TABLE");

        public override void Visit(CreateTableStatement node)
            => ValidateSpaceBetween(node, node.FirstTokenIndex, node.SchemaObjectName.FirstTokenIndex - 1, "CREATE TABLE");

        public override void Visit(AlterTableStatement node)
            => ValidateSpaceBetween(node, node.FirstTokenIndex, node.SchemaObjectName.FirstTokenIndex - 1, "ALTER TABLE");

        public override void Visit(ProcedureStatementBody node)
            => ValidateSpaceBetween(node, node.FirstTokenIndex, node.ProcedureReference.FirstTokenIndex - 1, "CREATE PROCEDURE");

        public override void Visit(DropProcedureStatement node)
            => ValidateSpaceBetween(node, node.FirstTokenIndex, node.Objects[0].FirstTokenIndex - 1, "DROP PROCEDURE");

        public override void Visit(FunctionStatementBody node)
            => ValidateSpaceBetween(node, node.FirstTokenIndex, node.Name.FirstTokenIndex - 1, "CREATE FUNCTION");

        public override void Visit(DropFunctionStatement node)
            => ValidateSpaceBetween(node, node.FirstTokenIndex, node.Objects[0].FirstTokenIndex - 1, "DROP FUNCTION");

        public override void Visit(TriggerStatementBody node)
            => ValidateSpaceBetween(node, node.FirstTokenIndex, node.Name.FirstTokenIndex - 1, "CREATE TRIGGER");

        public override void Visit(DropTriggerStatement node)
            => ValidateSpaceBetween(node, node.FirstTokenIndex, node.Objects[0].FirstTokenIndex - 1, "DROP TRIGGER");

        public override void Visit(ViewStatementBody node)
            => ValidateSpaceBetween(node, node.FirstTokenIndex, node.SchemaObjectName.FirstTokenIndex - 1, "CREATE VIEW");

        public override void Visit(DropViewStatement node)
            => ValidateSpaceBetween(node, node.FirstTokenIndex, node.Objects[0].FirstTokenIndex - 1, "DROP VIEW");

        public override void Visit(CreateIndexStatement node)
            => ValidateSpaceBetween(node, node.FirstTokenIndex, node.Name.FirstTokenIndex - 1, "CREATE INDEX");

        public override void Visit(DropIndexStatement node)
            => ValidateSpaceBetween(node, node.FirstTokenIndex, node.DropIndexClauses[0].FirstTokenIndex - 1, "DROP INDEX");

        public override void Visit(CreateSynonymStatement node)
            => ValidateSpaceBetween(node, node.FirstTokenIndex, node.Name.FirstTokenIndex - 1, "CREATE SYNONYM");

        public override void Visit(DropSynonymStatement node)
            => ValidateSpaceBetween(node, node.FirstTokenIndex, node.Objects[0].FirstTokenIndex - 1, "DROP SYNONYM");

        public override void Visit(CreateTypeStatement node)
            => ValidateSpaceBetween(node, node.FirstTokenIndex, node.Name.FirstTokenIndex - 1, "CREATE TYPE");

        public override void Visit(DropTypeStatement node)
            => ValidateSpaceBetween(node, node.FirstTokenIndex, node.Name.FirstTokenIndex - 1, "DROP TYPE");

        public override void Visit(CreateMessageTypeStatement node)
            => ValidateSpaceBetween(node, node.FirstTokenIndex, node.Name.FirstTokenIndex - 1, "CREATE MESSAGE TYPE");

        public override void Visit(CreateContractStatement node)
            => ValidateSpaceBetween(node, node.FirstTokenIndex, node.Name.FirstTokenIndex - 1, "CREATE CONTRACT");

        public override void Visit(CreateQueueStatement node)
            => ValidateSpaceBetween(node, node.FirstTokenIndex, node.Name.FirstTokenIndex - 1, "CREATE QUEUE");

        public override void Visit(CreateServiceStatement node)
            => ValidateSpaceBetween(node, node.FirstTokenIndex, node.Name.FirstTokenIndex - 1, "CREATE SERVICE");
    }
}
