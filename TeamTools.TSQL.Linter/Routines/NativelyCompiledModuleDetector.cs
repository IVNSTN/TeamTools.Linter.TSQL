using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;

namespace TeamTools.TSQL.Linter.Routines
{
    internal sealed class NativelyCompiledModuleDetector : TSqlFragmentVisitor
    {
        private readonly Action<TSqlFragment, bool> callback;

        public NativelyCompiledModuleDetector(Action<TSqlFragment, bool> callback)
        {
            this.callback = callback;
        }

        public override void Visit(CreateTableStatement node) => IfNativeThenValidate(node.Definition, node.Options.HasOption(TableOptionKind.MemoryOptimized), false);

        public override void Visit(CreateTypeTableStatement node) => IfNativeThenValidate(node.Definition, node.Options.HasOption(TableOptionKind.MemoryOptimized), false);

        public override void Visit(ProcedureStatementBody node) => IfNativeThenValidate(node, node.Options.HasOption(ProcedureOptionKind.NativeCompilation));

        public override void Visit(FunctionStatementBody node) => IfNativeThenValidate(node, node.Options.HasOption(FunctionOptionKind.NativeCompilation));

        public override void Visit(TriggerStatementBody node) => IfNativeThenValidate(node.StatementList, node.Options.HasOption(TriggerOptionKind.NativeCompile));

        private void IfNativeThenValidate(TSqlFragment node, bool isNative, bool isProgrammability = true)
        {
            if (isNative)
            {
                callback(node, isProgrammability);
            }
        }
    }
}
