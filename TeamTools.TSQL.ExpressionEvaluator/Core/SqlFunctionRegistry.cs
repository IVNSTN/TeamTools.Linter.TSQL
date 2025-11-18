using System;
using System.Collections.Generic;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions;
using TeamTools.TSQL.ExpressionEvaluator.Interfaces;

namespace TeamTools.TSQL.ExpressionEvaluator.Core
{
    public class SqlFunctionRegistry : ISqlFunctionRegistry
    {
        private readonly Dictionary<string, SqlFunctionHandler> functions
            = new Dictionary<string, SqlFunctionHandler>(100, StringComparer.OrdinalIgnoreCase);

        public SqlFunctionRegistry()
        {
        }

        public IDictionary<string, SqlFunctionHandler> Functions => functions;

        public bool IsFunctionRegistered(string name)
            => !string.IsNullOrEmpty(name) && functions.ContainsKey(name);

        public void RegisterFunction(SqlFunctionHandler def)
        {
            if (def is null)
            {
                throw new ArgumentNullException(nameof(def));
            }

#if NETSTANDARD
            if (!functions.ContainsKey(def.FunctionName))
            {
                functions.Add(def.FunctionName, def);
            }
            else
#else
            if (!functions.TryAdd(def.FunctionName, def))
#endif
            {
                throw new ArgumentException($"Function '{def.FunctionName}' is already registered", nameof(def));
            }
        }

        public SqlFunctionHandler GetFunction(string name)
        {
            if (!string.IsNullOrEmpty(name) && functions.TryGetValue(name, out var fn))
            {
                return fn;
            }

            return default;
        }
    }
}
