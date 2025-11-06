namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
{
    public abstract class SqlViolation
    {
        public SqlViolation(string messsage, SqlValueSource source)
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
