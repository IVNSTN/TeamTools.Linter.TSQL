using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using TeamTools.TSQL.ExpressionEvaluator.Values;
using TeamTools.TSQL.ExpressionEvaluator.Violations;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    // TODO : refactor first then cover with tests
    // FIXME : fix Source for new values. maybe pass from outside
    [ExcludeFromCodeCoverage]
    public static class SqlBinaryTypeConverter
    {
        public static SqlBinaryTypeValue ConvertValueFrom(this SqlBinaryTypeHandler typeHandler, SqlValue src, string targetType)
        {
            int targetSize = typeHandler.BinaryValueFactory.GetDefaultTypeSize(targetType);

            if (src is SqlBinaryTypeValue binSrc)
            {
                if (string.Equals(binSrc.TypeName, targetType, StringComparison.OrdinalIgnoreCase))
                {
                    return binSrc;
                }

                targetSize = binSrc.EstimatedSize;
            }

            var targetTypeRef = typeHandler.BinaryValueFactory
                .MakeSqlDataTypeReference(targetType, targetSize);

            return typeHandler.DoConvertValueFrom(src, targetTypeRef, false, false);
        }

        public static SqlBinaryTypeValue ConvertValueFrom(this SqlBinaryTypeHandler typeHandler, SqlValue src, SqlBinaryTypeReference targetType, bool forceTargetType)
        {
            return typeHandler.DoConvertValueFrom(src, targetType, true, forceTargetType);
        }

        private static SqlBinaryTypeValue DoConvertValueFrom(
            this SqlBinaryTypeHandler typeHandler,
            SqlValue src,
            SqlBinaryTypeReference targetType,
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
                // return typeHandler.BinaryValueFactory.MakeNullValue(targetType.TypeName);
                return new SqlBinaryTypeValue(
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
                    int maxLength = Math.Max(EvalNumberAsHexLength(intSrc.EstimatedSize.Low), EvalNumberAsHexLength(intSrc.EstimatedSize.High));

                    if (targetType.Size < maxLength)
                    {
                        // target type size is not enough to store source value
                        // FIXME : this is the very wrong Source provided here
                        typeHandler.Violations.RegisterViolation(new ImplicitTruncationViolation(targetType.Size, maxLength, null, src.Source));
                    }
                    else if (targetType.Size > maxLength)
                    {
                        // source value cannot be that long
                        // TODO : register specific violation
                    }

                    int targetSize = forceTargetType ? targetType.Size : maxLength;

                    return typeHandler.BinaryValueFactory.MakeApproximateValue(targetType.TypeName, targetSize, src.Source);
                }

                int minResultLength = 0;
                if (strictTargetSize)
                {
                    if (!targetType.HasFixedLength && targetType.Size > intSrc.TypeReference.Bytes)
                    {
                        minResultLength = intSrc.TypeReference.Bytes;
                    }
                    else
                    {
                        minResultLength = targetType.Size;
                    }
                }

                var hexValue = new HexValue(intSrc.Value, minResultLength);

                // FIXME : magic
                if (targetType.Size < 0)
                {
                    // TODO : or unknown?
                    return typeHandler.BinaryValueFactory.MakePreciseValue(targetType.TypeName, hexValue, src.Source);
                }

                var resultSrc = src.Source;
                const int MaxPowerToCheck = 16;
                if (strictTargetSize && targetType.Size <= MaxPowerToCheck && Math.Abs((double)intSrc.Value) > Math.Pow(16, targetType.Size))
                {
                    // TODO : this is not a _string_ truncation
                    typeHandler.Violations.RegisterViolation(new ImplicitTruncationViolation(targetType.Size, hexValue.AsString.Length, hexValue.AsString, src.Source));
                    // truncate BINARY correctly
                    hexValue.AsString = hexValue.AsString.Substring(0, targetType.Size * 2);
                    resultSrc = new SqlValueSource(SqlValueSourceKind.Expression, src.Source.Node);
                }

                return typeHandler.BinaryValueFactory.MakePreciseValue(targetType.TypeName, hexValue, resultSrc);
            }

            // converting from bigint
            if (src is SqlBigIntTypeValue bigintSrc)
            {
                if (!bigintSrc.IsPreciseValue)
                {
                    int maxLength = Math.Max(EvalNumberAsHexLength(bigintSrc.EstimatedSize.Low), EvalNumberAsHexLength(bigintSrc.EstimatedSize.High));

                    if (targetType.Size < maxLength)
                    {
                        typeHandler.Violations.RegisterViolation(new ImplicitTruncationViolation(targetType.Size, maxLength, null, src.Source));
                    }

                    int targetSize = forceTargetType ? targetType.Size : maxLength;

                    return typeHandler.BinaryValueFactory.MakeApproximateValue(targetType.TypeName, targetSize, src.Source);
                }

                int minResultLength = 0;
                if (strictTargetSize)
                {
                    if (!targetType.HasFixedLength && targetType.Size > bigintSrc.TypeReference.Bytes)
                    {
                        minResultLength = bigintSrc.TypeReference.Bytes;
                    }
                    else
                    {
                        minResultLength = targetType.Size;
                    }
                }

                var hexValue = new HexValue(bigintSrc.Value, minResultLength);

                // FIXME : magic
                if (targetType.Size < 0)
                {
                    // TODO : or unknown?
                    return typeHandler.BinaryValueFactory.MakePreciseValue(targetType.TypeName, hexValue, src.Source);
                }

                var resultSrc = src.Source;
                const int MaxPowerToCheck = 16;
                if (strictTargetSize && targetType.Size <= MaxPowerToCheck && Math.Abs((double)bigintSrc.Value) > Math.Pow(16, targetType.Size))
                {
                    typeHandler.Violations.RegisterViolation(new ImplicitTruncationViolation(targetType.Size, hexValue.AsString.Length, hexValue.AsString, src.Source));
                    // truncate BINARY correctly
                    hexValue.AsString = hexValue.AsString.Substring(0, targetType.Size * 2);
                    resultSrc = new SqlValueSource(SqlValueSourceKind.Expression, src.Source.Node);
                }

                return typeHandler.BinaryValueFactory.MakePreciseValue(targetType.TypeName, hexValue, resultSrc);
            }

            // TODO : see also binary conversion styles
            // https://learn.microsoft.com/en-us/sql/t-sql/functions/cast-and-convert-transact-sql?view=sql-server-ver17&f1url=%3FappId%3DDev15IDEF1%26l%3DEN-US%26k%3Dk(convert_TSQL)%3Bk(sql13.swb.tsqlresults.f1)%3Bk(sql13.swb.tsqlquery.f1)%3Bk(MiscellaneousFilesProject)%3Bk(DevLang-TSQL)%26rd%3Dtrue#binary-styles
            // converting from other string
            if (src is SqlStrTypeValue strSrc)
            {
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
                            typeHandler.Violations.RegisterViolation(new ImplicitTruncationViolation(definedTargetSize, strSrc.EstimatedSize, null, src.Source));
                        }

                        targetSize = definedTargetSize;
                    }

                    if (!targetType.IsUnicode && !strSrc.IsNull
                    && strSrc.TypeReference is SqlStrTypeReference strRef && strRef.IsUnicode)
                    {
                        typeHandler.Violations.RegisterViolation(new NationalSymbolLossViolation(strSrc.TypeName, src.Source));
                    }

                    return typeHandler.BinaryValueFactory.MakeApproximateValue(targetType.TypeName, targetSize, src.Source);
                }

                string strValue = strSrc.Value;
                var resultSrc = strSrc.Source;

                // TODO : if no style provided then binary result should be === source string as byte array
                // not a valid hex string
                if (!HexValue.TryConvert(strValue, out var hexValue))
                {
                    typeHandler.Violations.RegisterViolation(new UnableToConvertViolation("'" + strValue + "'", targetType.TypeName, src.Source));

                    return typeHandler.BinaryValueFactory.MakeUnknownValue(targetType.TypeName);
                }

                // FIXME : magic for unpredictable size
                if (targetType.Size < 0)
                {
                    // TODO : or unknown?
                    return typeHandler.BinaryValueFactory.MakePreciseValue(targetType.TypeName, hexValue, src.Source);
                }

                if (strictTargetSize && definedTargetSize < strValue.Length)
                {
                    // FIXME : how to distinct implicit truncation from explicit one?
                    // in case of implicit a violation warning is needed.
                    typeHandler.Violations.RegisterViolation(new ImplicitTruncationViolation(definedTargetSize, strValue.Length, strValue, src.Source));

                    hexValue.AsString = hexValue.AsString.Substring(0, definedTargetSize);
                    resultSrc = new SqlValueSource(SqlValueSourceKind.Expression, src.Source.Node);
                }

                return typeHandler.BinaryValueFactory.MakePreciseValue(targetType.TypeName, hexValue, resultSrc);
            }

            // converting from another (var)binary value
            if (src is SqlBinaryTypeValue bin)
            {
                int definedTargetSize = targetType.Size == 0 ? 1 : targetType.Size;
                // In string representation each byte takes 2 symbols
                int maxTargetLength = 2 * definedTargetSize;

                if (!bin.IsPreciseValue)
                {
                    return typeHandler.BinaryValueFactory.MakeUnknownValue(targetType.TypeName);
                }

                var strValue = bin.Value.AsString;
                var srcByteLength = strValue.Length / 2;
                var resultSrc = bin.Source;
                var hexValue = new HexValue(bin.Value.AsNumber, srcByteLength);

                if (strictTargetSize && definedTargetSize < srcByteLength)
                {
                    // FIXME : how to distinct implicit truncation from explicit one?
                    // in case of implicit a violation warning is needed.
                    typeHandler.Violations.RegisterViolation(new ImplicitTruncationViolation(definedTargetSize, srcByteLength, bin.Value.ToString(), src.Source));

                    hexValue = new HexValue(strValue.Substring(0, maxTargetLength), definedTargetSize);
                    resultSrc = new SqlValueSource(SqlValueSourceKind.Expression, src.Source.Node);
                }
                else if (strictTargetSize && targetType.HasFixedLength && maxTargetLength > strValue.Length)
                {
                    // in this case zeroes must be not prepended to the beginning
                    // but appended to the end (just like spaces for fixed-length CHAR string)
                    hexValue.AsString = strValue.PadRight(maxTargetLength, '0');
                }

                return typeHandler.BinaryValueFactory.MakePreciseValue(targetType.TypeName, hexValue, src.Source);
            }

            // TODO : support date/time types
            return default;
        }

        // TODO : check for negative values
        private static int EvalNumberAsHexLength(int value)
        {
            if (value == int.MinValue || value == int.MaxValue)
            {
                // precomputed constant results
                return 8;
            }

            if (value >= 0 && value < 256)
            {
                // optimization for small numbers
                return 2;
            }

            int exp = (int)Math.Ceiling(Math.Log(Math.Abs(value), 16));
            if (exp % 2 > 0)
            {
                // In T-SQL (VAR)BINARY values always have even number of symbols
                exp++;
            }

            return exp;
        }

        private static int EvalNumberAsHexLength(BigInteger value)
        {
            if (value >= int.MinValue && value <= int.MaxValue)
            {
                // implementation for int has some optimization
                return EvalNumberAsHexLength((int)value);
            }

            int exp = (int)Math.Ceiling(BigInteger.Log(BigInteger.Abs(value), 16));
            if (exp % 2 > 0)
            {
                // In T-SQL (VAR)BINARY values always have even number of symbols
                exp++;
            }

            return exp;
        }
    }
}
