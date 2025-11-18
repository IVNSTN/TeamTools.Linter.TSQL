using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Linq;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Infrastructure
{
    /// <summary>
    /// Loading Keywords from SQL Server JSON meta data file.
    /// </summary>
    internal partial class SqlLinterResourceLoader
    {
        // TODO : use deserialize to collection
        private static void LoadKeywords(LintingConfig config, JToken json)
        {
            Debug.WriteLine("LoadKeywords");
            Debug.Assert(config != null, "config not set");
            Debug.Assert(json != null, "json not set");

            var keywordList = json.SelectTokens("..keywords");

            foreach (var configValue in keywordList.Children().OfType<JValue>())
            {
                var keywordName = configValue.Value.ToString().Trim();

                if (!string.IsNullOrEmpty(keywordName))
                {
                    config.SqlServerMetadata.Keywords.Add(keywordName);
                }
            }
        }
    }
}
