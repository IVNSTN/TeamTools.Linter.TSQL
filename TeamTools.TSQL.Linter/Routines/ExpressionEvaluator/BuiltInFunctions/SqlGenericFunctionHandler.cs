using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions
{
    public abstract class SqlGenericFunctionHandler<TArgs> : SqlFunctionHandler
    where TArgs : new()
    {
        public SqlGenericFunctionHandler(string funcName, int requiredArgs)
        : base(funcName, requiredArgs, requiredArgs)
        {
        }

        public SqlGenericFunctionHandler(string funcName, int minArgs, int maxArgs)
        : base(funcName, minArgs, maxArgs)
        {
        }

        public sealed override SqlValue Evaluate(IList<SqlFunctionArgument> args, EvaluationContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (args is null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            return SqlFunctionDecorator.Run(this, args, context);
        }

        public abstract bool ValidateArgumentValues(CallSignature<TArgs> call);

        public bool EvaluateResultType(CallSignature<TArgs> call)
        {
            call.ResultType = DoEvaluateResultType(call);
            return call.ResultTypeHandler != null;
        }

        public bool EvaluateResultValue(CallSignature<TArgs> call)
        {
            call.EvaluatedResult = DoEvaluateResultValue(call);
            return call.EvaluatedResult != null;
        }

        protected abstract string DoEvaluateResultType(CallSignature<TArgs> call);

        protected virtual SqlValue DoEvaluateResultValue(CallSignature<TArgs> call)
        {
            return call
                .ResultTypeHandler
                .MakeSqlDataTypeReference(call.ResultType)
                .MakeUnknownValue();
        }
    }
}
