namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
{
    public enum SqlValueCompareResult
    {
        /// <summary>
        /// Result is Unknown / NULL
        /// </summary>
        Unknown,

        /// <summary>
        /// Both are equal
        /// </summary>
        Equal,

        /// <summary>
        /// First arg is less
        /// </summary>
        Less,

        /// <summary>
        /// First arg is greater
        /// </summary>
        Greater,
    }
}
