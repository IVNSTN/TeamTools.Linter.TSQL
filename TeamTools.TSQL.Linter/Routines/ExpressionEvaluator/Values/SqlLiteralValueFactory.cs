using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Numerics;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
{
    public class SqlLiteralValueFactory : ILiteralValueFactory
    {
        // TODO : switch to static FallbackTypeName properties of specific types/handlers/factories
        private static readonly string DefaultStringType = "dbo.VARCHAR";
        private static readonly string UnicodeStringType = "dbo.NVARCHAR";
        private static readonly string DefaultNumericType = "dbo.INT";
        private static readonly string BigIntType = "dbo.BIGINT";
        private static readonly string FallbackType = DefaultStringType;

        private readonly ISqlTypeResolver typeResolver;

        public SqlLiteralValueFactory(ISqlTypeResolver typeResolver)
        {
            this.typeResolver = typeResolver ?? throw new ArgumentNullException(nameof(typeResolver));
        }

        public SqlValue Make(Literal src)
        {
            string typeName = DefaultStringType;

            if (src is StringLiteral str)
            {
                if (str.IsNational)
                {
                    typeName = UnicodeStringType;
                }
            }
            else if (int.TryParse(src.Value, out int _))
            {
                typeName = DefaultNumericType;
            }
            else if (BigInteger.TryParse(src.Value, out BigInteger _))
            {
                typeName = BigIntType;
            }
            else
            {
                // ScriptDom treats as literals MAX keyword from VARCHAR size for example
                // MaxLiteral class is Literal descendant
                return default;
            }

            return typeResolver
                .ResolveTypeHandler(typeName)?
                .ValueFactory
                .NewLiteral(typeName, src.Value, src);
        }

        public SqlValue MakeNull(TSqlFragment node)
        {
            return typeResolver
                .ResolveTypeHandler(FallbackType)?
                .ValueFactory
                .NewNull(node);
        }
    }
}
