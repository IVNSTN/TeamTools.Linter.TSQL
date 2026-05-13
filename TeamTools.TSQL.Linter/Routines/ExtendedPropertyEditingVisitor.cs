using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    internal abstract class ExtendedPropertyEditingVisitor : TSqlFragmentVisitor
    {
        private static readonly HashSet<string> PropertyEditProcs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "sp_addextendedproperty",
            "sp_updateextendedproperty",
            "sp_dropextendedproperty",
        };

        protected ExtendedPropertyEditingVisitor(Action<TSqlFragment, string> callback)
        {
            Callback = callback ?? throw new ArgumentNullException(nameof(callback));
        }

        protected Action<TSqlFragment, string> Callback { get; }

        public override sealed void Visit(ExecuteSpecification node)
        {
            if (!(node.ExecutableEntity is ExecutableProcedureReference procRef))
            {
                // EXEC 'cmd'
                return;
            }

            var procName = procRef.ProcedureReference.ProcedureReference?.Name;

            if (procName is null)
            {
                // EXEC @var
                return;
            }

            if (procName.SchemaIdentifier != null
            && !string.Equals(procName.SchemaIdentifier.Value, TSqlDomainAttributes.SystemSchemaName))
            {
                // not a sys proc call
                return;
            }

            if (!IsPropertyEditingProc(procName.BaseIdentifier.Value))
            {
                // not a property editing proc
                return;
            }

            ValidatePropertyEditingProcArgs(procRef.Parameters, node);
        }

        // Only named EXEC parameter provisioning is supported.
        // There is a separate rule for preventing passing arguments by position.
        protected static int MatchArgs(
            IDictionary<string, string> argValueMap,
            IList<ExecuteParameter> procParams,
            Action<ExecuteParameter, string> onMismatch)
        {
            int argCollectionFlags = 0;

            for (int i = procParams.Count - 1; i >= 0; i--)
            {
                var param = procParams[i];
                if (param.Variable != null
                && param.ParameterValue is StringLiteral levelTypeValue
                && argValueMap.TryGetValue(param.Variable.Name, out var expectedArgValue))
                {
                    if (string.Equals(expectedArgValue, levelTypeValue.Value, StringComparison.OrdinalIgnoreCase))
                    {
                        if (param.Variable.Name.EndsWith("type", StringComparison.OrdinalIgnoreCase))
                        {
                            // property level type
                            argCollectionFlags += 10;
                        }
                        else
                        {
                            // level/object name value
                            argCollectionFlags += 1;
                        }
                    }
                    else
                    {
                        onMismatch?.Invoke(param, expectedArgValue);
                    }
                }
            }

            return argCollectionFlags;
        }

        protected virtual bool IsPropertyEditingProc(string procName)
        {
            return PropertyEditProcs.Contains(procName);
        }

        protected abstract void ValidatePropertyEditingProcArgs(IList<ExecuteParameter> procParams, TSqlFragment call);
    }
}
