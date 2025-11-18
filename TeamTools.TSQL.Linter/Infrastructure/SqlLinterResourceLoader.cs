using Newtonsoft.Json.Linq;
using TeamTools.Common.Linting.Infrastructure;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Infrastructure
{
    /// <summary>
    /// Resource loader.
    /// </summary>
    internal sealed partial class SqlLinterResourceLoader : ResourceLoader<LintingConfig>
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
            LoadTypeInfo(config, json);
        }

        protected override LintingConfig MakeConfig()
        {
            // not making anything new
            return Config;
        }
    }
}
