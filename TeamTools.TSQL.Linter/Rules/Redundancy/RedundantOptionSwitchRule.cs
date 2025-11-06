using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0156", "REDUNDANT_OPTION_SWITCH")]
    internal sealed class RedundantOptionSwitchRule : AbstractRule
    {
        private static readonly IEnumerable<SetOptions> AllOptions;

        static RedundantOptionSwitchRule()
        {
            AllOptions = Enum.GetValues(typeof(SetOptions))
                .Cast<SetOptions>()
                .Where(opt => opt != SetOptions.None)
                .ToList();
        }

        public RedundantOptionSwitchRule() : base()
        {
        }

        // analyzing options per batch
        public override void Visit(TSqlBatch node) => node.AcceptChildren(new OptionVisitor(HandleNodeError));

        private class OptionVisitor : TSqlFragmentVisitor
        {
            private readonly IDictionary<string, bool> setOptionInstances = new SortedDictionary<string, bool>();
            private readonly Action<TSqlFragment, string> callback;

            public OptionVisitor(Action<TSqlFragment, string> callback)
            {
                this.callback = callback;
            }

            public override void Visit(PredicateSetStatement node)
            {
                var setOptions = AllOptions
                    .Where(opt => (node.Options & opt) == opt)
                    .Select(opt => opt.ToString());

                foreach (string optionName in setOptions)
                {
                    if (!setOptionInstances.ContainsKey(optionName))
                    {
                        setOptionInstances.Add(optionName, node.IsOn);
                    }
                    else if (setOptionInstances[optionName] != node.IsOn)
                    {
                        // TODO : check there were statements between option switches
                        setOptionInstances[optionName] = node.IsOn;
                    }
                    else
                    {
                        callback(node, optionName);
                    }
                }
            }
        }
    }
}
