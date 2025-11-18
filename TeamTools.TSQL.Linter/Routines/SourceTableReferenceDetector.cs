using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines
{
    internal class SourceTableReferenceDetector : TSqlFragmentVisitor
    {
        private static readonly HashSet<string> KnownTestProcs;
        private readonly TableReferenceDetector tableRefDetector;
        private readonly Dictionary<string, TSqlFragment> tableReferences =
            new Dictionary<string, TSqlFragment>(StringComparer.OrdinalIgnoreCase);

        static SourceTableReferenceDetector()
        {
            KnownTestProcs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "sp_executesql",
                "sys.sp_executesql",
                "tSQLt.AssertEmptyTable",
                "tSQLt.AssertEqualsTable",
                "tSQLt.AssertEqualsTableSchema",
                "tSQLt.AssertResultSetsHaveSameMetadata",
            };
        }

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

        public override void Visit(ExecutableProcedureReference node)
        {
            if (node.ProcedureReference.ProcedureReference is null || node.Parameters.Count == 0)
            {
                return;
            }

            string procName = node.ProcedureReference.ProcedureReference.Name.GetFullName();
            if (KnownTestProcs.Contains(procName))
            {
                ExtractTempTableReferences(node.Parameters);
            }
        }

        private void ExtractTempTableReferences(IList<ExecuteParameter> parameters)
        {
            // detecting all possible # dynamic references
            int n = parameters.Count;
            for (int i = 0; i < n; i++)
            {
                if (parameters[i].ParameterValue is StringLiteral str
                && str.Value.StartsWith(TSqlDomainAttributes.TempTablePrefix))
                {
                    tableReferences.TryAdd(str.Value, str);
                }
            }
        }
    }
}
