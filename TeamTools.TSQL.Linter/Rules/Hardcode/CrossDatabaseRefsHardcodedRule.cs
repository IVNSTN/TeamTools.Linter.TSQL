using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("HD0122", "CROSS_DB_REF_HARDCODED")]
    internal sealed class CrossDatabaseRefsHardcodedRule : AbstractRule
    {
        private const string DataToolsVariablePrefix = "$(";
        private static readonly Lazy<HashSet<string>> XmlMethodsInstance
            = new Lazy<HashSet<string>>(() => InitXmlMethodsInstance(), true);

        public CrossDatabaseRefsHardcodedRule() : base()
        {
        }

        private static HashSet<string> XmlMethods => XmlMethodsInstance.Value;

        protected override void ValidateBatch(TSqlBatch node) => node.AcceptChildren(new CrossDbLinkDetector(ViolationHandlerWithMessage));

        private static HashSet<string> InitXmlMethodsInstance()
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "nodes",
                "query",
                "value",
                "exists",
                "modify",
            };
        }

        private sealed class CrossDbLinkDetector : TSqlFragmentVisitor
        {
            private static readonly HashSet<string> SystemDbLinks;
            private readonly Action<TSqlFragment, string> callback;
            private List<SchemaObjectName> synonymTargets;

            static CrossDbLinkDetector()
            {
                SystemDbLinks = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "$(master)",
                    "$(msdb)",
                    "$(tempdb)",
                };
            }

            public CrossDbLinkDetector(Action<TSqlFragment, string> callback)
            {
                this.callback = callback;
            }

            public override void Visit(SchemaObjectName node)
            {
                if (node.DatabaseIdentifier is null)
                {
                    return;
                }

                string dbName = node.DatabaseIdentifier.Value;

                if (SystemDbLinks.Contains(dbName) || (synonymTargets != null && synonymTargets.Contains(node)))
                {
                    return;
                }

                if (IsXmlMethod(node, dbName, node.BaseIdentifier.Value))
                {
                    return;
                }

                callback(node, dbName);
            }

            public override void Visit(CreateSynonymStatement node) => (synonymTargets ?? (synonymTargets = new List<SchemaObjectName>())).Add(node.ForName);

            public override void Visit(OpenQueryTableReference node)
            {
                if (node.LinkedServer != null)
                {
                    callback(node.LinkedServer, "OPENQUERY");
                }
            }

            private static bool IsXmlMethod(TSqlFragment node, string dbName, string baseIdentifier)
            {
                if (dbName.Contains(DataToolsVariablePrefix))
                {
                    // XML method cannot have SSDT-var ref
                    return false;
                }

                if (!XmlMethods.Contains(baseIdentifier))
                {
                    return false;
                }

                int i = node.LastTokenIndex + 1;
                int n = node.ScriptTokenStream.Count;
                while (i < n && ScriptDomExtension.IsSkippableTokens(node.ScriptTokenStream[i].TokenType))
                {
                    i++;
                }

                // For XML method there must be an opening parenthesis right after the last identifier part
                if (i >= n || node.ScriptTokenStream[i].TokenType != TSqlTokenType.LeftParenthesis)
                {
                    return false;
                }

                return true;
            }
        }
    }
}
