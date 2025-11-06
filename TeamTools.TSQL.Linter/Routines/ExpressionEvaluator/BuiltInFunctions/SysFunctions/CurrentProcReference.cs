using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions
{
    [ExcludeFromCodeCoverage]
    public sealed class CurrentProcReference : SqlIntTypeValue
    {
        private static readonly int DummyId = 777;

        public CurrentProcReference(string procSchema, string procName, SqlIntTypeHandler typeHandler, SqlValueSource source)
        : base(typeHandler, new SqlIntTypeReference("dbo.INT", new SqlIntValueRange(0, int.MaxValue), typeHandler.GetValueFactory()), DummyId, source)
        {
            ProcSchema = procSchema;
            ProcName = procName;
        }

        public string ProcSchema { get; }

        public string ProcName { get; }

        public override SqlIntTypeValue DeepClone()
            => new CurrentProcReference(ProcName, ProcName, TypeHandler, Source.Clone());
    }
}
