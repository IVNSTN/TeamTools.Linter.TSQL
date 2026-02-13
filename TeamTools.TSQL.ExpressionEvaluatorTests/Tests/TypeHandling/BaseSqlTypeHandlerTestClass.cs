using TeamTools.TSQL.ExpressionEvaluator.Core;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator.TypeHandling
{
    public abstract class BaseSqlTypeHandlerTestClass
    {
        private SqlStrTypeHandler strTypeHandler;

        protected ViolationReporter Violations { get; private set; }

        protected SqlTypeResolver TypeResolver { get; private set; }

        protected SqlTypeConverter Converter { get; private set; }

        public virtual void SetUp()
        {
            Violations = new ViolationReporter();
            TypeResolver = new SqlTypeResolver();
            Converter = new SqlTypeConverter(TypeResolver);
            strTypeHandler = new SqlStrTypeHandler(Converter, Violations);
        }

        protected SqlValue MakeStr(string str)
        {
            return strTypeHandler.StrValueFactory.MakePreciseValue("VARCHAR", str, new SqlValueSource(SqlValueSourceKind.Variable, null));
        }
    }
}
