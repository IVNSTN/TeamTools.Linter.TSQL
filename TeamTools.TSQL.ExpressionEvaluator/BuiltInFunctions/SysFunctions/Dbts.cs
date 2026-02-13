using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;
using TeamTools.TSQL.ExpressionEvaluator.Routines;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.SysFunctions
{
    internal class Dbts : GlobalVariableHandler
    {
        private static readonly string FuncName = "@@DBTS";
        private static readonly string ResultTypeName = TSqlDomainAttributes.Types.VarBinary;

        public Dbts() : base(FuncName, ResultTypeName)
        {
        }
    }
}
