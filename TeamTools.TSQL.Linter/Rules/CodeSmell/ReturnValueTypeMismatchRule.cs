using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0785", "RETURN_VALUE_TYPE_MISMATCH")]
    internal sealed partial class ReturnValueTypeMismatchRule : ScriptAnalysisServiceConsumingRule
    {
        public ReturnValueTypeMismatchRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            // CREATE PROC/TRIGGER/FUNC must be the first statement in a batch
            var firstStmt = batch.Statements[0];
            if (firstStmt is FunctionStatementBody fn)
            {
                DoVisit(fn);
            }
            else if (firstStmt is ProcedureStatementBody proc)
            {
                DoVisit(proc);
            }
        }

        private void DoVisit(FunctionStatementBody node)
        {
            if (node.StatementList is null)
            {
                // CLR
                return;
            }

            if (!(node.ReturnType is ScalarFunctionReturnType ret))
            {
                // we need scalar-valued functions only
                return;
            }

            var typeName = ret.DataType.Name;
            if (typeName.SchemaIdentifier != null
            && !string.Equals(typeName.SchemaIdentifier.Value, TSqlDomainAttributes.SystemSchemaName, StringComparison.OrdinalIgnoreCase))
            {
                // compatibility of user-defined types with system types is undefined
                return;
            }

            ValidateReturnedType(typeName.BaseIdentifier.Value, node);
        }

        private void DoVisit(ProcedureStatementBody node)
        {
            if (node.StatementList is null)
            {
                // CLR
                return;
            }

            ValidateReturnedType(TSqlDomainAttributes.Types.Int, node);
        }
    }
}
