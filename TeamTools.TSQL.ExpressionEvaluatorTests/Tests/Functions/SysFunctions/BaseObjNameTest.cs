using NUnit.Framework;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.SysFunctions;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    public abstract class BaseObjNameTest : BaseMockFunctionTest
    {
        protected SqlValue RandomObjId { get; private set; }

        protected SqlValue CurrentProcId { get; private set; }

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            var intHandler = new SqlIntTypeHandler(Context.Converter, Context.Violations);

            RandomObjId = MakeInt(321);
            CurrentProcId = new CurrentProcReference("myschema", "myproc", intHandler, default);
        }
    }
}
