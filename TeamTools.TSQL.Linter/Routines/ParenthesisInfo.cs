namespace TeamTools.TSQL.Linter.Routines
{
    internal sealed class ParenthesisInfo
    {
        public ParenthesisInfo(int openTokenIndex, int parentTokenIndex)
        {
            this.OpenTokenIndex = openTokenIndex;
            this.ParentTokenIndex = parentTokenIndex;
        }

        public int OpenTokenIndex { get; set; } = -1;

        public int CloseTokenIndex { get; set; } = -1;

        public int ParentTokenIndex { get; set; } = -1;

        public int FirstMeaningfullTokenIndex { get; set; } = -1;

        public int LastMeaningfullTokenIndex { get; set; } = -1;

        public bool HasNestedParenthesis { get; set; } = false;

        public bool HasMeaningfullToken => FirstMeaningfullTokenIndex >= 0;
    }
}
