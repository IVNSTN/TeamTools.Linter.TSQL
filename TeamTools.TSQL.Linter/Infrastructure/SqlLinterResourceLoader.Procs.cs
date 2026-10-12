using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Linq;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Infrastructure
{
    /// <summary>
    /// Loading Procs from SQL Server JSON meta data file.
    /// </summary>
    internal partial class SqlLinterResourceLoader
    {
        // TODO : use deserialize to object
        // TODO : refactoring and performance optimization needed
        private static void LoadProcs(LintingConfig config, JToken json)
        {
            Debug.WriteLine("LoadProcs");
            Debug.Assert(config != null, "config not set");
            Debug.Assert(json != null, "json not set");

            var procList = json.SelectTokens("..procs")
                .Children()
                .OfType<JProperty>();

            foreach (var proc in procList)
            {
                var procName = proc.Name;
                if (string.IsNullOrEmpty(procName))
                {
                    continue;
                }

                var props = ((JObject)proc.Value).Children()
                    .OfType<JProperty>()
                    .Where(prop => prop.Value is JValue)
                    .ToDictionary(prop => prop.Name, prop => prop.Value.ToString(), StringComparer.OrdinalIgnoreCase);

                if (props.TryGetValue("StatusCheck", out string statusCheckValue))
                {
                    var procSignature = new SqlServerMetaProcSignature { StatusCheck = statusCheckValue };
                    config.SqlServerMetadata.Procs.Add(procName, procSignature);
                }
            }
        }
    }
}
