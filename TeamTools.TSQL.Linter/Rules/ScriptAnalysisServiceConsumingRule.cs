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

        protected TSVC GetService<TSVC>(TSqlFragment node)
        where TSVC : class
        {
            return serviceProvider.GetService<TSVC>(node);
        }
    }
}
