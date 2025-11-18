using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.TSQL.ExpressionEvaluator.Routines;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    public static class SqlStrTypeDefinitionParser
    {
        public static int GetTypeSize(DataTypeReference datatype, Func<string, int> getDefaultSize)
        {
            if (datatype?.Name is null)
            {
                return default;
            }

            string typeName = datatype.GetFullName();
            int defaultSize = getDefaultSize.Invoke(typeName);

            if (defaultSize <= 0)
            {
                // unsupported type
                return default;
            }

            if (datatype is SqlDataTypeReference sqlType && sqlType.Parameters.Count > 0)
            {
                return GetTypeLengthValue(sqlType.Parameters[0]);
            }

            // type definition has no size parameter
            return defaultSize;
        }

        private static int GetTypeLengthValue(ScalarExpression node)
        {
            while (node is ParenthesisExpression pe)
            {
                node = pe.Expression;
            }

            if (node is MaxLiteral)
            {
                return int.MaxValue;
            }

            if (node is IntegerLiteral i && int.TryParse(i.Value, out int size))
            {
                return size;
            }

            return default;
        }
    }
}
