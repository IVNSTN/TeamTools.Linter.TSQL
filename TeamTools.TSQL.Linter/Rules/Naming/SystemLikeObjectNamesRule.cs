using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("NM0206", "SYS_LIKE_NAME")]
    internal sealed class SystemLikeObjectNamesRule : AbstractRule
    {
        private readonly SystemProcDetector systemProcDetector = new SystemProcDetector();

        public SystemLikeObjectNamesRule() : base()
        {
        }

        public override void Visit(CreateProcedureStatement node) => ValidateObjectName(node.ProcedureReference.Name.BaseIdentifier);

        public override void Visit(CreateTableStatement node) => ValidateObjectName(node.SchemaObjectName.BaseIdentifier);

        public override void Visit(CreateTriggerStatement node) => ValidateObjectName(node.Name.BaseIdentifier);

        public override void Visit(CreateFunctionStatement node) => ValidateObjectName(node.Name.BaseIdentifier);

        public override void Visit(CreateTypeStatement node) => ValidateObjectName(node.Name.BaseIdentifier);

        public override void Visit(CreateSynonymStatement node) => ValidateObjectName(node.Name.BaseIdentifier);

        public override void Visit(CreateViewStatement node) => ValidateObjectName(node.SchemaObjectName.BaseIdentifier);

        private void ValidateObjectName(Identifier name)
        {
            if (systemProcDetector.IsSystemProc(name.Value))
            {
                HandleNodeError(name);
            }
        }
    }
}
