using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Infrastructure
{
    /// <summary>
    /// Loading Enums from SQL Server JSON meta data file.
    /// </summary>
    internal partial class SqlLinterResourceLoader
    {
        // TODO : use deserialize to object
        private static void LoadEnums(LintingConfig config, JToken json)
        {
            Debug.WriteLine("LoadEnums");
            Debug.Assert(config != null, "config not set");
            Debug.Assert(json != null, "json not set");

            var enumList = json.SelectTokens("..enums");

            foreach (var enumDefinition in enumList.Children().OfType<JProperty>())
            {
                string enumName = enumDefinition.Name.Trim();

                if (!string.IsNullOrEmpty(enumName))
                {
                    var enumData = new List<SqlServerMetaEnumElementProperties>();
                    var enumElements = ((JObject)enumDefinition.Value)
                        .Children()
                        .OfType<JProperty>()
                        .ToDictionary(prop => prop.Name, prop => prop.Value);

                    foreach (var enumElement in enumElements)
                    {
                        var enumElementData = new SqlServerMetaEnumElementProperties { Name = enumElement.Key };
                        enumData.Add(enumElementData);

                        if (enumElement.Value is null)
                        {
                            continue;
                        }

                        foreach (var elementProperty in enumElement.Value.Children().OfType<JProperty>())
                        {
                            if (string.IsNullOrEmpty(elementProperty.Name))
                            {
                                continue;
                            }

                            enumElementData.Properties.Add(elementProperty.Name, elementProperty.Value.ToString());
                        }
                    }

                    config.SqlServerMetadata.Enums.Add(enumName, enumData);
                }
            }
        }
    }
}
