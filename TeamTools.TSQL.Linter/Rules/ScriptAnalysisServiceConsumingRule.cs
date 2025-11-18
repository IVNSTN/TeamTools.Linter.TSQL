using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TeamTools.TSQL.Linter.Rules
{
    internal abstract class ScriptAnalysisServiceConsumingRule : AbstractRule, IScriptAnalysisServiceConsumer
    {
        private IScriptAnalysisServiceProvider serviceProvider;

        protected ScriptAnalysisServiceConsumingRule() : base()
        { }

        public void InjectServiceProvider(IScriptAnalysisServiceProvider provider)
        {
            serviceProvider = provider;
        }

        protected SVC GetService<SVC>(TSqlFragment node)
        where SVC : class
        {
            return serviceProvider.GetService<SVC>(node);
        }
    }
}
