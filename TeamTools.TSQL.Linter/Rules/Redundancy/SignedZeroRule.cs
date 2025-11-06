using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Globalization;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0731", "SIGNED_ZERO")]
    internal sealed class SignedZeroRule : AbstractRule
    {
        private readonly CultureInfo sqlCulture;

        public SignedZeroRule() : base()
        {
            sqlCulture = CultureInfo.CurrentCulture.Clone() as CultureInfo;
            sqlCulture.NumberFormat = new NumberFormatInfo
            {
                NumberDecimalSeparator = ".",
            };
        }

        public override void Visit(UnaryExpression node)
        {
            var expr = ExtractExpression(node.Expression);
            if (IsZero(expr) || IsNull(expr))
            {
                HandleNodeError(node);
            }
        }

        private static ScalarExpression ExtractExpression(ScalarExpression node)
        {
            while (node is ParenthesisExpression pe)
            {
                node = pe.Expression;
            }

            return node;
        }

        private static bool IsNull(ScalarExpression node) => node is NullLiteral;

        private bool IsZero(ScalarExpression node)
        => node is Literal l && decimal.TryParse(l.Value, NumberStyles.AllowDecimalPoint, sqlCulture, out decimal value) && value == 0;
    }
}
