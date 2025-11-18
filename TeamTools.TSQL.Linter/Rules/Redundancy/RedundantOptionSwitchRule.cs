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
        private static readonly Dictionary<SetOptions, string> AllOptions;

        static RedundantOptionSwitchRule()
        {
            AllOptions = Enum.GetValues(typeof(SetOptions))
                .Cast<SetOptions>()
                .Where(opt => opt != SetOptions.None)
                .ToDictionary(opt => opt, opt => opt.ToString().ToUpperInvariant());
        }

        public RedundantOptionSwitchRule() : base()
        {
        }

        // analyzing options per batch
        protected override void ValidateBatch(TSqlBatch node) => node.Accept(new OptionVisitor(ViolationHandlerWithMessage));

        private class OptionVisitor : TSqlFragmentVisitor
        {
            private readonly Dictionary<SetOptions, bool> setOptionInstances = new Dictionary<SetOptions, bool>();
            private readonly Action<TSqlFragment, string> callback;

            public OptionVisitor(Action<TSqlFragment, string> callback)
            {
                this.callback = callback;
            }

            public override void Visit(PredicateSetStatement node)
            {
                foreach (var option in AllOptions.Keys)
                {
                    if ((node.Options & option) == option)
                    {
                        if (!setOptionInstances.TryGetValue(option, out var optionState))
                        {
                            setOptionInstances.Add(option, node.IsOn);
                        }
                        else if (optionState != node.IsOn)
                        {
                            // TODO : check there were statements between option switches
                            setOptionInstances[option] = node.IsOn;
                        }
                        else
                        {
                            // option was already switched into the same state
                            callback(node, AllOptions[option]);
                        }
                    }
                }
            }
        }
    }
}
