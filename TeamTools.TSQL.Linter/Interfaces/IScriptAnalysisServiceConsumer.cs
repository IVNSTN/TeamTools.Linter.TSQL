namespace TeamTools.TSQL.Linter
{
    internal interface IScriptAnalysisServiceConsumer
    {
        void InjectServiceProvider(IScriptAnalysisServiceProvider provider);
    }
}
