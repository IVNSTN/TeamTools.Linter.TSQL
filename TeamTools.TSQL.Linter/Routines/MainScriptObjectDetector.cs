using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TeamTools.TSQL.Linter.Routines
{
    internal class MainScriptObjectDetector : TSqlFragmentVisitor
    {
        // TODO : refactor
        private TSqlFragment detectedIdentifier = null;

        public string ObjectFullName { get; private set; } = "";

        public TSqlBatch ObjectDefinitionBatch { get; private set; } = null;

        public TSqlStatement ObjectDefinitionNode { get; private set; } = null;

        public override void Visit(TSqlScript node)
        {
            var firstStatementDetector = new FirstStatementVisitor();
            node.Accept(firstStatementDetector);

            if (null == firstStatementDetector.FirstCreateStatement)
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
                ObjectFullName += TSqlDomainAttributes.NamePartSeparator;
            }

            ObjectFullName += node.Value;
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

        private class FirstStatementVisitor : TSqlFragmentVisitor
        {
            public TSqlStatement FirstCreateStatement { get; private set; } = null;

            public TSqlBatch LastCheckedBatch { get; private set; } = null;

            public override void Visit(TSqlBatch node)
            {
                if (FirstCreateStatement == null)
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
