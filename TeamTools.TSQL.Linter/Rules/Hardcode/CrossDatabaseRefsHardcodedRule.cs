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
        private static readonly Lazy<ICollection<string>> XmlMethodsInstance
            = new Lazy<ICollection<string>>(() => InitXmlMethodsInstance(), true);

        public CrossDatabaseRefsHardcodedRule() : base()
        {
        }

        private static ICollection<string> XmlMethods => XmlMethodsInstance.Value;

        public override void Visit(TSqlScript node)
            => node.AcceptChildren(new CrossDbLinkDetector(HandleNodeError));

        public override void Visit(OpenQueryTableReference node)
            => HandleNodeErrorIfAny(node.LinkedServer, "OPENQUERY");

        private static ICollection<string> InitXmlMethodsInstance()
        {
            return new SortedSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "nodes",
                "query",
                "value",
                "exists",
                "modify",
            };
        }

        private class CrossDbLinkDetector : TSqlFragmentVisitor
        {
            private static readonly ICollection<string> SystemDbLinks;
            private readonly IList<SchemaObjectName> synonymTargets = new List<SchemaObjectName>();
            private readonly Action<TSqlFragment, string> callback;

            static CrossDbLinkDetector()
            {
                SystemDbLinks = new SortedSet<string>(StringComparer.OrdinalIgnoreCase)
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

                if (SystemDbLinks.Contains(dbName) || synonymTargets.Contains(node))
                {
                    return;
                }

                if (IsXmlMethod(node, dbName, node.BaseIdentifier.Value))
                {
                    return;
                }

                callback(node, dbName);
            }

            public override void Visit(CreateSynonymStatement node) => synonymTargets.Add(node.ForName);

            private static bool IsXmlMethod(TSqlFragment node, string dbName, string baseIdentifier)
            {
                if (dbName.Contains(DataToolsVariablePrefix))
                {
                    // XML method cannot have SSDT-var ref
                    return false;
                }

                int i = node.LastTokenIndex + 1;
                int n = node.ScriptTokenStream.Count;
                while (i < n
                && node.ScriptTokenStream[i].TokenType.In(TSqlTokenType.WhiteSpace, TSqlTokenType.MultilineComment, TSqlTokenType.SingleLineComment))
                {
                    i++;
                }

                // For XML method there must be an opening parenthesis right after the last identifier part
                if (i >= n || node.ScriptTokenStream[i].TokenType != TSqlTokenType.LeftParenthesis)
                {
                    return false;
                }

                return XmlMethods.Contains(baseIdentifier);
            }
        }
    }
}
