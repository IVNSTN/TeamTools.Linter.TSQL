using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Numerics;
using TeamTools.TSQL.ExpressionEvaluator.Interfaces;
using TeamTools.TSQL.ExpressionEvaluator.Routines;

namespace TeamTools.TSQL.ExpressionEvaluator.Values
{
    public class SqlLiteralValueFactory : ILiteralValueFactory
    {
        // TODO : switch to static FallbackTypeName properties of specific types/handlers/factories
        private static readonly string DefaultStringType = TSqlDomainAttributes.Types.Varchar;
        private static readonly string UnicodeStringType = TSqlDomainAttributes.Types.NVarchar;
        private static readonly string DefaultNumericType = TSqlDomainAttributes.Types.Int;
        private static readonly string BigIntType = TSqlDomainAttributes.Types.BigInt;
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
