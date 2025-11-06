using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TeamTools.TSQL.Linter.Routines
{
    internal sealed class FunctionReferenceVisitor : TSqlFragmentVisitor
    {
        private static readonly char[] TrimmedCharsStart = new char[] { '$', '(' };
        private static readonly char[] TrimmedCharsEnd = new char[] { ')' };
        private readonly int lastTokenIndex;
        private string functionName = "";

        public FunctionReferenceVisitor(int lastTokenIndex)
        {
            this.lastTokenIndex = lastTokenIndex;
        }

        public string Name => functionName;

        public override void Visit(Identifier node)
        {
            if ((this.lastTokenIndex > 0) && (node.FirstTokenIndex >= this.lastTokenIndex))
            {
                return;
            }

            string namePart = CleanIdentifier(node.Value);

            if (string.IsNullOrEmpty(namePart))
            {
                return;
            }

            if (!string.IsNullOrEmpty(functionName))
            {
                functionName += TSqlDomainAttributes.NamePartSeparator;
            }

            functionName += namePart;
        }

        private static string CleanIdentifier(string name)
        {
            name = name.Replace("[", "").Replace("]", "");
            name = name.TrimStart(TrimmedCharsStart).TrimEnd(TrimmedCharsEnd);
            return name;
        }
    }
}
