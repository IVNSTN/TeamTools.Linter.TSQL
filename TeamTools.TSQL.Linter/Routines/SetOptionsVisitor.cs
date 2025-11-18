using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TeamTools.TSQL.Linter.Routines
{
    internal class SetOptionsVisitor : TSqlConcreteFragmentVisitor
    {
        private static readonly SetOptions[] AllOptions;

        static SetOptionsVisitor()
        {
            AllOptions = Enum.GetValues(typeof(SetOptions))
                .OfType<SetOptions>()
                .Where(opt => opt != SetOptions.None)
                .ToArray();
        }

        // true - always ON, false - always OFF, null - both states detected
        public Dictionary<SetOptions, bool?> DetectedOptions { get; } = new Dictionary<SetOptions, bool?>();

        public void Reset() => DetectedOptions.Clear();

        public override void ExplicitVisit(PredicateSetStatement node)
        {
            foreach (var opt in AllOptions)
            {
                if (node.Options.HasFlag(opt))
                {
                    if (DetectedOptions.TryGetValue(opt, out var optState))
                    {
                        if (optState != null && optState != node.IsOn)
                        {
                            DetectedOptions[opt] = null;
                        }
                    }
                    else
                    {
                        DetectedOptions.Add(opt, node.IsOn);
                    }
                }
            }
        }
    }
}
