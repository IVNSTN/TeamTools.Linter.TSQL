using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.SysFunctions
{
    [ExcludeFromCodeCoverage]
    public sealed class CurrentProcReference : SqlIntTypeValue
    {
        private static readonly int DummyId = 777;

        public CurrentProcReference(string procSchema, string procName, SqlIntTypeHandler typeHandler, SqlValueSource source)
        : base(typeHandler, new SqlIntTypeReference(TSqlDomainAttributes.Types.Int, new SqlIntValueRange(0, int.MaxValue), typeHandler.GetValueFactory()), DummyId, source)
        {
            ProcSchema = procSchema;
            ProcName = procName;
        }

        public string ProcSchema { get; }

        public string ProcName { get; }

        // TODO : does it really need to be clonable?
        public override SqlIntTypeValue DeepClone()
            => new CurrentProcReference(ProcName, ProcName, TypeHandler, Source.Clone());
    }
}
