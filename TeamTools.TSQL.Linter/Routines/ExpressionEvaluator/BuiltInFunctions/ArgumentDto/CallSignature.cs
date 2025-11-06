using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions
{
    public class CallSignature<TIn>
    where TIn : new()
    {
        private string resultType;

        public CallSignature(EvaluationContext context, IList<SqlFunctionArgument> rawArgs)
        {
            Context = context;
            RawArgs = rawArgs;
            ValidatedArgs = new TIn();
        }

        public EvaluationContext Context { get; }

        public IList<SqlFunctionArgument> RawArgs { get; }

        public TIn ValidatedArgs { get; }

        public string ResultType
        {
            get
            {
                return resultType;
            }

            set
            {
                resultType = value;
                ResultTypeHandler = Context.TypeResolver.ResolveTypeHandler(value);
            }
        }

        public ISqlTypeHandler ResultTypeHandler { get; private set; }

        public SqlValue EvaluatedResult { get; set; }
    }
}
