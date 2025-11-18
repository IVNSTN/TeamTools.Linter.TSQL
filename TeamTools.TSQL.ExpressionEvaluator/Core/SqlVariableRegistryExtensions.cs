using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Core
{
    public static class SqlVariableRegistryExtensions
    {
        public static void ResetValuesToUnknown(this SqlVariableRegistry reg, int fromTokenIndex, int tillTokenIndex, SqlValueSource src)
        {
            var varsEstimatedInBlock = reg.FindAssignmentsFromWithinBlock(fromTokenIndex, tillTokenIndex);
            if (varsEstimatedInBlock is null || varsEstimatedInBlock.Count == 0)
            {
                return;
            }

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
            if (varsEstimatedInBlock is null || varsEstimatedInBlock.Count == 0)
            {
                return;
            }

            int n = varsEstimatedInBlock.Count;
            for (int i = 0; i < n; i++)
            {
                var varName = varsEstimatedInBlock[i];

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

        private static List<string> FindVariablesChangedInBlock(this SqlVariableRegistry reg, int fromTokenIndex, int tillTokenIndex)
        {
            if (reg.Values.Count == 0)
            {
                return default;
            }

            var filter = new Func<string, bool>(v => reg.Values.TryGetValue(v, out var varValues)
                    && varValues.Any(ev => ev.StartingFromTokenIndex >= fromTokenIndex && ev.StartingFromTokenIndex <= tillTokenIndex));

            // TODO : optimize this search
            return reg.Variables.Keys
                .Where(filter)
                .ToList();
        }

        private static Dictionary<string, List<SqlValue>> FindAssignmentsFromWithinBlock(this SqlVariableRegistry reg, int fromTokenIndex, int tillTokenIndex)
        {
            if (reg.Values.Count == 0)
            {
                return default;
            }

            var filter = new Func<EvaluatedValueAtPos, bool>(ev => ev.StartingFromTokenIndex >= fromTokenIndex
                && ev.StartingFromTokenIndex <= tillTokenIndex);

            // TODO : optimize this search
            return reg.Values
                .ToDictionary(
                    v => v.Key,
                    v => v.Value
                        .Where(filter)
                        .Select(ev => ev.EvaluatedValue))
                 .Where(v => v.Value.Any())
                 .ToDictionary(v => v.Key, v => v.Value.ToList());
        }

        private static SqlValue MergeAllValues(SqlValue valueBeforeBlock, List<SqlValue> valuesInBlock)
        {
            // TODO : optimize this merge loop:
            // all approximate values may be the same - some kind of distinct would help
            var afterBlock = valueBeforeBlock ?? (valuesInBlock?.Count > 0 ? valuesInBlock[0] : default);
            var typeHandler = afterBlock.GetTypeHandler();

            int n = valuesInBlock.Count;
            for (int i = 0; i < n; i++)
            {
                afterBlock = typeHandler.MergeTwoEstimates(afterBlock, valuesInBlock[i]);
            }

            return afterBlock;
        }
    }
}
