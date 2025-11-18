using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Validating space between words in TRUNCATE TABLE and other DML instructions.
    /// </summary>
    internal partial class SingleSpaceInTwoWordInstructionRule
    {
        // TRUNCATE TABLE
        public override void Visit(TruncateTableStatement node)
            => ValidateSpaceBetween(node, node.FirstTokenIndex, node.TableName.FirstTokenIndex - 1, "TRUNCATE TABLE");

        // DELETE FROM
        public override void Visit(DeleteSpecification node)
        {
            if (node.TopRowFilter != null)
            {
                return;
            }

            ValidateSpaceBetween(node, node.FirstTokenIndex, node.Target.FirstTokenIndex - 1, "DELETE FROM");
        }

        // INSERT INTO
        public override void Visit(InsertSpecification node)
        {
            if (node.InsertOption != InsertOption.Into)
            {
                // no INTO
                return;
            }

            ValidateSpaceBetween(node, node.FirstTokenIndex, node.Target.FirstTokenIndex - 1, "INSERT INTO");
        }

        // MERGE clauses WHEN [NOT] MATCHED THEN
        public override void Visit(MergeActionClause node)
        {
            // TODO : There is a bug in ScriptDom: node.FirstTokenIndex is pointing either to the Action or SearchCondition
            // this locate-when-code is to be removed after bugfix.
            int lastIndex = ((TSqlFragment)node.SearchCondition ?? node.Action).FirstTokenIndex - 1;
            int firstIndex = ((TSqlFragment)node.SearchCondition ?? node).FirstTokenIndex;
            while (firstIndex > 0 && node.ScriptTokenStream[firstIndex].TokenType != TSqlTokenType.When)
            {
                firstIndex--;
            }

            ValidateSpaceBetween(node, firstIndex, lastIndex, GetMatchedSpelling(node.Condition == MergeCondition.Matched));
        }

        private static string GetMatchedSpelling(bool matched) => matched ? "WHEN MATCHED" : "WHEN NOT MATCHED";
    }
}
