using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0931", "NAME_REUSED_TRANSACTION")]
    internal sealed class ReusedTranNameRule : AbstractRule
    {
        public ReusedTranNameRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
            => node.AcceptChildren(new TranNameVisitor(ViolationHandlerWithMessage));

        private class TranNameVisitor : TSqlFragmentVisitor
        {
            private readonly Action<TSqlFragment, string> callback;
            private readonly HashSet<string> tranNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            public TranNameVisitor(Action<TSqlFragment, string> callback)
            {
                this.callback = callback;
            }

            public override void Visit(BeginTransactionStatement node) => ValidateTranName(node.Name?.Identifier);

            public override void Visit(SaveTransactionStatement node) => ValidateTranName(node.Name?.Identifier);

            // TODO : if it was a variable then ExpressionEvaluator should be used
            private void ValidateTranName(Identifier name)
            {
                string tranName = name?.Value;

                if (!string.IsNullOrEmpty(tranName) && !tranNames.Add(tranName))
                {
                    callback(name, tranName);
                }
            }
        }
    }
}
