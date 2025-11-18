using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using TeamTools.Common.Linting;
using TeamTools.Common.Linting.Infrastructure;
using TeamTools.TSQL.ExpressionEvaluator;
using TeamTools.TSQL.Linter.Infrastructure;
using TeamTools.TSQL.Linter.Interfaces;
using TeamTools.TSQL.Linter.Properties;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter
{
    public class TransactSqlLinter : ILinter
    {
        private readonly LintingConfigLoader loader = new LintingConfigLoader();
        private readonly ScriptAnalysisServiceProvider svcProvider = new ScriptAnalysisServiceProvider();
        private IRuleClassFinder ruleClassFinder;
        private IRuleCollectionHandler<ISqlRule, TSqlFragment> ruleCollection;
        private IReporter reporter;
        private int ruleCount = 0;

        public TransactSqlLinter() : base()
        {
        }

        public TransactSqlLinter(IRuleClassFinder classFinder) : this()
        {
            this.ruleClassFinder = classFinder;
        }

        public TransactSqlLinter(LintingConfig config, IRuleClassFinder classFinder, IReporter reporter) : this(classFinder)
        {
            this.Config = config;
            this.reporter = reporter;

            InitRulesCollection();
        }

        public IDictionary<string, List<RuleInstance<ISqlRule>>> Rules => ruleCollection?.Rules;

        public LintingConfig Config { get; protected internal set; }

        public int GetRulesCount() => ruleCount;

        public void SetReporter(IReporter reporter)
        {
            this.reporter = reporter;
        }

        public IEnumerable<string> GetSupportedFiles() => Config.SupportedFileTypes;

        // TODO : refactor whole workflow
        public void Init(string configPath, IReporter reporter, string cultureCode)
        {
            Debug.Assert(configPath != "", "empty config path");
            Debug.Assert(reporter != null, "reporter null");

            if (Config != null)
            {
                throw new InvalidOperationException(Strings.PluginMsg_ConfigAlreadyLoaded);
            }

            SetReporter(reporter);
            LoadConfig(configPath);
            LoadResources(Config, new AssemblyWrapper(), cultureCode);

            InitRulesCollection();
        }

        public virtual void LoadConfig(string configPath)
        {
            Config = loader.LoadConfig(configPath?.Trim());

            if (Config is null)
            {
                throw new FileLoadException(Strings.PluginMsg_FailedLoadingConfig, configPath);
            }
        }

        public void PerformAction(ILintingContext context)
        {
            // TODO : or error, some message for verbose-mode?
            if (GetRulesCount() == 0)
            {
                return;
            }

            var applyDelegate = new Action<ISqlRule, TSqlFragment>((rule, sqlFragment) =>
            {
                if (rule is IFileLevelRule fileRule)
                {
                    fileRule.VerifyFile(context.FilePath, sqlFragment);
                }
                else
                {
                    rule.Validate(sqlFragment);
                }
            });

            ruleCollection.ApplyRulesTo(context, applyDelegate);

            svcProvider.DisposeScriptServices(default);
        }

        protected void InitRulesCollection()
        {
            var tsqlParser = TSqlParserFactory.Make(Config.CompatibilityLevel);

            if (ruleClassFinder is null)
            {
                ruleClassFinder = new RuleClassFinder(CompatibilityConverter.ToSqlVersion(Config.CompatibilityLevel));
            }

            ruleCollection = new RuleCollectionHandler(
                reporter,
                new RuleFactory(Config, tsqlParser, svcProvider),
                ruleClassFinder,
                new SqlFileParser(tsqlParser),
                Config);
            ruleCollection.MakeRules();

            ruleCount = ruleCollection?.RuleCount() ?? 0;

            // TODO : refactoring needed. current implementation is just a proof of concept.
            svcProvider.RegisterServiceFactory((TSqlScript script) =>
            {
                var svc = new MainScriptObjectDetector();
                svc.Analyze(script);
                return svc;
            });

            svcProvider.RegisterServiceFactory((TSqlScript script) => new TableDefinitionElementsEnumerator(script));

            svcProvider.RegisterServiceFactory((TSqlBatch batch) => new ScalarExpressionEvaluator(batch));

            svcProvider.RegisterServiceFactory((TSqlScript script) =>
            {
                var svc = new TableIndicesVisitor();
                script.Accept(svc);
                return svc;
            });

            svcProvider.RegisterServiceFactory((TSqlBatch batch) =>
            {
                var svc = new ParenthesisParser(batch);
                svc.Parse();
                return svc;
            });
        }

        private static void LoadResources(LintingConfig cfg, IAssemblyWrapper assemblyWrapper, string cultureCode = "en-us")
        {
            const string resourceSubfolder = "Resources";
            var loader = new SqlLinterResourceLoader(cfg);
            // This is DLL code and we need path to current DLL
            // but AppDomain.CurrentDomain.BaseDirectory would return EXE path, not DLL
            // TODO : does not look like a real abstraction from infrastructure
            string dllPath = assemblyWrapper.GetExecutingPath(Assembly.GetExecutingAssembly());

            string messagesFile = "ViolationMessages.json";
            if (!string.IsNullOrEmpty(cultureCode) && !string.Equals(cultureCode, "en-us"))
            {
                if (File.Exists(Path.Combine(dllPath, resourceSubfolder, $"ViolationMessages.{cultureCode}.json")))
                {
                    messagesFile = $"ViolationMessages.{cultureCode}.json";
                }
            }

            loader.LoadConfig(Path.Combine(dllPath, resourceSubfolder, messagesFile));
            loader.LoadConfig(Path.Combine(dllPath, resourceSubfolder, "SqlServerMetadata.json"));
        }
    }
}
