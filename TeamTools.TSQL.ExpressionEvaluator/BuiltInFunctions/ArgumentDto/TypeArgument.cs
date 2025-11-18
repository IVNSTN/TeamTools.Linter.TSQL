using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto
{
    [ExcludeFromCodeCoverage]
    public class TypeArgument : SqlFunctionArgument
    {
        public TypeArgument(SqlTypeReference typeRef)
        {
            TypeRef = typeRef;
        }

        public SqlTypeReference TypeRef { get; }
    }
}
