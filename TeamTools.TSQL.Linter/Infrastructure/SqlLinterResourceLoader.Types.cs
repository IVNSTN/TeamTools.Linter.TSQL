using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Linq;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Infrastructure
{
    /// <summary>
    /// Loading System Types from SQL Server JSON meta data file.
    /// </summary>
    internal partial class SqlLinterResourceLoader
    {
        // TODO : use deserialize to collection
        private static void LoadTypeInfo(LintingConfig config, JToken json)
        {
            Debug.WriteLine("LoadGlobalVariables");
            Debug.Assert(config != null, "config not set");
            Debug.Assert(json != null, "json not set");

            var typeList = json.SelectTokens("..types");

            foreach (var typeData in typeList.Children().OfType<JProperty>())
            {
                var typeName = typeData.Name.Trim();

                if (string.IsNullOrEmpty(typeName))
                {
                    continue;
                }

                var props = ((JObject)typeData.Value).Children()
                    .OfType<JProperty>()
                    .Where(prop => prop.Value is JValue)
                    .ToDictionary(prop => prop.Name, prop => prop.Value.ToString(), StringComparer.OrdinalIgnoreCase);

                var typeDescr = new SqlServerMetaTypeDescription();
                if (props.TryGetValue("aka", out string alsoKnownAs))
                {
                    typeDescr.AlsoKnownAs = alsoKnownAs;
                }

                if (props.TryGetValue("nativelyCompiled", out string nativelyCompiledValue)
                && bool.TryParse(nativelyCompiledValue, out bool nativelyCompiled))
                {
                    typeDescr.CanBeNativelyCompiled = nativelyCompiled;
                }

                if (props.TryGetValue("precedence", out string precedenceValue)
                && int.TryParse(precedenceValue, out int precedence))
                {
                    typeDescr.Precedence = precedence;
                }

                if (props.TryGetValue("forbidden", out string forbiddenValue)
                && bool.TryParse(forbiddenValue, out bool forbidden))
                {
                    typeDescr.ForceToOriginalName = forbidden;
                }

                config.SqlServerMetadata.Types.Add(typeName, typeDescr);
            }
        }
    }
}
