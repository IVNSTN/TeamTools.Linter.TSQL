using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0888", "EXTENDED_PROPERTY_DUP")]
    internal sealed partial class ExtendedPropertyDupRule : AbstractRule
    {
        public ExtendedPropertyDupRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            var sb = ObjectPools.StringBuilderPool.Get();

            var detector = new ExtendedPropertyDupDetector(sb, ViolationHandlerWithMessage);

            for (int i = 0, n = node.Batches.Count; i < n; i++)
            {
                var batch = node.Batches[i];
                for (int j = 0, m = batch.Statements.Count; j < m; j++)
                {
                    var stmt = batch.Statements[j];
                    if (stmt is ExecuteStatement exec)
                    {
                        exec.AcceptChildren(detector);
                    }
                }
            }

            ObjectPools.StringBuilderPool.Return(sb);
        }
    }
}
