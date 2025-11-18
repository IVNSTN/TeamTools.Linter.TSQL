using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Linq;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0765", "NATIVELY_COMPILED_TRIGGER")]
    [InMemoryRule]
    [TriggerRule]
    internal sealed class NativelyCompiledTriggerRule : AbstractRule
    {
        public NativelyCompiledTriggerRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            // CREATE PROC/TRIGGER/FUNC must be the first statement in a batch
            var firstStmt = batch.Statements[0];
            if (firstStmt is TriggerStatementBody trg)
            {
                HandleNodeErrorIfAny(trg.Options.FirstOrDefault(opt => opt.OptionKind == TriggerOptionKind.NativeCompile));
            }
        }
    }
}
