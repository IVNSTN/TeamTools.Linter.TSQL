using System;
using System.Collections.Generic;
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
        private static readonly Dictionary<int, string> DateTimeFormats = new Dictionary<int, string>
        {
            { 0, "mon dd yyyy hh:miAM" },
            { 100, "mon dd yyyy hh:miAM" },
            { 1, "mm/dd/yy" },
            { 101, "mm/dd/yyyy" },
            { 2, "yy.mm.dd" },
            { 102, "yyyy.mm.dd" },
            { 3, "dd/mm/yy" },
            { 103, "dd/mm/yy" },
            { 4, "dd.mm.yy" },
            { 104, "dd.mm.yyyy" },
            { 5, "dd-mm-yy" },
            { 105, "dd-mm-yyyy" },
            { 6, "dd mon yy" },
            { 106, "dd mon yyyy" },
            { 7, "Mon dd, yy" },
            { 107, "Mon dd, yyyy" },
            { 8, "hh:mi:ss" },
            { 108, "hh:mi:ss" },
            { 9, "mon dd yy hh:mi:ss:mmmAM" },
            { 109, "mon dd yyyy hh:mi:ss:mmmAM" },
            { 10, "mm-dd-yy" },
            { 110, "mm-dd-yyyy" },
            { 11, "yy/mm/dd" },
            { 111, "yyyy/mm/dd" },
            { 12, "yymmdd" },
            { 112, "yyyymmdd" },
            { 13, "dd mon yyyy hh:mi:ss:mmm" },
            { 113, "dd mon yyyy hh:mi:ss:mmm" },
            { 14, "hh:mi:ss:mmm" },
            { 114, "hh:mi:ss:mmm" },
            { 20, "yyyy-mm-dd hh:mi:ss" },
            { 120, "yyyy-mm-dd hh:mi:ss" },
            { 21, "yyyy-mm-dd hh:mi:ss.mmm" },
            { 121, "yyyy-mm-dd hh:mi:ss.mmm" },
            { 25, "yyyy-mm-dd hh:mi:ss.mmm" },
            { 22, "mm/dd/yy hh:mi:ss AM" },
            { 23, "yyyy-mm-dd" },
            { 24, "hh:mi:ss" },
            { 126, "yyyy-mm-ddThh:mi:ss.mmm" },
            { 127, "yyyy-MM-ddThh:mm:ss.fffZ" },
            { 130, "dd mon yyyy hh:mi:ss:mmmAM" },
            { 131, "dd/mm/yyyy hh:mi:ss:mmmAM" },
        };

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
                        typeHandler.Violations.RegisterViolation(new ImplicitTruncationViolation(targetType.Size, maxLength, null, src.Source));
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
                    typeHandler.Violations.RegisterViolation(new ImplicitTruncationViolation(targetType.Size, strValue.Length, strValue, src.Source));
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
                        typeHandler.Violations.RegisterViolation(new ImplicitTruncationViolation(targetType.Size, maxLength, null, src.Source));
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
                    typeHandler.Violations.RegisterViolation(new ImplicitTruncationViolation(targetType.Size, strValue.Length, strValue, src.Source));
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
                            typeHandler.Violations.RegisterViolation(new ImplicitTruncationViolation(definedTargetSize, strSrc.EstimatedSize, null, src.Source));
                        }

                        targetSize = definedTargetSize;
                    }

                    if (!targetType.IsUnicode && !strSrc.IsNull
                    && strSrc.TypeReference is SqlStrTypeReference strRef && strRef.IsUnicode)
                    {
                        typeHandler.Violations.RegisterViolation(new NationalSymbolLossViolation(strSrc.TypeName, src.Source));
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
                    typeHandler.Violations.RegisterViolation(new ImplicitTruncationViolation(definedTargetSize, strValue.Length, strValue, src.Source));

                    strValue = strValue.Substring(0, definedTargetSize);
                    resultSrc = new SqlValueSource(SqlValueSourceKind.Expression, src.Source.Node);
                }

                return typeHandler.StrValueFactory.MakePreciseValue(targetType.TypeName, strValue, resultSrc);
            }

            // converting from datetime
            if (src is SqlDateTimeValue datetimeSrc)
            {
                // yyyy-MM-ddThh:mm:ss.fffZ
                const int maxDateTimeFormatLength = 24;
                const int maxDateFormatLength = 10;

                if (!datetimeSrc.IsPreciseValue)
                {
                    int approximateLength = maxDateTimeFormatLength;

                    if (strictTargetSize)
                    {
                        approximateLength = targetType.Size;
                    }
                    else if (datetimeSrc.EstimatedSize.High.TimeAttributes == TimeDetails.None)
                    {
                        approximateLength = maxDateFormatLength;
                    }

                    return typeHandler.StrValueFactory.MakeApproximateValue(targetType.TypeName, approximateLength, src.Source);
                }

                // TODO : respect specific style if provided to CONVERT
                // TODO : respect culture defined on server-side? or is current thread culture a better choice?
                string strValue = datetimeSrc.Value.ToString();
                // little crutch for dates without time before implementing CONVERT styles
                if (datetimeSrc.Value.TimeOfDay.Equals(TimeSpan.Zero))
                {
                    strValue = datetimeSrc.Value.Date.ToString("yyyy'-'MM'-'dd");
                }

                // FIXME : magic
                if (targetType.Size < 0)
                {
                    // TODO : or unknown?
                    return typeHandler.StrValueFactory.MakePreciseValue(targetType.TypeName, strValue, src.Source);
                }

                var resultSrc = src.Source;
                if (strictTargetSize && strValue.Length > targetType.Size)
                {
                    // TODO : implement CONVERT style support first
                    // typeHandler.Violations.RegisterViolation(new ImplicitTruncationViolation(targetType.Size, strValue.Length, strValue, src.Source));
                    strValue = strValue.Substring(0, targetType.Size);
                    resultSrc = new SqlValueSource(SqlValueSourceKind.Expression, src.Source.Node);
                }

                return typeHandler.StrValueFactory.MakePreciseValue(targetType.TypeName, strValue, resultSrc);
            }

            // converting from date
            if (src is SqlDateOnlyValue dateSrc)
            {
                // yyyy-MM-dd
                const int maxDateFormatLength = 10;

                if (!dateSrc.IsPreciseValue)
                {
                    int approximateLength = strictTargetSize ? targetType.Size : maxDateFormatLength;

                    return typeHandler.StrValueFactory.MakeApproximateValue(targetType.TypeName, approximateLength, src.Source);
                }

                // TODO : respect specific format if provided to CAST/CONVERT
                string strValue = dateSrc.Value.Date.ToString("yyyy'-'MM'-'dd");

                // FIXME : magic
                if (targetType.Size < 0)
                {
                    // TODO : or unknown?
                    return typeHandler.StrValueFactory.MakePreciseValue(targetType.TypeName, strValue, src.Source);
                }

                var resultSrc = src.Source;
                if (strictTargetSize && strValue.Length > targetType.Size)
                {
                    // TODO : implement CONVERT style support first
                    // typeHandler.Violations.RegisterViolation(new ImplicitTruncationViolation(targetType.Size, strValue.Length, strValue, src.Source));
                    strValue = strValue.Substring(0, targetType.Size);
                    resultSrc = new SqlValueSource(SqlValueSourceKind.Expression, src.Source.Node);
                }

                return typeHandler.StrValueFactory.MakePreciseValue(targetType.TypeName, strValue, resultSrc);
            }

            // converting from time
            if (src is SqlTimeOnlyValue timeSrc)
            {
                // 23:59:59.9970000
                const int maxTimeFormatLength = 16;

                if (!timeSrc.IsPreciseValue)
                {
                    int approximateLength = strictTargetSize ? targetType.Size : maxTimeFormatLength;
                    return typeHandler.StrValueFactory.MakeApproximateValue(targetType.TypeName, approximateLength, src.Source);
                }

                // TODO : respect specific format if provided to CONVERT
                string strValue = timeSrc.Value.ToString("hh':'mm':'ss'.'ffffff");

                // FIXME : magic
                if (targetType.Size < 0)
                {
                    // TODO : or unknown?
                    return typeHandler.StrValueFactory.MakePreciseValue(targetType.TypeName, strValue, src.Source);
                }

                var resultSrc = src.Source;
                if (strictTargetSize && strValue.Length > targetType.Size)
                {
                    // TODO : implement CONVERT style support first
                    // typeHandler.Violations.RegisterViolation(new ImplicitTruncationViolation(targetType.Size, strValue.Length, strValue, src.Source));
                    strValue = strValue.Substring(0, targetType.Size);
                    resultSrc = new SqlValueSource(SqlValueSourceKind.Expression, src.Source.Node);
                }

                return typeHandler.StrValueFactory.MakePreciseValue(targetType.TypeName, strValue, resultSrc);
            }

            // TODO : see also binary conversion styles
            // https://learn.microsoft.com/en-us/sql/t-sql/functions/cast-and-convert-transact-sql?view=sql-server-ver17&f1url=%3FappId%3DDev15IDEF1%26l%3DEN-US%26k%3Dk(convert_TSQL)%3Bk(sql13.swb.tsqlresults.f1)%3Bk(sql13.swb.tsqlquery.f1)%3Bk(MiscellaneousFilesProject)%3Bk(DevLang-TSQL)%26rd%3Dtrue#binary-styles
            // converting from varbinary
            if (src is SqlBinaryTypeValue binarySrc)
            {
                if (!binarySrc.IsPreciseValue)
                {
                    // Each byte from source is represented by two symbols in a string
                    int targetSize = binarySrc.EstimatedSize * 2;
                    if (strictTargetSize)
                    {
                        if (targetType.HasFixedLength)
                        {
                            targetSize = targetType.Size;
                        }
                        else if (targetType.Size < targetSize)
                        {
                            targetSize = targetType.Size;
                        }
                    }

                    return typeHandler.StrValueFactory.MakeApproximateValue(targetType.TypeName, targetSize, src.Source);
                }

                // TODO : if style is 1 then 0x must be prepended - use Value.ToString() in such case
                // TODO : if no style provided then binary data should not be formatted as Hex and should be used as array of ASCII char codes
                var strValue = binarySrc.Value.AsString;
                var resultSrc = src.Source;

                if (strictTargetSize && strValue.Length > targetType.Size)
                {
                    // TODO : implement CONVERT style support first
                    typeHandler.Violations.RegisterViolation(new ImplicitTruncationViolation(targetType.Size, strValue.Length, strValue, src.Source));
                    strValue = strValue.Substring(0, targetType.Size);
                    resultSrc = new SqlValueSource(SqlValueSourceKind.Expression, src.Source.Node);
                }

                return typeHandler.StrValueFactory.MakePreciseValue(targetType.TypeName, strValue, resultSrc);
            }

            // converting from decimal
            if (src is SqlDecimalTypeValue decSrc)
            {
                // TODO : strictTargetSize + HasFixedLength => targetType.Size
                if (!decSrc.IsPreciseValue)
                {
                    // +1 - the decimal part separator
                    int targetSize = decSrc.EstimatedSize.Precision + (decSrc.EstimatedSize.Scale > 0 ? 1 : 0);
                    if (strictTargetSize)
                    {
                        if (targetType.HasFixedLength)
                        {
                            targetSize = targetType.Size;
                        }
                        else if (targetType.Size < targetSize)
                        {
                            targetSize = targetType.Size;
                        }
                    }

                    return typeHandler.StrValueFactory.MakeApproximateValue(targetType.TypeName, targetSize, src.Source);
                }

                var strValue = decSrc.Value.ToString();
                var resultSrc = src.Source;

                if (strictTargetSize && strValue.Length > targetType.Size)
                {
                    typeHandler.Violations.RegisterViolation(new ImplicitTruncationViolation(targetType.Size, strValue.Length, strValue, src.Source));
                    strValue = strValue.Substring(0, targetType.Size);
                    resultSrc = new SqlValueSource(SqlValueSourceKind.Expression, src.Source.Node);
                }

                return typeHandler.StrValueFactory.MakePreciseValue(targetType.TypeName, strValue, resultSrc);
            }

            return default;
        }
    }
}
