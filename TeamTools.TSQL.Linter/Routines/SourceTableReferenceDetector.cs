using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines
{
    internal class SourceTableReferenceDetector : TSqlFragmentVisitor
    {
        private readonly TableReferenceDetector tableRefDetector;
        private readonly IDictionary<string, TSqlFragment> tableReferences =
            new Dictionary<string, TSqlFragment>(StringComparer.OrdinalIgnoreCase);

        public SourceTableReferenceDetector()
        {
            tableRefDetector = new TableReferenceDetector(tableReferences);
        }

        public IDictionary<string, TSqlFragment> TableReferences => tableReferences;

        public override void Visit(QuerySpecification node)
        {
            node.AcceptChildren(tableRefDetector);
        }

        // update, delete and merge are not inherited from QuerySpecification
        // but they can have FROM clauses and subqueries with FROM
        public override void Visit(FromClause node)
        {
            node.AcceptChildren(tableRefDetector);
        }

        // if merge source is not a subquery but a direct link to a table
        public override void Visit(MergeSpecification node)
        {
            node.TableReference.Accept(tableRefDetector);
        }
    }
}
