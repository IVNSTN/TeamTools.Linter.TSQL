using System.Diagnostics.CodeAnalysis;

namespace TeamTools.TSQL.ExpressionEvaluator.Routines
{
    // TODO : this is a copy of NetStandardExtensions from TSQL linter library - get rid of duplication
    [ExcludeFromCodeCoverage]
    public static class NetStandardExtensions
    {
        static NetStandardExtensions()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        }

        public static bool IsAscii(this string value)
        {
            foreach (var c in value)
            {
#if NETSTANDARD
                if (c > byte.MaxValue)
#else
                if (!char.IsAscii(c))
#endif
                {
                    return false;
                }
            }

            return true;
        }

        public static bool ContainsNationalCharacter(this string s, int codePage)
        {
            if (string.IsNullOrWhiteSpace(s) || s.IsAscii())
            {
                return false;
            }

            try
            {
                var e = System.Text.Encoding.GetEncoding(
                        codePage,
                        System.Text.EncoderExceptionFallback.ExceptionFallback,
                        System.Text.DecoderExceptionFallback.ExceptionFallback)
                    .GetBytes(s);

                return false;
            }
            catch (System.Text.EncoderFallbackException)
            {
                return true;
            }
        }
    }
}
