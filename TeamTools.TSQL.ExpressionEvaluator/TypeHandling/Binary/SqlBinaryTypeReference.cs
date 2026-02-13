using TeamTools.TSQL.ExpressionEvaluator.Interfaces;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    public class SqlBinaryTypeReference : SqlStrTypeReference
    {
        public SqlBinaryTypeReference(string typeName, int size, ISqlValueFactory valueFactory)
        : base(typeName, size, valueFactory)
        {
        }
    }
}
