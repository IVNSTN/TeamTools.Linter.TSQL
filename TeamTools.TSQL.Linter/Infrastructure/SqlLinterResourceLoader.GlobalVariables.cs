using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Linq;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Infrastructure
{
    /// <summary>
    /// Loading Global variables from SQL Server JSON meta data file.
    /// </summary>
    internal partial class SqlLinterResourceLoader
    {
        // TODO : use deserialize to object
        private static void LoadGlobalVariables(LintingConfig config, JToken json)
        {
            Debug.WriteLine("LoadGlobalVariables");
            Debug.Assert(config != null, "config not set");
            Debug.Assert(json != null, "json not set");

            var globalVarList = json.SelectTokens("..globalVariables");

            foreach (var configValue in globalVarList.Children().OfType<JObject>())
            {
                var props = configValue.Children()
                    .OfType<JProperty>()
                    .ToDictionary(prop => prop.Name, prop => prop.Value.ToString());

                if (props.TryGetValue("Name", out var globalVarName)
                && props.TryGetValue("ResultType", out var globalVarDatatype)
                && !string.IsNullOrEmpty(globalVarName))
                {
                    config.SqlServerMetadata.GlobalVariables.Add(globalVarName, globalVarDatatype);
                }
            }
        }
    }
}
