using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TeamTools.Common.Linting.Infrastructure;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Infrastructure
{
    internal sealed class SqlLinterResourceLoader : ResourceLoader<LintingConfig>
    {
        public SqlLinterResourceLoader(LintingConfig config) : base(config)
        {
        }

        protected override void FillConfig(LintingConfig config, JToken json)
        {
            base.FillConfig(config, json);

            LoadKeywords(config, json);
            LoadGlobalVariables(config, json);
            LoadFunctions(config, json);
            LoadEnums(config, json);
        }

        protected override LintingConfig MakeConfig()
        {
            // not making anything new
            return Config;
        }

        // TODO : use deserialize to object
        private static void LoadEnums(LintingConfig config, JToken json)
        {
            Debug.WriteLine("LoadEnums");
            Debug.Assert(config != null, "config not set");
            Debug.Assert(json != null, "json not set");

            var enumList = json.SelectTokens("..enums").ToList();

            foreach (var enumDefinition in enumList.Children().OfType<JProperty>())
            {
                string enumName = enumDefinition.Name.Trim();

                if (!string.IsNullOrEmpty(enumName))
                {
                    var enumData = new List<SqlServerMetadata.EnumElementProperties>();
                    var enumElements = ((JObject)enumDefinition.Value)
                        .Children()
                        .OfType<JProperty>()
                        .ToDictionary(prop => prop.Name, prop => prop.Value);

                    foreach (var enumElement in enumElements)
                    {
                        var enumElementData = new SqlServerMetadata.EnumElementProperties { Name = enumElement.Key };
                        enumData.Add(enumElementData);

                        if (enumElement.Value == null)
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

        // TODO : use deserialize to object
        private static void LoadFunctions(LintingConfig config, JToken json)
        {
            Debug.WriteLine("LoadFunctions");
            Debug.Assert(config != null, "config not set");
            Debug.Assert(json != null, "json not set");

            var functionList = json.SelectTokens("..functions").ToList();
            foreach (var func in functionList.Children().OfType<JProperty>())
            {
                var props = ((JObject)func.Value).Children()
                    .OfType<JProperty>()
                    .Where(prop => prop.Value is JValue)
                    .ToDictionary(prop => prop.Name, prop => prop.Value.ToString(), StringComparer.OrdinalIgnoreCase);
                var functionName = func.Name.Trim();

                if (string.IsNullOrEmpty(functionName))
                {
                    continue;
                }

                var functionSignature = new SqlMetaProgrammabilitySignature();
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
                    if (!funcParam.Value.ContainsKey("DataType"))
                    {
                        continue;
                    }

                    functionSignature.ParamDefinition.Add(funcParam.Key, funcParam.Value["DataType"]);
                }

                config.SqlServerMetadata.Functions.Add(functionName, functionSignature);
            }
        }

        // TODO : use deserialize to object
        private static void LoadGlobalVariables(LintingConfig config, JToken json)
        {
            Debug.WriteLine("LoadGlobalVariables");
            Debug.Assert(config != null, "config not set");
            Debug.Assert(json != null, "json not set");

            var globalVarList = json.SelectTokens("..globalVariables").ToList();

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

        // TODO : use deserialize to collection
        private static void LoadKeywords(LintingConfig config, JToken json)
        {
            Debug.WriteLine("LoadKeywords");
            Debug.Assert(config != null, "config not set");
            Debug.Assert(json != null, "json not set");

            var keywordList = json.SelectTokens("..keywords").ToList();

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
