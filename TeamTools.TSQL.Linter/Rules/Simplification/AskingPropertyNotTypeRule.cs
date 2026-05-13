using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("SI0889", "USE_TYPE_NOT_PROPERTY")]
    internal sealed partial class AskingPropertyNotTypeRule : AbstractRule
    {
        // docs: https://learn.microsoft.com/en-us/sql/t-sql/functions/objectpropertyex-transact-sql?view=sql-server-ver17#property
        private static readonly Dictionary<string, string> PropertiesForObjectTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "IsInlineFunction", "IF" },
            { "IsProcedure", "P" },
            { "IsScalarFunction", "FN" },
            { "IsTable", "U" },
            { "IsTableFunction", "TF" },
            { "IsTrigger", "TR" },
            { "IsView", "V" },
        };

        public AskingPropertyNotTypeRule() : base()
        {
        }

        public override void Visit(FunctionCall node)
        {
            if (!node.FunctionName.Value.Equals("OBJECTPROPERTY", StringComparison.OrdinalIgnoreCase)
            && !node.FunctionName.Value.Equals("OBJECTPROPERTYEX", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (node.Parameters.Count != 2)
            {
                return;
            }

            var propertyName = node.Parameters[1].ExtractScalarExpression();

            if (!(propertyName is StringLiteral s
            && PropertiesForObjectTypes.TryGetValue(s.Value, out var objectType)))
            {
                // not supported property name
                return;
            }

            var firstArg = ExpandExpression(node.Parameters[0]);

            if (!(firstArg is FunctionCall func
            && func.FunctionName.Value.Equals("OBJECT_ID", StringComparison.OrdinalIgnoreCase)))
            {
                // first arg is not OBJECT_ID() call
                return;
            }

            HandleNodeError(node, objectType);
        }

        private static ScalarExpression ExpandExpression(ScalarExpression expr)
        {
            while (expr is ParenthesisExpression pe)
            {
                expr = pe.Expression;
            }

            return expr;
        }
    }
}
