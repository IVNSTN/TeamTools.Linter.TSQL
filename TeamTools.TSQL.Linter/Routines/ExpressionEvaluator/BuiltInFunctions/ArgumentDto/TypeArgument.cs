using System.Diagnostics.CodeAnalysis;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions
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
