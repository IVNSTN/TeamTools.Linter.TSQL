using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0856", "SELF_CALL")]
    internal sealed class SelfCallRule : AbstractRule
    {
        public SelfCallRule() : base()
        {
        }

        public override void ExplicitVisit(CreateFunctionStatement node)
        {
            string funcName = node.Name.GetFullName();

            if (node.ReturnType is SelectFunctionReturnType sel)
            {
                // inline table function has no body
                DetectSelfCall(sel.SelectStatement, funcName);
                return;
            }

            DetectSelfCall(node.StatementList, funcName);
        }

        public override void ExplicitVisit(CreateProcedureStatement node)
        {
            string procName = node.ProcedureReference.Name.GetFullName();
            if (node.ProcedureReference.Number != null)
            {
                // If proc is numbered then the fully qualified reference must use the same number.
                // There is a separate rule to prevent use of numbered procs which is a deprecated feature.
                procName += TSqlDomainAttributes.NamePartSeparator + node.ProcedureReference.Number.Value;
            }

            DetectSelfCall(node.StatementList, procName);
        }

        private void DetectSelfCall(SelectStatement body, string selfName) => DoValidate(body, selfName);

        private void DetectSelfCall(StatementList body, string selfName)
        {
            if ((body?.Statements?.Count ?? 0) == 0)
            {
                // Empty body or this is an external module
                return;
            }

            if (string.IsNullOrEmpty(selfName))
            {
                return;
            }

            DoValidate(body, selfName);
        }

        private void DoValidate(TSqlFragment node, string selfName)
        {
           node.Accept(new SelfCallDetector(selfName, ViolationHandlerWithMessage));
        }

        private sealed class SelfCallDetector : TSqlFragmentVisitor
        {
            private readonly string selfName;
            private readonly Action<TSqlFragment, string> callback;

            public SelfCallDetector(string selfName, Action<TSqlFragment, string> callback)
            {
                this.selfName = selfName;
                this.callback = callback;
            }

            public override void Visit(ExecutableProcedureReference node)
            {
                var procRef = node.ProcedureReference.ProcedureReference;
                if (procRef is null)
                {
                    // EXEC @proc
                    return;
                }

                string procName = procRef.Name.GetFullName();
                if (procRef.Number != null)
                {
                    // Numbered sp call
                    procName += TSqlDomainAttributes.NamePartSeparator + procRef.Number.Value;
                }

                Validate(node, procName);
            }

            public override void Visit(SchemaObjectFunctionTableReference node)
            {
                // SELECT from a table-valued function
                Validate(node.SchemaObject, node.SchemaObject.GetFullName());
            }

            public override void Visit(FunctionCall node)
            {
                string funcName = node.FunctionName.Value;
                if (node.CallTarget != null)
                {
                    if (node.CallTarget is MultiPartIdentifierCallTarget id)
                    {
                        // Full function name is usually divided into CallTarget (schema) + Name (base name).
                        // However it may be something else e.g. 4-part fully qualified reference or CLR method call.
                        funcName = id.MultiPartIdentifier.Identifiers.GetFullName() + TSqlDomainAttributes.NamePartSeparator + funcName;
                    }
                    else
                    {
                        // XML method call or something alike - not our case
                        return;
                    }
                }

                Validate(node, funcName);
            }

            private void Validate(TSqlFragment node, string calledName)
            {
                if (string.Equals(calledName, selfName, StringComparison.OrdinalIgnoreCase))
                {
                    callback(node, calledName);
                }
            }
        }
    }
}
