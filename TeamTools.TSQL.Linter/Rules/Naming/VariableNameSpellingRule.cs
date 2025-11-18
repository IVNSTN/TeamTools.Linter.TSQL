using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("NM0204", "VAR_NAME_MISSPELLED")]
    internal sealed class VariableNameSpellingRule : AbstractRule
    {
        public VariableNameSpellingRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch node) => node.Accept(new DeclarationVisitor(ViolationHandlerWithMessage));

        private sealed class DeclarationVisitor : TSqlFragmentVisitor
        {
            private readonly Dictionary<string, string> variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            private readonly List<VariableReference> ignoredExecParams = new List<VariableReference>();
            private readonly Action<TSqlFragment, string> callback;

            public DeclarationVisitor(Action<TSqlFragment, string> callback)
            {
                this.callback = callback;
            }

            public override void Visit(DeclareVariableElement node)
            {
                variables[node.VariableName.Value] = node.VariableName.Value;
            }

            public override void Visit(VariableReference node)
            {
                string currentSpelling = node.Name;

                if (!variables.TryGetValue(currentSpelling, out string originalSpelling))
                {
                    // exec proc params reach this place somehow
                    return;
                }

                if (ignoredExecParams.Contains(node))
                {
                    return;
                }

                if (string.Equals(currentSpelling, originalSpelling))
                {
                    return;
                }

                callback(node, string.Format("{0} vs {1}", currentSpelling, originalSpelling));
            }

            public override void Visit(ExecuteParameter node)
            {
                if (node.Variable is null)
                {
                    return;
                }

                // Referenced proc arg names should be ignored
                ignoredExecParams.Add(node.Variable);
            }
        }
    }
}
