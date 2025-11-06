using System.Collections.Generic;
using System.Linq;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.Core
{
    public static class SqlVariableRegistryExtensions
    {
        public static void ResetValuesToUnknown(this SqlVariableRegistry reg, int fromTokenIndex, int tillTokenIndex, SqlValueSource src)
        {
            var varsEstimatedInBlock = reg.FindAssignmentsFromWithinBlock(fromTokenIndex, tillTokenIndex);

            foreach (var varName in varsEstimatedInBlock.Keys)
            {
                var beforeBlock = reg.GetValueAt(varName, fromTokenIndex - 1);
                SqlValue afterBlock = MergeAllValues(beforeBlock, varsEstimatedInBlock[varName]);

                if (afterBlock != null)
                {
                    reg.RegisterEvaluatedValue(varName, tillTokenIndex, afterBlock);
                }
                else
                {
                    reg.RegisterEvaluatedValue(varName, tillTokenIndex, SqlValueKind.Unknown, src);
                }
            }
        }

        public static void RevertValuesToPrior(this SqlVariableRegistry reg, int fromTokenIndex, int tillTokenIndex)
        {
            var varsEstimatedInBlock = reg.FindVariablesChangedInBlock(fromTokenIndex, tillTokenIndex);

            foreach (var varName in varsEstimatedInBlock)
            {
                var beforeBlock = reg.GetValueAt(varName, fromTokenIndex - 1);
                if (beforeBlock != null)
                {
                    reg.RegisterEvaluatedValue(varName, tillTokenIndex, beforeBlock);
                }
                else
                {
                    reg.RegisterEvaluatedValue(varName, tillTokenIndex, SqlValueKind.Unknown, default);
                }
            }
        }

        private static IEnumerable<string> FindVariablesChangedInBlock(this SqlVariableRegistry reg, int fromTokenIndex, int tillTokenIndex)
        {
            // TODO : optimize this search
            return reg.Variables.Keys
             .Where(v => reg.Values.ContainsKey(v))
             .Where(v => reg.Values[v]
                 .Any(ev => ev.StartingFromTokenIndex >= fromTokenIndex
                     && ev.StartingFromTokenIndex <= tillTokenIndex));
        }

        private static IDictionary<string, List<SqlValue>> FindAssignmentsFromWithinBlock(this SqlVariableRegistry reg, int fromTokenIndex, int tillTokenIndex)
        {
            // TODO : optimize this search
            return reg.Values
                .ToDictionary(
                    v => v.Key,
                    v => v.Value
                        .Where(ev => ev.StartingFromTokenIndex >= fromTokenIndex
                            && ev.StartingFromTokenIndex <= tillTokenIndex)
                        .Select(ev => ev.EvaluatedValue)
                        .ToList())
                 .Where(v => v.Value.Count > 0)
                 .ToDictionary(v => v.Key, v => v.Value);
        }

        private static SqlValue MergeAllValues(SqlValue valueBeforeBlock, IEnumerable<SqlValue> valuesInBlock)
        {
            // TODO : optimize this merge loop:
            // all approximate values may be the same - some kind of distinct would help
            var afterBlock = valueBeforeBlock ?? valuesInBlock.FirstOrDefault();
            var typeHandler = afterBlock.GetTypeHandler();

            foreach (var v in valuesInBlock)
            {
                afterBlock = typeHandler.MergeTwoEstimates(afterBlock, v);
            }

            return afterBlock;
        }
    }
}
