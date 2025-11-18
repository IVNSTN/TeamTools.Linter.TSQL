using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    // FIXME : looks like a dup of NonDeterministicConditionRule
    [RuleIdentity("CS0743", "INVARIANT_CASE_ARG")]
    internal sealed class InvariantCaseArgumentRule : AbstractRule
    {
        private readonly VaryingArgDetector badArgDetector;

        public InvariantCaseArgumentRule() : base()
        {
            badArgDetector = new VaryingArgDetector(ViolationHandler);
        }

        public override void Visit(SearchedCaseExpression node) => ValidateExpression(node);

        public override void Visit(SimpleCaseExpression node) => ValidateExpression(node);

        public override void Visit(CoalesceExpression node) => ValidateExpression(node);

        public override void Visit(NullIfExpression node) => ValidateExpression(node);

        public override void Visit(IIfCall node) => ValidateExpression(node);

        private void ValidateExpression(TSqlFragment node)
        {
            node.AcceptChildren(badArgDetector);
        }

        private sealed class VaryingArgDetector : VisitorWithCallback
        {
            private static readonly HashSet<string> ForbiddenFunctions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "CRYPT_GET_RANDOM",
                "NEWID",
                "NEWSEQUENTIALID",
                "RAND",
            };

            public VaryingArgDetector(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(FunctionCall node)
            {
                if (ForbiddenFunctions.Contains(node.FunctionName.Value))
                {
                    Callback(node);
                }
            }

            public override void Visit(QueryExpression node) => Callback(node);
        }
    }
}
