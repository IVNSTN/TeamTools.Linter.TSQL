using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0457", "VARIABLE_REDECLARED")]
    internal sealed class VariableRedeclaredRule : AbstractRule
    {
        public VariableRedeclaredRule() : base()
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

            // This will also visit input parameter declarations
            public override void Visit(DeclareVariableElement node) => ValidateName(node.VariableName);

            // Same var name cannot be used both in scalar and table vars
            public override void Visit(DeclareTableVariableBody node) => ValidateName(node.VariableName);

            private void ValidateName(Identifier varName)
            {
                if (!variables.Add(varName.Value))
                {
                    // already registered
                    callback(varName, varName.Value);
                }
            }
        }
    }
}
