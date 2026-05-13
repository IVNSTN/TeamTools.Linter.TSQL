using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Text;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    internal partial class ExtendedPropertyDupRule
    {
        private sealed class ExtendedPropertyDupDetector : ExtendedPropertyEditingVisitor
        {
            private static readonly string AddPropertyProc = "sp_addextendedproperty";

            // It is supposed to store all added properties across multiple sp_addextendedproperty calls
            // so it can catch dups.
            private readonly HashSet<string> addedProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            public ExtendedPropertyDupDetector(StringBuilder stringBuilder, Action<TSqlFragment, string> callback)
            : base(callback)
            {
                PropertyNameBuilder = stringBuilder ?? throw new ArgumentNullException(nameof(stringBuilder));
            }

            private StringBuilder PropertyNameBuilder { get; }

            protected override bool IsPropertyEditingProc(string procName)
            {
                return string.Equals(procName, AddPropertyProc, StringComparison.OrdinalIgnoreCase);
            }

            // Only named EXEC parameter provisioning is supported.
            // There is a separate rule for preventing passing arguments by position.
            protected override void ValidatePropertyEditingProcArgs(IList<ExecuteParameter> procParams, TSqlFragment call)
            {
                // all args except @value
                // a little reordering by ASC sorting is fine
                var argValueMap = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "@level0type", null },
                    { "@level0name", null },
                    { "@level1type", null },
                    { "@level1name", null },
                    { "@level2type", null },
                    { "@level2name", null },
                    { "@name", null },
                };

                for (int i = procParams.Count - 1; i >= 0; i--)
                {
                    var param = procParams[i];
                    if (param.Variable != null && argValueMap.ContainsKey(param.Variable.Name))
                    {
                        if (param.ParameterValue is StringLiteral s)
                        {
                            argValueMap[param.Variable.Name] = s.Value;
                        }
                    }
                }

                if (string.IsNullOrEmpty(argValueMap["@name"]))
                {
                    // property name not detected
                    return;
                }

                foreach (var arg in argValueMap)
                {
                    PropertyNameBuilder
                        .Append(arg.Key)
                        .Append('=')
                        .Append(arg.Value)
                        .Append(';');
                }

                string propertyFullPath = PropertyNameBuilder.ToString();
                PropertyNameBuilder.Length = 0; // cleanup before next proc call analysis

                if (!addedProperties.Add(propertyFullPath))
                {
                    Callback(call, argValueMap["@name"]);
                }
            }
        }
    }
}
