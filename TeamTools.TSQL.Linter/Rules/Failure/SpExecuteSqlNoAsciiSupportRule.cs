using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0996", "SP_EXECUTE_SQL_NO_ASCII")]
    internal sealed class SpExecuteSqlNoAsciiSupportRule : AbstractRule
    {
        public SpExecuteSqlNoAsciiSupportRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch batch) => batch.AcceptChildren(new ExecValidator(ViolationHandler));

        private sealed class ExecValidator : VisitorWithCallback
        {
            private static readonly string ProcName = "sp_executesql";
            private readonly Dictionary<string, string> variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            public ExecValidator(Action<TSqlFragment> callback) : base(callback)
            {
            }

            public override void Visit(DeclareVariableElement node)
            {
                variables.TryAdd(node.VariableName.Value, node.DataType.GetFullName());
            }

            public override void Visit(ExecuteSpecification node)
            {
                if (!(node.ExecutableEntity is ExecutableProcedureReference proc
                && proc.ProcedureReference?.ProcedureReference?.Name != null
                && proc.Parameters.Count > 0))
                {
                    // dynamic proc name or proc with no args
                    return;
                }

                var procName = proc.ProcedureReference.ProcedureReference.Name;
                if (!((procName.SchemaIdentifier is null || procName.SchemaIdentifier.Value.Equals(TSqlDomainAttributes.SystemSchemaName, StringComparison.OrdinalIgnoreCase))
                && procName.BaseIdentifier.Value.Equals(ProcName, StringComparison.OrdinalIgnoreCase)))
                {
                    // not our proc
                    return;
                }

                ValidateSpExecuteSqlArg(proc.Parameters[0]);
                if (proc.Parameters.Count > 1)
                {
                    ValidateSpExecuteSqlArg(proc.Parameters[1]);
                }
            }

            private void ValidateSpExecuteSqlArg(ExecuteParameter arg)
            {
                if (arg.Variable != null)
                {
                    // For sp_executesql first two args must be not named.
                    // A separate rule should verify this.
                    return;
                }

                if (arg.ParameterValue is StringLiteral str && str.IsNational)
                {
                    // string literal is prefixed with N''
                    return;
                }

                if (arg.ParameterValue is VariableReference varRef
                && variables.TryGetValue(varRef.Name, out var varType)
                && (string.Equals(varType, "NVARCHAR", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(varType, "NCHAR", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(varType, "NTEXT", StringComparison.OrdinalIgnoreCase)))
                {
                    // variable has on of supported types
                    return;
                }

                Callback(arg);
            }
        }
    }
}
