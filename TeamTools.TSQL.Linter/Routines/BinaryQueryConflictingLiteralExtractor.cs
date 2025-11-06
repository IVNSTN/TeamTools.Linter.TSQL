using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines
{
    internal static class BinaryQueryConflictingLiteralExtractor
    {
        public static TSqlFragment GetFirstDifferentLiteral(BinaryQueryExpression node)
        {
            var selectedElements = new List<ScalarExpression>();
            bool init;

            foreach (var spec in ExtractTopLevelQueries(node))
            {
                init = selectedElements.Count == 0;

                for (int i = 0; i < spec.SelectElements.Count; i++)
                {
                    var selectedValue = ExtractSelectedValue(spec.SelectElements[i]);

                    if (init)
                    {
                        selectedElements.Add(selectedValue);
                    }
                    else if (selectedElements.Count > i
                    && selectedElements[i] is Literal oldValue
                    && selectedValue is Literal newValue)
                    {
                        // exiting loop on first mismatch
                        if (!newValue.Value.Equals(oldValue.Value, StringComparison.OrdinalIgnoreCase))
                        {
                            return newValue;
                        }
                    }
                }
            }

            return null;
        }

        private static IEnumerable<QuerySpecification> ExtractTopLevelQueries(QueryExpression node)
        {
            if (node is BinaryQueryExpression bin)
            {
                foreach (var spec in ExtractTopLevelQueries(bin.FirstQueryExpression))
                {
                    yield return spec;
                }

                foreach (var spec in ExtractTopLevelQueries(bin.SecondQueryExpression))
                {
                    yield return spec;
                }
            }
            else if (node is QuerySpecification spec)
            {
                yield return spec;
            }
        }

        private static ScalarExpression ExtractSelectedValue(TSqlFragment node)
        {
            if (node is ParenthesisExpression p)
            {
                return ExtractSelectedValue(p.Expression);
            }
            else if (node is SelectScalarExpression sel)
            {
                return ExtractSelectedValue(sel.Expression);
            }
            else if (node is ScalarExpression s)
            {
                return s;
            }

            return null;
        }
    }
}
