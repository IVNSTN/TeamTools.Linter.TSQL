namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.SysFunctions
{
    public class ObjectName : ObjectIdentificationFunction
    {
        private static readonly string FuncName = "OBJECT_NAME";
        private static readonly int MinArgCount = 1;
        private static readonly int MaxArgCount = 2;

        public ObjectName() : base(FuncName, MinArgCount, MaxArgCount)
        {
        }
    }
}
