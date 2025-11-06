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

        public override void Visit(TSqlBatch node)
        {
            var varVisitor = new DeclarationVisitor(HandleNodeError);
            node.AcceptChildren(varVisitor);
        }

        private class DeclarationVisitor : TSqlFragmentVisitor
        {
            private readonly IDictionary<string, string> variables = new SortedDictionary<string, string>();
            private readonly IDictionary<KeyValuePair<int, int>, string> procParams = new Dictionary<KeyValuePair<int, int>, string>();
            private readonly Action<TSqlFragment, string> callback;

            public DeclarationVisitor(Action<TSqlFragment, string> callback)
            {
                this.callback = callback;
            }

            public override void Visit(DeclareVariableElement node)
            {
                variables[node.VariableName.Value.ToLower()] = node.VariableName.Value;
            }

            public override void Visit(ExecutableProcedureReference node)
            {
                var procParamVisitor = new ProcParamVisitor(procParams);

                node.AcceptChildren(procParamVisitor);
            }

            public override void Visit(VariableReference node)
            {
                string key = node.Name.ToLower();

                if (!variables.ContainsKey(key))
                {
                    // exec proc params reach this place somehow
                    return;
                }

                // TODO : mostlikely StartOffset is misunderstood here
                var paramKey = new KeyValuePair<int, int>(node.StartLine, node.StartColumn + node.StartOffset);

                if (procParams.ContainsKey(paramKey))
                {
                    return;
                }

                if (string.Equals(node.Name, variables[key]))
                {
                    return;
                }

                callback(node, string.Format("{0} vs {1}", node.Name, variables[key]));
            }
        }

        private class ProcParamVisitor : TSqlFragmentVisitor
        {
            private readonly IDictionary<KeyValuePair<int, int>, string> procParams;

            public ProcParamVisitor(IDictionary<KeyValuePair<int, int>, string> procParams) : base()
            {
                this.procParams = procParams;
            }

            public override void Visit(ExecuteParameter node)
            {
                if (node.Variable == null)
                {
                    return;
                }

                // TODO : most likely StartOffset is misunderstood here
                var paramKey = new KeyValuePair<int, int>(node.StartLine, node.StartColumn + node.StartOffset);

                if (!procParams.ContainsKey(paramKey))
                {
                    procParams.Add(paramKey, node.Variable.Name);
                }
            }
        }
    }
}
