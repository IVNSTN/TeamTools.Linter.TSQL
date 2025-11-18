using System;
using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.Values;
using TeamTools.TSQL.ExpressionEvaluator.Violations;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    // TODO : refactor first then cover with tests
    // FIXME : fix Source for new values. maybe pass from outside
    [ExcludeFromCodeCoverage]
    public static class SqlStrTypeConverter
    {
        public static SqlStrTypeValue ConvertValueFrom(this SqlStrTypeHandler typeHandler, SqlValue src, string targetType)
        {
            int targetSize = typeHandler.StrValueFactory.GetDefaultTypeSize(targetType);

            if (src is SqlStrTypeValue strSrc)
            {
                if (string.Equals(strSrc.TypeName, targetType, StringComparison.OrdinalIgnoreCase))
                {
                    return strSrc;
                }

                targetSize = strSrc.EstimatedSize;
            }

            var targetTypeRef = typeHandler.StrValueFactory
                .MakeSqlTypeReference(targetType, targetSize);

            return typeHandler.DoConvertValueFrom(src, targetTypeRef, false, false);
        }

        public static SqlStrTypeValue ConvertValueFrom(this SqlStrTypeHandler typeHandler, SqlValue src, SqlStrTypeReference targetType, bool forceTargetType)
        {
            return typeHandler.DoConvertValueFrom(src, targetType, true, forceTargetType);
        }

        private static SqlStrTypeValue DoConvertValueFrom(
            this SqlStrTypeHandler typeHandler,
            SqlValue src,
            SqlStrTypeReference targetType,
            bool strictTargetSize,
            bool forceTargetType)
        {
            if (src is null)
            {
                return default;
            }

            if (targetType is null)
            {
                return default;
            }

            if (src.IsNull)
            {
                // TODO : to factory
                // return typeHandler.StrValueFactory.MakeNullValue(targetType.TypeName);
                return new SqlStrTypeValue(
                    typeHandler,
                    targetType,
                    SqlValueKind.Null,
                    src.Source);
            }

            // converting from int
            if (src is SqlIntTypeValue intSrc)
            {
                if (!intSrc.IsPreciseValue)
                {
                    var maxLength = intSrc.EstimatedSize.High.ToString().Length;
                    var lowMaxLength = intSrc.EstimatedSize.Low.ToString().Length;
                    if (lowMaxLength > maxLength)
                    {
                        // TODO : Int values are mostly treated as positive and convertible
                        // into 10-symbols string. However this is not essentially right
                        // negative integer can be 11 symbols long because of "-".
                        maxLength = lowMaxLength - 1;
                    }

                    if (targetType.Size < maxLength)
                    {
                        // target type size is not enough to store source value
                        // FIXME : this is the very wrong Source provided here
                        typeHandler.Violations.RegisterViolation(new ImplicitTruncationViolation(targetType.Size, maxLength, null, intSrc.Source));
                    }
                    else if (targetType.Size > maxLength)
                    {
                        // source value cannot be that long
                        // TODO : register specific violation
                    }

                    int targetSize = forceTargetType ? targetType.Size : maxLength;

                    return typeHandler.StrValueFactory.MakeApproximateValue(targetType.TypeName, targetSize, src.Source);
                }

                string strValue = intSrc.Value.ToString();

                // FIXME : magic
                if (targetType.Size < 0)
                {
                    // TODO : or unknown?
                    return typeHandler.StrValueFactory.MakePreciseValue(targetType.TypeName, strValue, src.Source);
                }

                var resultSrc = src.Source;
                if (strictTargetSize && strValue.Length > targetType.Size)
                {
                    typeHandler.Violations.RegisterViolation(new ImplicitTruncationViolation(targetType.Size, strValue.Length, strValue, intSrc.Source));
                    strValue = strValue.Substring(0, targetType.Size);
                    resultSrc = new SqlValueSource(SqlValueSourceKind.Expression, src.Source.Node);
                }

                return typeHandler.StrValueFactory.MakePreciseValue(targetType.TypeName, strValue, resultSrc);
            }

            // converting from bigint
            if (src is SqlBigIntTypeValue bigintSrc)
            {
                if (!bigintSrc.IsPreciseValue)
                {
                    var maxLength = bigintSrc.EstimatedSize.High.ToString().Length;
                    var lowMaxLength = bigintSrc.EstimatedSize.Low.ToString().Length;
                    if (lowMaxLength > maxLength)
                    {
                        maxLength = lowMaxLength;
                    }

                    if (targetType.Size < maxLength)
                    {
                        typeHandler.Violations.RegisterViolation(new ImplicitTruncationViolation(targetType.Size, maxLength, null, bigintSrc.Source));
                    }

                    int targetSize = forceTargetType ? targetType.Size : maxLength;

                    return typeHandler.StrValueFactory.MakeApproximateValue(targetType.TypeName, targetSize, src.Source);
                }

                string strValue = bigintSrc.Value.ToString();

                // FIXME : magic
                if (targetType.Size < 0)
                {
                    // TODO : or unknown?
                    return typeHandler.StrValueFactory.MakePreciseValue(targetType.TypeName, strValue, src.Source);
                }

                var resultSrc = src.Source;
                if (strictTargetSize && strValue.Length > targetType.Size)
                {
                    typeHandler.Violations.RegisterViolation(new ImplicitTruncationViolation(targetType.Size, strValue.Length, strValue, bigintSrc.Source));
                    strValue = strValue.Substring(0, targetType.Size);
                    resultSrc = new SqlValueSource(SqlValueSourceKind.Expression, src.Source.Node);
                }

                return typeHandler.StrValueFactory.MakePreciseValue(targetType.TypeName, strValue, resultSrc);
            }

            // converting from other string
            if (src is SqlStrTypeValue strSrc)
            {
                // CHAR/VARCHAR can be empty but minimum string storage size is 1 char
                int definedTargetSize = targetType.Size == 0 ? 1 : targetType.Size;

                if (!strSrc.IsPreciseValue)
                {
                    // FIXME : in case of Implicit conversion it should be strSrc.EstimatedSize
                    // in case of Explicit convertion it should be targetType.Size
                    int targetSize = forceTargetType ? definedTargetSize : strSrc.EstimatedSize;

                    if (definedTargetSize < strSrc.EstimatedSize)
                    {
                        // FIXME : in case of explicit convertion
                        // no ImplicitTruncation violation should be generated
                        if (strictTargetSize)
                        {
                            // FIXME : wrong source is provided here
                            // current node is expected, not the node where another
                            // variable was initialized
                            typeHandler.Violations.RegisterViolation(new ImplicitTruncationViolation(definedTargetSize, strSrc.EstimatedSize, null, strSrc.Source));
                        }

                        targetSize = definedTargetSize;
                    }

                    if (!targetType.IsUnicode && !strSrc.IsNull
                    && strSrc.TypeReference is SqlStrTypeReference strRef && strRef.IsUnicode)
                    {
                        typeHandler.Violations.RegisterViolation(new NationalSymbolLossViolation(strSrc.TypeName, strSrc.Source));
                    }

                    return typeHandler.StrValueFactory.MakeApproximateValue(targetType.TypeName, targetSize, src.Source);
                }

                string strValue = strSrc.Value;
                var resultSrc = strSrc.Source;
                // FIXME: get code page from args
                const int MaxStringLengthToAnalyze = 300;
                const int CodePage = 1251;

                // If source value is unicode type, does contain a unicode symbol
                // however target type does not support unicode
                if (!targetType.IsUnicode && !strSrc.IsNull
                && !string.IsNullOrWhiteSpace(strValue)
                && strSrc.TypeReference is SqlStrTypeReference srcStrRef && srcStrRef.IsUnicode
                && strValue.Length <= MaxStringLengthToAnalyze && strValue.ContainsNationalCharacter(CodePage))
                {
                    typeHandler.Violations.RegisterViolation(new NationalSymbolLossViolation(strSrc.TypeName, strSrc.Source));
                }

                // FIXME : magic for unpredictable size
                if (targetType.Size < 0)
                {
                    // TODO : or unknown?
                    return typeHandler.StrValueFactory.MakePreciseValue(targetType.TypeName, strValue, src.Source);
                }

                if (strictTargetSize && definedTargetSize < strValue.Length)
                {
                    // FIXME : how to distinct implicit truncation from explicit one?
                    // in case of implicit a violation warning is needed.
                    typeHandler.Violations.RegisterViolation(new ImplicitTruncationViolation(definedTargetSize, strValue.Length, strValue, strSrc.Source));

                    strValue = strValue.Substring(0, definedTargetSize);
                    resultSrc = new SqlValueSource(SqlValueSourceKind.Expression, src.Source.Node);
                }

                return typeHandler.StrValueFactory.MakePreciseValue(targetType.TypeName, strValue, resultSrc);
            }

            return default;
        }
    }
}
