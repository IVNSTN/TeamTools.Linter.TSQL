using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0931", "NAME_REUSED_TRANSACTION")]
    internal sealed class ReusedTranNameRule : AbstractRule
    {
        public ReusedTranNameRule() : base()
        {
        }

        public override void Visit(TSqlScript node)
            => node.AcceptChildren(new TranNameVisitor(HandleNodeError));

        private class TranNameVisitor : TSqlFragmentVisitor
        {
            private readonly Action<TSqlFragment, string> callback;
            private readonly ICollection<string> tranNames = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

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

                if (!string.IsNullOrEmpty(tranName) && !tranNames.TryAddUnique(tranName))
                {
                    callback(name, tranName);
                }
            }
        }
    }
}
