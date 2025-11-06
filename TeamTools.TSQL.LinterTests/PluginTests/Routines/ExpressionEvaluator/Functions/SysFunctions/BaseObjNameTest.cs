using NUnit.Framework;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

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
