using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TeamTools.TSQL.Linter.Routines
{
    internal sealed class NativelyCompiledModuleDetector : TSqlFragmentVisitor
    {
        private readonly Action<TSqlFragment, bool> callback;

        public NativelyCompiledModuleDetector(Action<TSqlFragment, bool> callback)
        {
            this.callback = callback;
        }

        public static void Detect(TSqlFragment node, Action<TSqlFragment, bool> callback)
        {
            node.Accept(new NativelyCompiledModuleDetector(callback));
        }

        public override void Visit(CreateTableStatement node)
        {
            IfNativeThenCallback(node, node.Options, opt => opt.OptionKind == TableOptionKind.MemoryOptimized, false);
        }

        public override void Visit(CreateTypeTableStatement node)
        {
            IfNativeThenCallback(node, node.Options, opt => opt.OptionKind == TableOptionKind.MemoryOptimized, false);
        }

        public override void Visit(ProcedureStatementBody node)
        {
            IfNativeThenCallback(node, node.Options, opt => opt.OptionKind == ProcedureOptionKind.NativeCompilation);
        }

        public override void Visit(FunctionStatementBody node)
        {
            IfNativeThenCallback(node, node.Options, opt => opt.OptionKind == FunctionOptionKind.NativeCompilation);
        }

        public override void Visit(TriggerStatementBody node)
        {
            IfNativeThenCallback(node, node.Options, opt => opt.OptionKind == TriggerOptionKind.NativeCompile);
        }

        private void IfNativeThenCallback<T>(TSqlFragment node, IList<T> options, Func<T, bool> hasOption, bool isProgrammability = true)
        {
            if (options.Any(hasOption))
            {
                callback?.Invoke(node, isProgrammability);
            }
        }
    }
}
