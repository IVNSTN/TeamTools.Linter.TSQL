using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.ExpressionEvaluator.Routines
{
    // TODO : this is a copy of ObjectOptionsExtensions from TSQL linter library - get rid of duplication
    internal static class ScriptDomExtension
    {
        private static readonly HashSet<string> BuiltInUserDefinedTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "GEOGRAPHY",
            "GEOMETRY",
            "JSON",
            "SYSNAME",
            "XML",
        };

        public static bool IsNonStatementToken(TSqlTokenType token)
        {
            return token == TSqlTokenType.WhiteSpace
                || token == TSqlTokenType.SingleLineComment
                || token == TSqlTokenType.MultilineComment
                || token == TSqlTokenType.Semicolon;
        }

        public static bool IsSkippableTokens(TSqlTokenType token)
        {
            return token == TSqlTokenType.WhiteSpace
                || token == TSqlTokenType.SingleLineComment
                || token == TSqlTokenType.MultilineComment
                || token == TSqlTokenType.EndOfFile;
        }

        public static bool IsTokensNotNeedingSpaceAround(TSqlTokenType token)
        {
            return token == TSqlTokenType.Comma
                || token == TSqlTokenType.LeftParenthesis
                || token == TSqlTokenType.RightParenthesis
                || token == TSqlTokenType.Plus
                || token == TSqlTokenType.Minus
                || token == TSqlTokenType.Star
                || token == TSqlTokenType.Divide
                || token == TSqlTokenType.Percent
                || token == TSqlTokenType.PercentSign
                || token == TSqlTokenType.EqualsSign
                || token == TSqlTokenType.Add
                || token == TSqlTokenType.AddEquals
                || token == TSqlTokenType.SubtractEquals
                || token == TSqlTokenType.ModEquals
                || token == TSqlTokenType.DivideEquals
                || token == TSqlTokenType.Dot;
        }

        /// <summary>
        /// Returns full name for given SchemaObjectName.
        /// </summary>
        /// <param name="node">Parsed object name.</param>
        /// <returns>Result is prepended with "dbo." if schema was omitted.</returns>
        public static string GetFullName(this SchemaObjectName node)
        {
            string objectName = node.BaseIdentifier.Value;

            if (node.SchemaIdentifier is null
            && (objectName.Equals(TSqlDomainAttributes.TriggerSystemTables.Inserted, StringComparison.OrdinalIgnoreCase)
            || objectName.Equals(TSqlDomainAttributes.TriggerSystemTables.Deleted, StringComparison.OrdinalIgnoreCase)))
            {
                return objectName;
            }

            if (!objectName.StartsWith(TSqlDomainAttributes.TempTablePrefix))
            {
                string schemaName = node.SchemaIdentifier?.Value ?? TSqlDomainAttributes.DefaultSchemaName;
                objectName = $"{schemaName}{TSqlDomainAttributes.NamePartSeparator}{objectName}";
            }

            return objectName;
        }

        /// <summary>
        /// Returns full name for given data type reference for internal needs.
        /// </summary>
        /// <param name="node">Parsed type reference.</param>
        /// <returns>Type name without schema for built-in types and name with schema for others.
        /// Result is prepended with "dbo." schema name if it was omitted for UDT.
        /// Schema "sys" is ignored for build-in types.</returns>
        public static string GetFullName(this DataTypeReference node)
        {
            if (node.Name is null)
            {
                if (node is SqlDataTypeReference sqlType)
                {
                    switch (sqlType.SqlDataTypeOption)
                    {
                        case SqlDataTypeOption.Cursor:
                            return "CURSOR";
                    }
                }

                // no name
                return default;
            }

            string typeName = node.Name.BaseIdentifier.Value;

            if (node is SqlDataTypeReference || BuiltInUserDefinedTypes.Contains(typeName))
            {
                // Ignoring "sys" schema if it was provided
                if (node.Name.SchemaIdentifier is null
                || node.Name.SchemaIdentifier.Value.Equals(TSqlDomainAttributes.SystemSchemaName, StringComparison.OrdinalIgnoreCase))
                {
                    return typeName;
                }
            }

            return string.Join(
                TSqlDomainAttributes.NamePartSeparator,
                node.Name.SchemaIdentifier?.Value ?? TSqlDomainAttributes.DefaultSchemaName,
                typeName);
        }

        public static string GetFullName(this IList<Identifier> node, string delimiter = ".")
        {
            if (node.Count == 0)
            {
                return default;
            }

            if (node.Count == 1)
            {
                return node[0].Value;
            }

            var sb = ObjectPools.StringBuilderPool.Get();
            for (int i = 0, n = node.Count; i < n; i++)
            {
                if (i > 0)
                {
                    sb.Append(delimiter);
                }

                sb.Append(node[i].Value);
            }

            var result = sb.ToString();
            ObjectPools.StringBuilderPool.Return(sb);

            return result;
        }

        /// <summary>
        /// Returns full name for given table reference.
        /// </summary>
        /// <param name="node">Table reference.</param>
        /// <returns>Table name with schema. If table variable or temp table referenced then no schema will be added.
        /// For regular tables result is prepended with "dbo." schema name if it was omitted.</returns>
        public static string GetFullName(this TableReference node)
        {
            if (node is NamedTableReference tblName)
            {
                return tblName.SchemaObject.GetFullName();
            }
            else if (node is VariableTableReference varName)
            {
                return varName.Variable.Name;
            }

            return default;
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

            var sb = ObjectPools.StringBuilderPool.Get();
            bool lastWasSpace = false;
            int start = node.FirstTokenIndex;
            int end = node.LastTokenIndex + 1;

            for (int i = start; i < end; i++)
            {
                var token = node.ScriptTokenStream[i];
                if (IsTokensNotNeedingSpaceAround(token.TokenType)
                || (i == end - 1)
                || IsTokensNotNeedingSpaceAround(node.ScriptTokenStream[i + 1].TokenType))
                {
                    lastWasSpace = true;
                }

                if (IsSkippableTokens(token.TokenType))
                {
                    if (lastWasSpace)
                    {
                        continue;
                    }

                    // Not sure why, but this space started breaking some code (comparison fails)
                    // result.Append(' ');
                    lastWasSpace = true;
                }
                else
                {
                    if (token.TokenType == TSqlTokenType.QuotedIdentifier
                    || (token.TokenType == TSqlTokenType.AsciiStringOrQuotedIdentifier && token.Text[0] == '"'))
                    {
                        // removes [] and "" from quoted identifier
                        // and we don't care about possible inner [] or "":
                        // if identifier has these chars it will be quoted everywhere
                        // thus removing quotation will result equally for every identifier mention
                        sb.Append(token.Text, 1, token.Text.Length - 2);
                    }
                    else
                    {
                        sb.Append(token.Text);
                    }

                    lastWasSpace = false;
                }
            }

            var result = sb.ToString();
            ObjectPools.StringBuilderPool.Return(sb);
            return result;
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

        public static ScalarExpression ExtractScalarExpression(this ScalarExpression node)
        {
            while (node is ParenthesisExpression pe)
            {
                node = pe.Expression;
            }

            if (node is UnaryExpression ue)
            {
                var src = ue.Expression.ExtractScalarExpression();
                if (src is NullLiteral nullLiteral)
                {
                    return nullLiteral;
                }

                return ue;
            }

            if (node is ScalarSubquery sq)
            {
                var spec = sq.QueryExpression.GetQuerySpecification();
                if (spec != null && spec.WhereClause is null && spec.HavingClause is null)
                {
                    if (spec.SelectElements.Count == 1 && spec.SelectElements[0] is SelectScalarExpression sse)
                    {
                        return ExtractScalarExpression(sse.Expression);
                    }
                }
            }

            return node;
        }

        public static Identifier GetLastIdentifier(this MultiPartIdentifier node) => node.Identifiers[node.Identifiers.Count - 1];

        public static bool HasInMemoryFlag(this CreateTypeTableStatement node) => node.Options.HasOption(TableOptionKind.MemoryOptimized);

        public static bool HasInMemoryFlag(this CreateTableStatement node) => node.Options.HasOption(TableOptionKind.MemoryOptimized);
    }
}
