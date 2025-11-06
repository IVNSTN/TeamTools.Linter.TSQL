using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0905", "VAR_LACKS_PRECISION")]
    internal sealed class VariableTypePrecisionLackRule : AbstractRule
    {
        // TODO : char, varchar out of size LEFT, RIGHT, SUBSTRING
        // relates ImplicitTruncationRule
        public VariableTypePrecisionLackRule() : base()
        {
        }

        public override void Visit(TSqlBatch node)
        {
            var visitor = new DateTimeVariableDeclarationVisitor();
            node.Accept(visitor);

            foreach (var v in visitor.Violations)
            {
                HandleNodeError(v.Key, v.Value);
            }
        }

        private abstract class VariableDeclarationVisitor : TSqlFragmentVisitor
        {
            private readonly IDictionary<string, string> variables = new SortedDictionary<string, string>();
            private readonly IList<KeyValuePair<TSqlFragment, string>> violations = new List<KeyValuePair<TSqlFragment, string>>();

            public IList<KeyValuePair<TSqlFragment, string>> Violations => violations;

            protected IDictionary<string, string> Variables => variables;

            public override void Visit(DeclareVariableElement node) => RegisterVariableDeclaration(node);

            public override void Visit(FunctionCall node) => RegisterVariableUsage(node);

            public override void Visit(ScalarExpression node) => RegisterVariableUsage(node);

            protected abstract void RegisterVariableDeclaration(DeclareVariableElement node);

            protected abstract void RegisterVariableUsage(FunctionCall node);

            protected abstract void RegisterVariableUsage(ScalarExpression node);
        }

        private class DateTimeVariableDeclarationVisitor : VariableDeclarationVisitor
        {
            private const string ViolationTextTemplate = "Argument {0} of type {1} is not good enough for {2}";

            private static readonly Lazy<IDictionary<string, DateTimeDetalizationLevel>> HandledTypesInstance
                = new Lazy<IDictionary<string, DateTimeDetalizationLevel>>(() => InitHandledTypesInstance(), true);

            private static readonly Lazy<IDictionary<string, DateTimeDetalizationLevel>> FuncArgumentDetalizationNeededInstance
                = new Lazy<IDictionary<string, DateTimeDetalizationLevel>>(() => InitFuncArgumentDetalizationNeededInstance(), true);

            private static readonly Lazy<IDictionary<string, bool>> DateFuncsInstance
                = new Lazy<IDictionary<string, bool>>(() => InitDateFuncsInstance(), true);

            public DateTimeVariableDeclarationVisitor()
            {
                // TODO : consolidate all the metadata about known built-in functions
                // treating some known function as "variables" of known type
                Variables.Add("GETDATE", "DATETIME");
                Variables.Add("GETUTCDATE", "DATETIME");
                Variables.Add("CURRENT_TIMESTAMP", "DATETIME");
                Variables.Add("SYSDATETIME", "DATETIME2");
                Variables.Add("SYSUTCDATETIME", "DATETIME2");
            }

            [Flags]
            public enum DateTimeDetalizationLevel
            {
                Date = 1,
                TimeHHMM = 2,
                TimeMilliseconds = 4,
                TimeNanoseconds = 8,
                DateAndTime = Date | TimeHHMM | TimeMilliseconds,
                DateAndTimeNs = DateAndTime | TimeNanoseconds,
            }

            private static IDictionary<string, DateTimeDetalizationLevel> HandledTypes => HandledTypesInstance.Value;

            private static IDictionary<string, DateTimeDetalizationLevel> FuncArgumentDetalizationNeeded => FuncArgumentDetalizationNeededInstance.Value;

            private static IDictionary<string, bool> DateFuncs => DateFuncsInstance.Value;

            protected override void RegisterVariableDeclaration(DeclareVariableElement node)
            {
                if (node.DataType?.Name == null)
                {
                    // CURSOR and such
                    return;
                }

                string typeName = node.DataType.Name.BaseIdentifier.Value;

                if (!HandledTypes.ContainsKey(typeName))
                {
                    return;
                }

                Variables.TryAdd(node.VariableName.Value, typeName);
            }

            protected void ValidateVariableUsage(TSqlFragment node, string funcName, string argName, string varName)
            {
                if (string.IsNullOrEmpty(argName) || string.IsNullOrEmpty(varName))
                {
                    return;
                }

                if (!FuncArgumentDetalizationNeeded.ContainsKey(argName))
                {
                    return;
                }

                if (!Variables.ContainsKey(varName) || !HandledTypes.ContainsKey(Variables[varName]))
                {
                    return;
                }

                if (HandledTypes[Variables[varName]].HasFlag(FuncArgumentDetalizationNeeded[argName]))
                {
                    return;
                }

                Violations.Add(new KeyValuePair<TSqlFragment, string>(
                    node,
                    string.Format(ViolationTextTemplate, varName, Variables[varName], argName)));
            }

            protected override void RegisterVariableUsage(FunctionCall node)
            {
                if (node.Parameters.Count == 0)
                {
                    return;
                }

                if (!DateFuncs.ContainsKey(node.FunctionName.Value))
                {
                    return;
                }

                DoValidateFunctionCallArguments(node);
            }

            protected void DoValidateFunctionCallArguments(FunctionCall node)
            {
                string funcName = node.FunctionName.Value;

                string argName = null;
                int maxParamIndex = 0;

                // if first arg contains type detalization info
                if (DateFuncs[funcName])
                {
                    if (node.Parameters.Count > 0 && node.Parameters[0] is ColumnReferenceExpression colRef
                        && colRef.MultiPartIdentifier?.Identifiers.Count == 1)
                    {
                        argName = colRef.MultiPartIdentifier.Identifiers[0].Value;
                    }

                    maxParamIndex = node.Parameters.Count > 2 ? 2 : node.Parameters.Count - 1;
                }
                else
                {
                    argName = node.FunctionName.Value;
                }

                for (int i = maxParamIndex; i >= 0; i--)
                {
                    ValidateFuncParameter(node, funcName, argName, i);
                }
            }

            protected void ValidateFuncParameter(FunctionCall node, string funcName, string argName, int paramIndex)
            {
                Debug.Assert(paramIndex >= 0 && node.Parameters.Count > paramIndex, "bad param index");

                if (node.Parameters[paramIndex] is VariableReference varRef)
                {
                    ValidateVariableUsage(node, funcName, argName, varRef.Name);
                }
                else if (node.Parameters[paramIndex] is FunctionCall funcRef && funcRef.Parameters.Count == 0)
                {
                    ValidateVariableUsage(node, funcName, argName, funcRef.FunctionName.Value);
                }
            }

            protected override void RegisterVariableUsage(ScalarExpression node)
            {
                // TODO : tbd
            }

            private static IDictionary<string, DateTimeDetalizationLevel> InitHandledTypesInstance()
            {
                return new SortedDictionary<string, DateTimeDetalizationLevel>(StringComparer.OrdinalIgnoreCase)
                {
                    { "DATE", DateTimeDetalizationLevel.Date },
                    { "TIME", DateTimeDetalizationLevel.TimeMilliseconds | DateTimeDetalizationLevel.TimeHHMM },
                    { "SMALLDATETIME", DateTimeDetalizationLevel.Date | DateTimeDetalizationLevel.TimeHHMM },
                    { "DATETIME", DateTimeDetalizationLevel.DateAndTime },
                    { "DATETIME2", DateTimeDetalizationLevel.DateAndTimeNs },
                };
            }

            private static IDictionary<string, DateTimeDetalizationLevel> InitFuncArgumentDetalizationNeededInstance()
            {
                return new SortedDictionary<string, DateTimeDetalizationLevel>(StringComparer.OrdinalIgnoreCase)
                {
                    // time detalization
                    { "HH", DateTimeDetalizationLevel.TimeHHMM },
                    { "HOUR", DateTimeDetalizationLevel.TimeHHMM },
                    { "N", DateTimeDetalizationLevel.TimeHHMM },
                    { "MI", DateTimeDetalizationLevel.TimeHHMM },
                    { "MINUTE", DateTimeDetalizationLevel.TimeHHMM },
                    { "S", DateTimeDetalizationLevel.TimeMilliseconds },
                    { "SS", DateTimeDetalizationLevel.TimeMilliseconds },
                    { "SECONDS", DateTimeDetalizationLevel.TimeMilliseconds },
                    { "MS", DateTimeDetalizationLevel.TimeMilliseconds },
                    { "MILLISECONDS", DateTimeDetalizationLevel.TimeMilliseconds },
                    { "MCS", DateTimeDetalizationLevel.TimeNanoseconds },
                    { "MICROSECONDS", DateTimeDetalizationLevel.TimeNanoseconds },
                    { "NS", DateTimeDetalizationLevel.TimeNanoseconds },
                    { "NANOSECONDS", DateTimeDetalizationLevel.TimeNanoseconds },
                    // date detalization
                    { "Y", DateTimeDetalizationLevel.Date },
                    { "YY", DateTimeDetalizationLevel.Date },
                    { "YYYY", DateTimeDetalizationLevel.Date },
                    { "YEAR", DateTimeDetalizationLevel.Date },
                    { "Q", DateTimeDetalizationLevel.Date },
                    { "QQ", DateTimeDetalizationLevel.Date },
                    { "QUARTER", DateTimeDetalizationLevel.Date },
                    { "M", DateTimeDetalizationLevel.Date },
                    { "MM", DateTimeDetalizationLevel.Date },
                    { "MONTH", DateTimeDetalizationLevel.Date },
                    { "WK", DateTimeDetalizationLevel.Date },
                    { "WW", DateTimeDetalizationLevel.Date },
                    { "WEEK", DateTimeDetalizationLevel.Date },
                    { "DY", DateTimeDetalizationLevel.Date },
                    { "DAYOFYEAR", DateTimeDetalizationLevel.Date },
                    { "DW", DateTimeDetalizationLevel.Date },
                    { "W", DateTimeDetalizationLevel.Date },
                    { "WEEKDAY", DateTimeDetalizationLevel.Date },
                    { "ISO_WEEK", DateTimeDetalizationLevel.Date },
                    // day, year and month are already here, so mixing with func names looks fine since
                    // they are unique enough and totally compatible with the dictoinary purpose
                    { "EOMONTH", DateTimeDetalizationLevel.Date },
                };
            }

            private static IDictionary<string, bool> InitDateFuncsInstance()
            {
                // TODO : consolidate all the metadata about known built-in functions
                // true - first arg means datetime part, false - first arg is variable
                return new SortedDictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
                {
                    { "DATEADD", true },
                    { "DATEDIFF", true },
                    { "DATEPART", true },
                    { "DATENAME", true },
                    { "DATE_BUCKET", true },
                    { "DATEDIFF_BIG", true },
                    { "DATETRUNC", true },
                    { "DAY", false },
                    { "MONTH", false },
                    { "YEAR", false },
                    { "EOMONTH", false },
                };
            }
        }
    }
}
