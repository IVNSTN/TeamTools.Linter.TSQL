using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.SysFunctions
{
    public class ObjectSchemaName : ObjectIdentificationFunction
    {
        private static readonly string FuncName = "OBJECT_SCHEMA_NAME";
        private static readonly int MinArgCount = 1;
        private static readonly int MaxArgCount = 2;

        public ObjectSchemaName() : base(FuncName, MinArgCount, MaxArgCount)
        {
        }

        protected override string GetNamePartForCurrentProc(CurrentProcReference currentProc)
            => currentProc.ProcSchema;
    }
}
