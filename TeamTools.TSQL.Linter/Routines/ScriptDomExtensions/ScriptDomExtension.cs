using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Text;

namespace TeamTools.TSQL.Linter.Routines
{
    internal static class ScriptDomExtension
    {
        private static readonly ICollection<TSqlTokenType> TokensNotNeedingSpaceAround = new List<TSqlTokenType>
        {
            TSqlTokenType.Comma,
            TSqlTokenType.LeftParenthesis,
            TSqlTokenType.RightParenthesis,
            TSqlTokenType.Plus,
            TSqlTokenType.Minus,
            TSqlTokenType.Star,
            TSqlTokenType.Divide,
            TSqlTokenType.Percent,
            TSqlTokenType.PercentSign,
            TSqlTokenType.EqualsSign,
            TSqlTokenType.Add,
            TSqlTokenType.AddEquals,
            TSqlTokenType.SubtractEquals,
            TSqlTokenType.ModEquals,
            TSqlTokenType.DivideEquals,
            TSqlTokenType.Dot,
        };

        private static readonly ICollection<TSqlTokenType> TokensToBeWipedOut = new List<TSqlTokenType>
        {
            TSqlTokenType.WhiteSpace,
            TSqlTokenType.SingleLineComment,
            TSqlTokenType.MultilineComment,
            TSqlTokenType.EndOfFile,
        };

        private static readonly char[] SquareBrackets = new char[] { '[', ']' };

        public static string GetFullName(this SchemaObjectName node)
        {
            string tableName = node.BaseIdentifier.Value;

            if (node.SchemaIdentifier is null
            && (tableName.Equals(TSqlDomainAttributes.TriggerSystemTables.Inserted, StringComparison.OrdinalIgnoreCase)
            || tableName.Equals(TSqlDomainAttributes.TriggerSystemTables.Deleted, StringComparison.OrdinalIgnoreCase)))
            {
                return tableName;
            }

            if (!tableName.StartsWith(TSqlDomainAttributes.TempTablePrefix))
            {
                tableName = string.Join(
                    TSqlDomainAttributes.NamePartSeparator,
                    node.SchemaIdentifier?.Value ?? TSqlDomainAttributes.DefaultSchemaName,
                    tableName);
            }

            return tableName;
        }

        public static QuerySpecification GetQuerySpecification(this QueryExpression node)
        {
            if (node is null)
            {
                return default;
            }

            if (node is QueryParenthesisExpression qp)
            {
                return GetQuerySpecification(qp.QueryExpression);
            }

            if (node is BinaryQueryExpression bin)
            {
                // first is enough
                return GetQuerySpecification(bin.FirstQueryExpression);
            }

            if (node is QuerySpecification spec)
            {
                return spec;
            }

            return default;
        }

        public static string GetFragmentText(this TSqlFragment node)
        {
            if (node is null)
            {
                return default;
            }

            if (node.FirstTokenIndex < 0)
            {
                // some parser bug
                return default;
            }

            var result = new StringBuilder();

            int start = node.FirstTokenIndex;
            int end = node.LastTokenIndex;
            for (int i = start; i <= end; i++)
            {
                result.Append(node.ScriptTokenStream[i].Text);
            }

            return result.ToString();
        }

        public static string GetFragmentText(this TSqlFragment node, IList<TSqlTokenType> untilDelim)
        {
            if (node is null)
            {
                return default;
            }

            if (node.FirstTokenIndex < 0)
            {
                // some parser bug
                return default;
            }

            var result = new StringBuilder();

            for (int i = node.FirstTokenIndex; i <= node.LastTokenIndex; i++)
            {
                if (untilDelim.Contains(node.ScriptTokenStream[i].TokenType))
                {
                    break;
                }

                result.Append(node.ScriptTokenStream[i].Text);
            }

            return result.ToString();
        }

        public static string GetFragmentCleanedText(this TSqlFragment node)
        {
            if (node is Literal literal)
            {
                // some optimization
                return literal.Value;
            }

            if (node is VariableReference varRef)
            {
                // some optimization
                return varRef.Name;
            }

            if (node is GlobalVariableExpression globRef)
            {
                // some optimization
                return globRef.Name;
            }

            var result = new StringBuilder();
            bool lastWasSpace = false;
            int n = node.LastTokenIndex;

            for (int i = node.FirstTokenIndex; i <= n; i++)
            {
                if (TokenDoesNotNeedSpaceAround(node.ScriptTokenStream[i].TokenType)
                || (i == n - 1)
                || TokenDoesNotNeedSpaceAround(node.ScriptTokenStream[i + 1].TokenType))
                {
                    lastWasSpace = true;
                }

                if (TokensToBeWipedOut.Contains(node.ScriptTokenStream[i].TokenType))
                {
                    if (lastWasSpace)
                    {
                        continue;
                    }

                    result.Append(' ');
                    lastWasSpace = true;
                }
                else
                {
                    string tokenText = node.ScriptTokenStream[i].Text;
                    if (node.ScriptTokenStream[i].TokenType == TSqlTokenType.QuotedIdentifier)
                    {
                        tokenText = tokenText.Trim(SquareBrackets);
                    }
                    else
                    {
                        tokenText = tokenText.Trim();
                    }

                    result.Append(tokenText);
                    lastWasSpace = false;
                }
            }

            return result.ToString();
        }

        public static bool IsMadeOfLiteral(this ScalarExpression node)
        {
            while (node is ParenthesisExpression pe)
            {
                node = pe.Expression;
            }

            if (node is UnaryExpression ue)
            {
                return IsMadeOfLiteral(ue.Expression);
            }

            if (node is BinaryExpression be)
            {
                return IsMadeOfLiteral(be.FirstExpression)
                    && IsMadeOfLiteral(be.SecondExpression);
            }

            return node is Literal;
        }

        // Attempt to fix sonar integration error when it says that there is no text in the line
        // e.g.
        // Start pointer [line=11, lineOffset=0] should be before end pointer [line=11, lineOffset=0]
        public static int GetLastReadableLine(this TSqlFragment node, int lastTokenIndex)
        {
            var givenToken = node.ScriptTokenStream[lastTokenIndex];
            int violationLine = givenToken.Line;

            if (givenToken.Line == node.ScriptTokenStream[node.LastTokenIndex].Line
            && node.ScriptTokenStream[node.FirstTokenIndex].Line < givenToken.Line
            && givenToken.TokenType.In(TSqlTokenType.EndOfFile, TSqlTokenType.WhiteSpace))
            {
                violationLine--;
            }

            return violationLine;
        }

        private static bool TokenDoesNotNeedSpaceAround(TSqlTokenType tokenType)
        => TokensNotNeedingSpaceAround.Contains(tokenType);
    }
}
