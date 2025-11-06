using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
{
    public class SqlTypeResolver : ISqlTypeResolver
    {
        private readonly IDictionary<string, ISqlTypeHandler> knownTypes
            = new SortedDictionary<string, ISqlTypeHandler>(StringComparer.OrdinalIgnoreCase);

        public bool IsSupportedType(string typeName)
            => !string.IsNullOrEmpty(typeName) && knownTypes.ContainsKey(typeName);

        public ISqlTypeHandler ResolveTypeHandler(string typeName)
            => IsSupportedType(typeName) ? knownTypes[typeName] : default;

        public void RegisterTypeHandler(ISqlTypeHandler typeHandler)
        {
            if (typeHandler is null)
            {
                throw new ArgumentNullException(nameof(typeHandler));
            }

            foreach (var typeName in typeHandler.SupportedTypes)
            {
                // yes, it should fail in case of collision
                knownTypes.Add(typeName, typeHandler);
            }
        }

        public SqlTypeReference ResolveType(DataTypeReference dataType)
        {
            if (dataType?.Name is null)
            {
                // e.g. CURSOR
                return default;
            }

            string typeName = dataType.Name.GetFullName();

            // TODO : generate dummy-references for built-in
            // but currently unsupported types
            return ResolveTypeHandler(typeName)?
                .MakeSqlDataTypeReference(dataType);
        }

        // TODO : Type will have default size. This is not always OK.
        public SqlTypeReference ResolveType(string typeName)
            => ResolveTypeHandler(typeName)?.MakeSqlDataTypeReference(typeName);
    }
}
