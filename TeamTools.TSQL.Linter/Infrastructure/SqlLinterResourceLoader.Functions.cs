using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Linq;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Infrastructure
{
    /// <summary>
    /// Loading Functions from SQL Server JSON meta data file.
    /// </summary>
    internal partial class SqlLinterResourceLoader
    {
        // TODO : use deserialize to object
        // TODO : refactoring and performance optimization needed
        private static void LoadFunctions(LintingConfig config, JToken json)
        {
            Debug.WriteLine("LoadFunctions");
            Debug.Assert(config != null, "config not set");
            Debug.Assert(json != null, "json not set");

            var functionList = json.SelectTokens("..functions")
                .Children()
                .OfType<JProperty>();

            foreach (var func in functionList)
            {
                var functionName = func.Name;
                if (string.IsNullOrEmpty(functionName))
                {
                    continue;
                }

                var props = ((JObject)func.Value).Children()
                    .OfType<JProperty>()
                    .Where(prop => prop.Value is JValue)
                    .ToDictionary(prop => prop.Name, prop => prop.Value.ToString(), StringComparer.OrdinalIgnoreCase);

                var functionSignature = new SqlServerMetaProgrammabilitySignature();
                if (props.TryGetValue("ResultType", out string resultType))
                {
                    functionSignature.DataType = resultType;
                }

                if (props.TryGetValue("ParamCount", out string paramCountValue)
                && int.TryParse(paramCountValue, out int paramCount))
                {
                    functionSignature.ParamCount = paramCount;
                }
                else
                {
                    functionSignature.ParamCount = -1;

                    if (props.TryGetValue("ParamCountMin", out string paramCountMinValue)
                    && int.TryParse(paramCountMinValue, out int paramCountMin))
                    {
                        functionSignature.ParamCountMin = paramCountMin;
                    }
                    else
                    {
                        functionSignature.ParamCountMin = -1;
                    }

                    if (props.TryGetValue("ParamCountMax", out string paramCountMaxValue)
                    && int.TryParse(paramCountMaxValue, out int paramCountMax))
                    {
                        functionSignature.ParamCountMax = paramCountMax;
                    }
                    else
                    {
                        functionSignature.ParamCountMax = -1;
                    }
                }

                // TODO : extract many param metadata, not only DataType
                // TODO : simplify expression or better move to Deserialize usage
                var funcParams = ((JObject)func.Value).Children()
                    .OfType<JProperty>()
                    .Where(prop => prop.Value is JObject && prop.Name.Equals("Params"))
                    .SelectMany(prop => prop.Value.Children())
                    .OfType<JProperty>()
                    .ToDictionary(prop => prop.Name, prop => prop.Value.Children().OfType<JProperty>().ToDictionary(p => p.Name, p => p.Value.ToString(), StringComparer.OrdinalIgnoreCase));

                foreach (var funcParam in funcParams)
                {
                    if (!funcParam.Value.TryGetValue("DataType", out var paramType))
                    {
                        continue;
                    }

                    functionSignature.ParamDefinition.Add(funcParam.Key, paramType);
                }

                config.SqlServerMetadata.Functions.Add(functionName, functionSignature);
            }
        }
    }
}
