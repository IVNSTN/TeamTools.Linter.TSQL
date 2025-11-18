using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Validating space between words in INNER JOIN, ORDER BY etc.
    /// </summary>
    internal partial class SingleSpaceInTwoWordInstructionRule
    {
        private static readonly Dictionary<FetchOrientation, string> FetchSpelling = new Dictionary<FetchOrientation, string>
        {
            { FetchOrientation.Next, "FETCH NEXT" },
            { FetchOrientation.Prior, "FETCH PRIOR" },
            { FetchOrientation.First, "FETCH FIRST" },
            { FetchOrientation.Last, "FETCH LAST" },
            { FetchOrientation.Relative, "FETCH ABSOLUTE" },
            { FetchOrientation.Absolute, "FETCH RELATIVE" },
            { FetchOrientation.None, "FETCH NEXT" },
        };

        public override void Visit(QualifiedJoin node)
        {
            ValidateSpaceBetween(
                node,
                node.FirstTableReference.LastTokenIndex + 1,
                node.SecondTableReference.FirstTokenIndex - 1,
                JoinTypeToSpelling(node.QualifiedJoinType));
        }

        public override void Visit(UnqualifiedJoin node)
            => ValidateSpaceBetween(node, node.FirstTableReference.LastTokenIndex + 1, node.SecondTableReference.FirstTokenIndex - 1);

        public override void Visit(OrderByClause node)
            => ValidateSpaceBetween(node, node.FirstTokenIndex, node.OrderByElements[0].FirstTokenIndex - 1, "ORDER BY");

        public override void Visit(GroupByClause node)
            => ValidateSpaceBetween(node, node.FirstTokenIndex, node.GroupingSpecifications[0].FirstTokenIndex - 1, "GROUP BY");

        // PARTITION BY
        // ORDER BY is already handled by OrderByClause visitor
        public override void Visit(OverClause node)
        {
            if (node.Partitions.Count == 0)
            {
                return;
            }

            int lastToken = node.Partitions[0].FirstTokenIndex - 1;
            int firstToken = lastToken;

            while (firstToken > 0 && !node.ScriptTokenStream[firstToken].Text.Equals("PARTITION", StringComparison.OrdinalIgnoreCase))
            {
                firstToken--;
            }

            ValidateSpaceBetween(node, firstToken, lastToken, "PARTITION BY");
        }

        // FOR JSON PATH/AUTO
        public override void Visit(JsonForClause node)
        {
            int start = node.FirstTokenIndex;
            // TODO : there is a bug in ScriptDom. remove loop after bugfix
            while (start > 0 && node.ScriptTokenStream[start].TokenType != TSqlTokenType.For)
            {
                start--;
            }

            ValidateSpaceBetween(node, start, node.Options[0].FirstTokenIndex, "FOR JSON");
        }

        // FOR XML PATH/AUTO
        public override void Visit(XmlForClause node)
        {
            int start = node.FirstTokenIndex;
            // TODO : there is a bug in ScriptDom. remove loop after bugfix
            while (start > 0 && node.ScriptTokenStream[start].TokenType != TSqlTokenType.For)
            {
                start--;
            }

            ValidateSpaceBetween(node, start, node.Options[0].FirstTokenIndex, "FOR XML");
        }

        // IS NULL, IS NOT NULL
        public override void Visit(BooleanIsNullExpression node)
            => ValidateSpaceBetween(node, node.Expression.LastTokenIndex + 1, node.LastTokenIndex, IsNotNullToSpelling(node.IsNot));

        // FETCH NEXT/PRIOR/.. FROM
        public override void Visit(FetchCursorStatement node)
        {
            ValidateSpaceBetween(
                  node,
                  node.FirstTokenIndex,
                  node.Cursor.FirstTokenIndex - 1,
                  FetchTypeToSpelling(node.FetchType));
        }

        // WHERE CURRENT OF
        public override void Visit(WhereClause node)
        {
            if (node.Cursor is null)
            {
                return;
            }

            ValidateSpaceBetween(node.Cursor, node.FirstTokenIndex, node.Cursor.FirstTokenIndex - 1, "WHERE CURRENT OF");
        }

        // GROUPING SETS
        public override void Visit(GroupingSetsGroupingSpecification node)
        {
            int i = node.Sets[0].FirstTokenIndex;
            int foundOpenParenthesis = 0;

            // Fist () is for a specific grouping set and the second - outer () is for list of grouping sets
            while (i > 0 && foundOpenParenthesis < 2)
            {
                if (node.ScriptTokenStream[i].TokenType == TSqlTokenType.LeftParenthesis)
                {
                    foundOpenParenthesis++;
                }

                i--;
            }

            ValidateSpaceBetween(node, node.FirstTokenIndex, i - 1, "GROUPING SETS");
        }

        // NOT IN
        public override void Visit(InPredicate node)
        {
            if (!node.NotDefined)
            {
                // IN is a single word
                return;
            }

            int i = (node.Subquery ?? node.Values[0]).FirstTokenIndex;
            while (node.ScriptTokenStream[i].TokenType != TSqlTokenType.LeftParenthesis)
            {
                i--;
            }

            ValidateSpaceBetween(node, node.Expression.LastTokenIndex + 1, i - 1, "NOT IN");
        }

        private static string JoinTypeToSpelling(QualifiedJoinType join)
        {
            switch (join)
            {
                case QualifiedJoinType.Inner:
                    return "INNER JOIN";
                case QualifiedJoinType.FullOuter:
                    return "FULL JOIN";
                case QualifiedJoinType.LeftOuter:
                    return "LEFT JOIN";
                case QualifiedJoinType.RightOuter:
                    return "RIGHT JOIN";
                default:
                    return default;
            }
        }

        private static string FetchTypeToSpelling(FetchType fetchType) => FetchSpelling[fetchType?.Orientation ?? FetchOrientation.Next];

        private static string IsNotNullToSpelling(bool isNot) => isNot ? "IS NOT NULL" : "IS NULL";
    }
}
