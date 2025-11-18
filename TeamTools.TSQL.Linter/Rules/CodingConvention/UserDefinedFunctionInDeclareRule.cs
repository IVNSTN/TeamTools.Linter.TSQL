using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0279", "UDF_IN_DECLARE")]
    internal sealed class UserDefinedFunctionInDeclareRule : AbstractRule, ISqlServerMetadataConsumer
    {
        private HashSet<string> systemFunctions;

        public UserDefinedFunctionInDeclareRule() : base()
        {
        }

        public void LoadMetadata(SqlServerMetadata data)
        {
            systemFunctions = new HashSet<string>(data.Functions.Keys, StringComparer.OrdinalIgnoreCase);
        }

        public override void Visit(DeclareVariableElement node)
        {
            Debug.Assert(systemFunctions?.Count > 0, "systemFunctions not loaded");
            if (systemFunctions.Count == 0)
            {
                return;
            }

            if (node.Value is null || node.Value is Literal)
            {
                return;
            }

            node.Value.Accept(new UdfCallVisitor(ViolationHandlerWithMessage, systemFunctions));
        }

        private class UdfCallVisitor : TSqlFragmentVisitor
        {
            private readonly Action<TSqlFragment, string> callback;
            private readonly HashSet<string> ignoredFunctions;

            public UdfCallVisitor(Action<TSqlFragment, string> callback, HashSet<string> ignoredFunctions)
            {
                this.callback = callback;
                this.ignoredFunctions = ignoredFunctions;
            }

            public override void Visit(FunctionCall node)
            {
                if (ignoredFunctions.Contains(node.FunctionName.Value))
                {
                    return;
                }

                if (node.CallTarget != null && !(node.CallTarget is MultiPartIdentifierCallTarget mid
                && mid.MultiPartIdentifier.Identifiers.Count == 1))
                {
                    // e.g. xml.value function
                    return;
                }

                callback(node, node.FunctionName.Value);
            }
        }
    }
}
