using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Violations
{
    public abstract class SqlViolation
    {
        protected SqlViolation(string messsage, SqlValueSource source)
        {
            Message = messsage;
            TokenIndex = source?.Node?.FirstTokenIndex ?? 0;
            Source = source;
        }

        public string Message { get; }

        public int TokenIndex { get; }

        public SqlValueSource Source { get; }
    }
}
