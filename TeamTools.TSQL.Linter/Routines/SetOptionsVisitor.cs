using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines
{
    internal class SetOptionsVisitor : TSqlFragmentVisitor
    {
        private readonly IDictionary<string, IList<bool>> detectedOptions = new SortedDictionary<string, IList<bool>>(StringComparer.OrdinalIgnoreCase);

        public IDictionary<string, IList<bool>> DetectedOptions => detectedOptions;

        public override void Visit(PredicateSetStatement node)
        {
            foreach (SetOptions i in Enum.GetValues(typeof(SetOptions)))
            {
                if (i == SetOptions.None)
                {
                    continue;
                }

                if (node.Options.HasFlag(i))
                {
                    // TODO : it should also record token index and the node itself
                    DetectedOptions.TryAdd(i.ToString(), new List<bool>());
                    DetectedOptions[i.ToString()].Add(node.IsOn);
                }
            }
        }
    }
}
