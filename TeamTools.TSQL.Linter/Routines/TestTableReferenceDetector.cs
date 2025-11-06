using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines
{
    internal class TestTableReferenceDetector : TSqlFragmentVisitor
    {
        private static readonly ICollection<string> KnownTestProcs = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly IDictionary<string, TSqlFragment> tableReferences =
            new Dictionary<string, TSqlFragment>(StringComparer.OrdinalIgnoreCase);

        static TestTableReferenceDetector()
        {
            KnownTestProcs.Add("sp_executesql");
            KnownTestProcs.Add("sys.sp_executesql");
            KnownTestProcs.Add("tSQLt.AssertEmptyTable");
            KnownTestProcs.Add("tSQLt.AssertEqualsTable");
            KnownTestProcs.Add("tSQLt.AssertEqualsTableSchema");
            KnownTestProcs.Add("tSQLt.AssertResultSetsHaveSameMetadata");
        }

        public IDictionary<string, TSqlFragment> TableReferences => tableReferences;

        public override void Visit(ExecutableProcedureReference node)
        {
            if (node.ProcedureReference.ProcedureReference == null || node.Parameters.Count == 0)
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
            foreach (var param in parameters)
            {
                if (param.ParameterValue is StringLiteral str
                && str.Value.StartsWith(TSqlDomainAttributes.TempTablePrefix))
                {
                    tableReferences.TryAdd(str.Value, param);
                }
            }
        }
    }
}
