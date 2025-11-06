using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0191", "NEVER_USED_SOURCE")]
    internal sealed class TableNeverUsedAsSourceRule : AbstractRule
    {
        public TableNeverUsedAsSourceRule() : base()
        {
        }

        public override void Visit(TSqlScript script)
        {
            var mainObjDetector = new MainScriptObjectDetector();
            script.Accept(mainObjDetector);

            // checking if this is "create table" script
            string tableName = mainObjDetector.ObjectFullName;
            if (!tableName.StartsWith(TSqlDomainAttributes.TempTablePrefix)
                && !tableName.StartsWith(TSqlDomainAttributes.VariablePrefix)
                && mainObjDetector.ObjectDefinitionNode is CreateTableStatement
                && mainObjDetector.ObjectDefinitionBatch.Statements.Count == 1)
            {
                return;
            }

            foreach (var batch in script.Batches)
            {
                ValidateBatch(batch);
            }
        }

        private void ValidateBatch(TSqlBatch node)
        {
            var tableDetector = new TableCreationDetector();
            node.AcceptChildren(tableDetector);

            var refDetector = new SourceTableReferenceDetector();
            node.AcceptChildren(refDetector);

            var tsqltDetector = new TestTableReferenceDetector();
            node.AcceptChildren(tsqltDetector);

            var tablesWithourRef = tableDetector.Tables.Keys.Where(tbl =>
                !refDetector.TableReferences.ContainsKey(tbl)
                && !tsqltDetector.TableReferences.ContainsKey(tbl));

            foreach (string tbl in tablesWithourRef)
            {
                HandleNodeError(tableDetector.Tables[tbl]);
            }
        }
    }
}
