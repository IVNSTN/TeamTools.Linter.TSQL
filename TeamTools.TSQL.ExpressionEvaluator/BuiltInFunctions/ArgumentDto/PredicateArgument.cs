using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Diagnostics.CodeAnalysis;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto
{
    [ExcludeFromCodeCoverage]
    public class PredicateArgument : SqlFunctionArgument
    {
        public PredicateArgument(BooleanExpression predicate)
        {
            Predicate = predicate;
        }

        public BooleanExpression Predicate { get; }
    }
}
