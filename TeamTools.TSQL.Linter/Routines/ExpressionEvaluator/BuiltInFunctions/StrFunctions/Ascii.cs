using System.Text;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.StrFunctions
{
    public class Ascii : EncodeSymbolsAsNumbers
    {
        private static readonly string OutputType = "dbo.TINYINT";
        private static readonly string FuncName = "ASCII";
        private static readonly SqlIntValueRange AsciiCodeRange = new SqlIntValueRange(0, 255);

        public Ascii() : base(FuncName, OutputType, AsciiCodeRange)
        {
        }

        protected override SqlIntTypeValue ClarifySymbolCodeValue(SqlStrTypeValue str, SqlIntTypeValue res, EvaluationContext context)
        {
            var bt = Encoding.ASCII.GetBytes(str.Value.Substring(0, 1));
            return res.ChangeTo(bt[0], str.Source);
        }
    }
}
