using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;

namespace TeamTools.TSQL.Linter.Routines
{
    internal class MainScriptObjectDetector
    {
        // TODO : refactor
        private TSqlFragment detectedIdentifier = null;

        public MainScriptObjectDetector()
        { }

        public string ObjectFullName { get; private set; } = "";

        public TSqlBatch ObjectDefinitionBatch { get; private set; } = null;

        public TSqlStatement ObjectDefinitionNode { get; private set; } = null;

        /// <summary>
        /// Finds the object which looks like the main thing script was created for.
        /// For example CREATE PROC or CREATE TABLE in the script root probably mean that
        /// this proc/table definition is the only purpose this script was written for.
        /// </summary>
        /// <param name="node">Is supposed to be a TSQLScript or other big enough scope.</param>
        public void Analyze(TSqlFragment node)
        {
            var firstStatementDetector = new FirstStatementVisitor();
            node.Accept(firstStatementDetector);

            if (firstStatementDetector.FirstCreateStatement is null)
            {
                return;
            }

            ObjectDefinitionBatch = firstStatementDetector.LastCheckedBatch;
            ObjectDefinitionNode = firstStatementDetector.FirstCreateStatement;

            var identityDetector = new DatabaseObjectIdentifierDetector(IdentifierDetected, FullIdentifierDetected, true, true, true);
            firstStatementDetector.FirstCreateStatement.Accept(identityDetector);
        }

        protected void IdentifierDetected(Identifier node, string cleanedName)
        {
            if (detectedIdentifier != null)
            {
                return;
            }

            // SchemaObject would be catched first thus schema node = null cannot enter here
            if (!string.IsNullOrEmpty(ObjectFullName))
            {
                ObjectFullName = $"{ObjectFullName}{TSqlDomainAttributes.NamePartSeparator}{node.Value}";
            }
            else
            {
                ObjectFullName = node.Value;
            }

            detectedIdentifier = node;
        }

        protected void FullIdentifierDetected(SchemaObjectName node)
        {
            if (detectedIdentifier != null)
            {
                return;
            }

            ObjectFullName = node.GetFullName();

            detectedIdentifier = node;
        }

        [Obsolete("don't visit, just loop through batches")]
        private sealed class FirstStatementVisitor : TSqlFragmentVisitor
        {
            public TSqlStatement FirstCreateStatement { get; private set; } = null;

            public TSqlBatch LastCheckedBatch { get; private set; } = null;

            public override void Visit(TSqlBatch node)
            {
                if (FirstCreateStatement is null)
                {
                    LastCheckedBatch = node;
                }
            }

            public override void Visit(TSqlStatement node)
            {
                if (null != FirstCreateStatement
                || node is PredicateSetStatement
                || node is DropObjectsStatement)
                {
                    return;
                }

                if (node.ScriptTokenStream[node.FirstTokenIndex].TokenType == TSqlTokenType.Create)
                {
                    FirstCreateStatement = node;
                }
            }
        }
    }
}
