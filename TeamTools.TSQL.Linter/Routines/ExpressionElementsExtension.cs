using System;

namespace TeamTools.TSQL.Linter.Routines
{
    [Flags]
    internal enum ExpressionElements
    {
        /// <summary>
        /// no elements of interest detected
        /// </summary>
        None = 0,

        /// <summary>
        /// Expression contains column reference
        /// </summary>
        Column = 1,

        /// <summary>
        /// Expression contains computation
        /// </summary>
        Computation = 2,

        /// <summary>
        /// Expression contains variable reference
        /// </summary>
        Variable = 4,

        /// <summary>
        /// Expression contains built-in function call
        /// </summary>
        BuiltInFunction = 8,

        /// <summary>
        /// Expression contains UDF function call
        /// </summary>
        UserDefinedFunction = 16,

        /// <summary>
        /// Expression contains literals
        /// </summary>
        Literal = 32,

        /// <summary>
        /// Expression contains any function call
        /// </summary>
        AnyFunction = BuiltInFunction | UserDefinedFunction,
    }

    internal static class ExpressionElementsExtension
    {
        public static bool IsNonSargablePredicate(this ExpressionElements elements)
        {
            return (IsColumnReference(elements) && HasComputations(elements))
                || IsUserDefinedFunction(elements);
        }

        public static bool IsColumnReference(this ExpressionElements elements)
        {
            return 0 != (elements & ExpressionElements.Column);
        }

        public static bool IsUserDefinedFunction(this ExpressionElements elements)
        {
            return 0 != (elements & ExpressionElements.UserDefinedFunction);
        }

        public static bool HasComputations(this ExpressionElements elements)
        {
            return 0 != (elements & (ExpressionElements.Computation | ExpressionElements.AnyFunction));
        }
    }
}
