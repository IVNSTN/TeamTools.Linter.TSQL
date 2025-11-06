using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions
{
    public class SqlFunctionDecorator
    {
        private readonly EvaluationContext context;

        protected SqlFunctionDecorator(EvaluationContext context)
        {
            this.context = context;
        }

        public static SqlValue Run<TArgs>(SqlGenericFunctionHandler<TArgs> func, IList<SqlFunctionArgument> args, EvaluationContext context)
        where TArgs : new()
        {
            return UnderContext(context)
                .StartFlow((ctx) => MakeCallSignature<TArgs>(ctx, args))
                .Then(call => func.ValidateArgumentCount(call.Context, args.Count))
                .Then(call => func.ValidateArgumentValues(call))
                .Then(call => func.EvaluateResultType(call))
                .Then(call => func.EvaluateResultValue(call))
                .Run();
        }

        private static SqlFunctionDecorator UnderContext(EvaluationContext context)
        {
            return new SqlFunctionDecorator(context);
        }

        private static CallSignature<TArgs> MakeCallSignature<TArgs>(EvaluationContext context, IList<SqlFunctionArgument> args)
        where TArgs : new()
        {
            return new CallSignature<TArgs>(context, args);
        }

        private SqlFunctionCallEvaluationRunnerForSignature<TArgs> StartFlow<TArgs>(Func<EvaluationContext, CallSignature<TArgs>> call)
        where TArgs : new()
        {
            return new SqlFunctionCallEvaluationRunnerForSignature<TArgs>(call.Invoke(context));
        }

        public class SqlFunctionCallEvaluationRunnerForSignature<TArgs>
        where TArgs : new()
        {
            private readonly ICollection<Func<CallSignature<TArgs>, bool>> actions = new List<Func<CallSignature<TArgs>, bool>>();
            private readonly CallSignature<TArgs> call;

            public SqlFunctionCallEvaluationRunnerForSignature(CallSignature<TArgs> call)
            {
                this.call = call;
            }

            public SqlFunctionCallEvaluationRunnerForSignature<TArgs> Then(Func<CallSignature<TArgs>, bool> actionCallback)
            {
                if (actionCallback is null)
                {
                    throw new ArgumentNullException(nameof(actionCallback));
                }

                actions.Add(actionCallback);

                return this;
            }

            public SqlValue Run()
            {
                call.EvaluatedResult = null;

                foreach (var act in actions)
                {
                    if (!act.Invoke(call))
                    {
                        // Any step can set precise or approximate result
                        // if it realized that no further evaluation is needed or possible
                        // based on given arguments and evaluated output type.
                        // Thus "null" is not assigned to EvaluatedResult here.
                        break;
                    }
                }

                return call.EvaluatedResult;
            }
        }
    }
}
