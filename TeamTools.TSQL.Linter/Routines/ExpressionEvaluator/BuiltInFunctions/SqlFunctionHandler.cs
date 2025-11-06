using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
{
    // TODO : looks more like Definition, not Handler
    // TODO : maybe get rid of specific classes, stick to metadata json file
    // and generate classes on the fly with dynamic mapping
    // to validation and evaluation strategy classes?
    public abstract class SqlFunctionHandler
    {
        protected SqlFunctionHandler(string funcName, int requiredArgs)
        : this(funcName, requiredArgs, requiredArgs)
        {
        }

        protected SqlFunctionHandler(string funcName, int minArgs, int maxArgs)
        : this(funcName)
        {
            if (minArgs < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minArgs), minArgs.ToString());
            }

            if (maxArgs < minArgs)
            {
                throw new ArgumentOutOfRangeException(nameof(maxArgs), maxArgs.ToString());
            }

            MinArgs = minArgs;
            MaxArgs = maxArgs;
        }

        protected SqlFunctionHandler(string funcName)
        {
            if (string.IsNullOrEmpty(funcName))
            {
                throw new ArgumentNullException(nameof(funcName));
            }

            FunctionName = funcName;
        }

        public string FunctionName { get; }

        protected int MinArgs { get; }

        protected int MaxArgs { get; }

        public bool ValidateArgumentCount(EvaluationContext context, int argCount)
        {
            if (argCount < MinArgs || argCount > MaxArgs)
            {
                context.InvalidNumberOfArgs(MinArgs, MaxArgs, argCount);
                return false;
            }

            return true;
        }

        public abstract SqlValue Evaluate(IList<SqlFunctionArgument> args, EvaluationContext context);
    }
}
