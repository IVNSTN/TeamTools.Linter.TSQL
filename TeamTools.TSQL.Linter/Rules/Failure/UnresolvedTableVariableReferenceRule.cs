using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0459", "UNRESOLVED_TABLE_VAR_NAME")]
    internal sealed class UnresolvedTableVariableReferenceRule : AbstractRule
    {
        public UnresolvedTableVariableReferenceRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch node) => node.AcceptChildren(new VarVisitor(ViolationHandlerWithMessage));

        private sealed class VarVisitor : TSqlFragmentVisitor
        {
            private readonly HashSet<string> variables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            private readonly Action<TSqlFragment, string> callback;

            public VarVisitor(Action<TSqlFragment, string> callback)
            {
                this.callback = callback;
            }

            public override void ExplicitVisit(DeclareTableVariableBody node)
            {
                variables.Add(node.VariableName.Value);
            }

            // "Explicit" will prevent visiting the same object as DeclareVariableElement
            public override void ExplicitVisit(ProcedureParameter node)
            {
                // table-type parameter
                if (node.Modifier == ParameterModifier.ReadOnly)
                {
                    variables.Add(node.VariableName.Value);
                }
            }

            // DECLARE statement can contain table-type var declarations
            public override void Visit(DeclareVariableElement node)
            {
                if (node.DataType is UserDataTypeReference)
                {
                    variables.Add(node.VariableName.Value);
                }
            }

            // Validating reference
            public override void Visit(VariableTableReference node)
            {
                if (!variables.Contains(node.Variable.Name))
                {
                    callback(node.Variable, node.Variable.Name);
                }
            }
        }
    }
}
