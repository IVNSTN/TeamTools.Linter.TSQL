using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator.TypeHandling
{
    public abstract class BaseSqlTypeHandlerTestClass
    {
        protected ViolationReporter Violations { get; private set; }

        protected SqlTypeResolver TypeResolver { get; private set; }

        protected SqlTypeConverter Converter { get; private set; }

        public virtual void SetUp()
        {
            Violations = new ViolationReporter();
            TypeResolver = new SqlTypeResolver();
            Converter = new SqlTypeConverter(TypeResolver);
        }
    }
}
