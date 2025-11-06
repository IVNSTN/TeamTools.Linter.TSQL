using System;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators
{
    public static class ValidationScenario
    {
        public static ArgumentValidationDecorator For(string argumentName, SqlFunctionArgument arg, EvaluationContext context)
        {
            var argContext = new ArgumentValidation(argumentName, arg, context);
            return new ArgumentValidationDecorator(argContext);
        }

        public class ArgumentValidationDecorator
        {
            private readonly ArgumentValidation argContext;

            public ArgumentValidationDecorator(ArgumentValidation argContext)
            {
                this.argContext = argContext;
            }

            public ArgumentValidationStage<TOut> When<TOut>(Func<ArgumentValidation, Action<TOut>, bool> call)
            {
                TOut output = default;

                if (call.Invoke(argContext, o => output = o))
                {
                    return new ArgumentValidationStage<TOut>(this, output);
                }
                else
                {
                    return new DeadEnd<TOut>();
                }
            }

            public class ArgumentValidationStage<TOut>
            {
                public ArgumentValidationStage(ArgumentValidationDecorator decorator, TOut priorResult)
                {
                    Decorator = decorator;
                    PriorResult = priorResult;
                }

                public ArgumentValidationDecorator Decorator { get; }

                public TOut PriorResult { get; }

                public virtual ArgumentValidationStage<TNextOut> And<TNextOut>(Func<ArgumentValidation, TOut, Action<TNextOut>, bool> call)
                {
                    TNextOut next = default;

                    if (call.Invoke(Decorator.argContext, PriorResult, n => next = n))
                    {
                        return new ArgumentValidationStage<TNextOut>(Decorator, next);
                    }
                    else
                    {
                        return new DeadEnd<TNextOut>();
                    }
                }

                public virtual bool Then(Action<TOut> call)
                {
                    call.Invoke(PriorResult);
                    return true;
                }
            }

            public class DeadEnd<TOut> : ArgumentValidationStage<TOut>
            {
                public DeadEnd() : base(default, default)
                {
                }

                public override ArgumentValidationStage<TNextOut> And<TNextOut>(Func<ArgumentValidation, TOut, Action<TNextOut>, bool> call)
                {
                    return new DeadEnd<TNextOut>();
                }

                public override bool Then(Action<TOut> call)
                {
                    return false;
                }
            }
        }
    }
}
