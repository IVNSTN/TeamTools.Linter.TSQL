using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("MA0148", "UNIQUE_ERR_NO_OR_STATE")]

    internal class UniqueErrorNumberOrStateRule : AbstractRule
    {
        public UniqueErrorNumberOrStateRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch node)
            => node.AcceptChildren(new ErrorVisitor(ViolationHandler));

        private class ErrorVisitor : VisitorWithCallback
        {
            public const int DefaultUserDefinedErrorNumber = 50000;
            private static readonly int MaxInfoSeverity = 10;
            private static readonly HashSet<string> StringTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "CHAR",
                "NCHAR",
                "NVARCHAR",
                "VARCHAR",
            };

            private Dictionary<string, string> declaredVariableTypes;
            private List<ErrInfo> errNumAndStates;

            public ErrorVisitor(Action<TSqlFragment> callback) : base(callback)
            { }

            public List<ErrInfo> ErrNumAndStates => errNumAndStates ?? (errNumAndStates = new List<ErrInfo>());

            public override void Visit(DeclareVariableElement node)
            {
                if (node.DataType?.Name is null)
                {
                    // e.g. CURSOR
                    return;
                }

                if (declaredVariableTypes != null && declaredVariableTypes.ContainsKey(node.VariableName.Value))
                {
                    return;
                }

                string typeName = node.DataType.Name.BaseIdentifier.Value;
                if (StringTypes.Contains(typeName))
                {
                    // TODO : what is this replacement for?
                    typeName = TSqlDomainAttributes.Types.Varchar;
                }

                (declaredVariableTypes ?? (declaredVariableTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase))).Add(node.VariableName.Value, typeName);
            }

            public override void Visit(RaiseErrorStatement node)
            {
                if (!ParseErrorNumberAndState(node.FirstParameter, node.ThirdParameter, out var err))
                {
                    return;
                }

                if ((node.SecondParameter is IntegerLiteral severity)
                && int.Parse(severity.Value) <= MaxInfoSeverity)
                {
                    // ignoring info messages
                    return;
                }

                if (!ErrNumAndStates.TryAddUnique(err))
                {
                    Callback(node);
                }
            }

            public override void Visit(ThrowStatement node)
            {
                if (!ParseErrorNumberAndState(node.ErrorNumber, node.State, out var err))
                {
                    return;
                }

                if (!ErrNumAndStates.TryAddUnique(err))
                {
                    Callback(node);
                }
            }

            protected bool ParseErrorNumberAndState(ScalarExpression errNum, ScalarExpression errState, out ErrInfo errInfo)
            {
                int errNumberValue;
                int errStateValue;
                string srcValue;
                errInfo = default;

                // we can say nothing about variable values
                if (!(errState is IntegerLiteral i))
                {
                    return false;
                }

                srcValue = i.Value;
                errStateValue = int.Parse(srcValue);

                // however if first argument is a string then the error number is 50000
                if (errNum is VariableReference varRef)
                {
                    string varName = varRef.Name;
                    if (declaredVariableTypes != null
                    && declaredVariableTypes.TryGetValue(varName, out string varType)
                    && varType.EndsWith("CHAR"))
                    {
                        // first parameter is a message
                        errNumberValue = DefaultUserDefinedErrorNumber;
                    }
                    else
                    {
                        // if int variable then we don't know which error number is that
                        return false;
                    }
                }
                else if (errNum is IntegerLiteral intValue)
                {
                    if (!int.TryParse(intValue.Value, out errNumberValue))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }

                errInfo = new ErrInfo(errNumberValue, errStateValue);

                return true;
            }
        }

        private sealed class ErrInfo : IEquatable<ErrInfo>
        {
            public ErrInfo() { }

            public ErrInfo(int number, int state)
            {
                ErrorNumber = number;
                ErrorState = state;
            }

            public int ErrorNumber { get; set; }

            public int ErrorState { get; set; }

            public bool Equals(ErrInfo other)
            {
                return other.ErrorNumber == this.ErrorNumber
                    && other.ErrorState == this.ErrorState;
            }
        }
    }
}
