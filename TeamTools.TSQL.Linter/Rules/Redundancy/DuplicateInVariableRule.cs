using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0814", "IN_DUP_VAR")]
    internal sealed class DuplicateInVariableRule : AbstractRule
    {
        public DuplicateInVariableRule() : base()
        {
        }

        public override void Visit(InPredicate node)
        {
            if (node.Subquery != null)
            {
                return;
            }

            var variables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0, n = node.Values.Count; i < n; i++)
            {
                var val = node.Values[i];
                if (val.ExtractScalarExpression() is VariableReference varRef)
                {
                    if (!variables.Add(varRef.Name))
                    {
                        HandleNodeError(varRef, varRef.Name);
                    }
                }
            }
        }
    }
}
