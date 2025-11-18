using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Properties;
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

        protected override void ValidateBatch(TSqlBatch node) => node.Accept(new DateTimeVariableDeclarationVisitor(ViolationHandlerWithMessage));

        private abstract class VariableDeclarationVisitor : TSqlFragmentVisitor
        {
            protected Dictionary<string, string> Variables { get; } = new Dictionary<string, string>();

            public override void Visit(DeclareVariableElement node) => RegisterVariableDeclaration(node);

            public override void Visit(FunctionCall node) => RegisterVariableUsage(node);

            public override void Visit(ScalarExpression node) => RegisterVariableUsage(node);

            protected abstract void RegisterVariableDeclaration(DeclareVariableElement node);

            protected abstract void RegisterVariableUsage(FunctionCall node);

            protected abstract void RegisterVariableUsage(ScalarExpression node);
        }

        private class DateTimeVariableDeclarationVisitor : VariableDeclarationVisitor
        {
            private static readonly string ViolationTextTemplate = Strings.ViolationDetails_VariableTypePrecisionLackRule_TypeIsNoGood;

            private static readonly Lazy<Dictionary<string, DateTimeDetalizationLevel>> HandledTypesInstance
                = new Lazy<Dictionary<string, DateTimeDetalizationLevel>>(() => InitHandledTypesInstance(), true);

            private static readonly Lazy<Dictionary<string, DateTimeDetalizationLevel>> FuncArgumentDetalizationNeededInstance
                = new Lazy<Dictionary<string, DateTimeDetalizationLevel>>(() => InitFuncArgumentDetalizationNeededInstance(), true);

            private static readonly Lazy<Dictionary<string, bool>> DateFuncsInstance
                = new Lazy<Dictionary<string, bool>>(() => InitDateFuncsInstance(), true);

            // TODO : consolidate all the metadata about known built-in functions
            private static readonly Lazy<Dictionary<string, string>> DateTimeParameterlessFunctionsInstance
                = new Lazy<Dictionary<string, string>>(() => InitDateTimeParameterlessFunctionsInstance(), true);

            private readonly Action<TSqlFragment, string> callback;

            public DateTimeVariableDeclarationVisitor(Action<TSqlFragment, string> callback)
            {
                this.callback = callback;
            }

            [Flags]
            public enum DateTimeDetalizationLevel
            {
                None = 0,
                Date = 1,
                TimeHHMM = 2,
                TimeMilliseconds = 4,
                DateAndTime = Date | TimeHHMM | TimeMilliseconds,
                TimeNanoseconds = 8,
                DateAndTimeNs = DateAndTime | TimeNanoseconds,
            }

            private static Dictionary<string, DateTimeDetalizationLevel> HandledTypes => HandledTypesInstance.Value;

            private static Dictionary<string, DateTimeDetalizationLevel> FuncArgumentDetalizationNeeded => FuncArgumentDetalizationNeededInstance.Value;

            private static Dictionary<string, bool> DateFuncs => DateFuncsInstance.Value;

            private static Dictionary<string, string> DateTimeParameterlessFunctions => DateTimeParameterlessFunctionsInstance.Value;

            protected override void RegisterVariableDeclaration(DeclareVariableElement node)
            {
                if (node.DataType?.Name is null)
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

                if (!FuncArgumentDetalizationNeeded.TryGetValue(argName, out var detalization))
                {
                    return;
                }

                if (!(Variables.TryGetValue(varName, out string varType) || DateTimeParameterlessFunctions.TryGetValue(varName, out varType))
                || !HandledTypes.TryGetValue(varType, out var expectedDetails))
                {
                    return;
                }

                if (expectedDetails.HasFlag(detalization))
                {
                    return;
                }

                callback(node, string.Format(ViolationTextTemplate, varName, varType, argName));
            }

            protected override void RegisterVariableUsage(FunctionCall node)
            {
                if (node.Parameters.Count == 0)
                {
                    return;
                }

                DoValidateFunctionCallArguments(node);
            }

            protected void DoValidateFunctionCallArguments(FunctionCall node)
            {
                string funcName = node.FunctionName.Value;

                if (!DateFuncs.TryGetValue(funcName, out var takeDetailsFromForstArg))
                {
                    return;
                }

                string argName = null;
                int maxParamIndex = 0;

                // if first arg contains type detalization info
                if (takeDetailsFromForstArg)
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
                    argName = funcName;
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

            private static Dictionary<string, DateTimeDetalizationLevel> InitHandledTypesInstance()
            {
                return new Dictionary<string, DateTimeDetalizationLevel>(StringComparer.OrdinalIgnoreCase)
                {
                    { "DATE", DateTimeDetalizationLevel.Date },
                    { "DATETIME", DateTimeDetalizationLevel.DateAndTime },
                    { "DATETIME2", DateTimeDetalizationLevel.DateAndTimeNs },
                    { "SMALLDATETIME", DateTimeDetalizationLevel.Date | DateTimeDetalizationLevel.TimeHHMM },
                    { "TIME", DateTimeDetalizationLevel.TimeMilliseconds | DateTimeDetalizationLevel.TimeHHMM },
                };
            }

            private static Dictionary<string, DateTimeDetalizationLevel> InitFuncArgumentDetalizationNeededInstance()
            {
                return new Dictionary<string, DateTimeDetalizationLevel>(StringComparer.OrdinalIgnoreCase)
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

            private static Dictionary<string, bool> InitDateFuncsInstance()
            {
                // TODO : consolidate all the metadata about known built-in functions
                // true - first arg means datetime part, false - first arg is variable
                return new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
                {
                    { "DATEADD", true },
                    { "DATEDIFF", true },
                    { "DATEDIFF_BIG", true },
                    { "DATENAME", true },
                    { "DATEPART", true },
                    { "DATETRUNC", true },
                    { "DATE_BUCKET", true },
                    { "DAY", false },
                    { "EOMONTH", false },
                    { "MONTH", false },
                    { "YEAR", false },
                };
            }

            private static Dictionary<string, string> InitDateTimeParameterlessFunctionsInstance()
            {
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "CURRENT_TIMESTAMP", "DATETIME" },
                    { "GETDATE", "DATETIME" },
                    { "GETUTCDATE", "DATETIME" },
                    { "SYSDATETIME", "DATETIME2" },
                    { "SYSUTCDATETIME", "DATETIME2" },
                };
            }
        }
    }
}
