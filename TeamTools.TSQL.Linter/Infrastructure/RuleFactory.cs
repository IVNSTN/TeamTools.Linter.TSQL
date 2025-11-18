using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Diagnostics;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Interfaces;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Infrastructure
{
    internal class RuleFactory : IRuleFactory<ISqlRule>
    {
        private readonly LintingConfig config;
        private readonly TSqlParser parser;
        private readonly IScriptAnalysisServiceProvider svcProvider;

        public RuleFactory(LintingConfig config, TSqlParser parser, IScriptAnalysisServiceProvider svcProvider)
        {
            Debug.Assert(config != null, "config not set");
            Debug.Assert(parser != null, "parser not set");

            this.config = config;
            this.parser = parser;
            this.svcProvider = svcProvider;
        }

        public ISqlRule MakeRule(Type ruleClass, ViolationCallbackEvent callback)
        {
            Debug.Assert(parser != null, "parser not set");
            Debug.Assert(typeof(ISqlRule).IsAssignableFrom(ruleClass), "Wrong rule class type");

            var rule = (ISqlRule)Activator.CreateInstance(ruleClass);
            rule.Subscribe(callback);

            if (rule is IDeprecationHandler derp)
            {
                derp.LoadDeprecations(config.Deprecations);
            }

            if (rule is IKeywordDetector kwrd)
            {
                kwrd.LoadKeywords(config.SqlServerMetadata.Keywords);
            }

            if (rule is ISqlServerMetadataConsumer meta)
            {
                meta.LoadMetadata(config.SqlServerMetadata);
            }

            if (rule is ICommentAnalyzer anlz)
            {
                anlz.LoadSpecialCommentPrefixes(config.SpecialCommentPrefixes);
            }

            if (rule is IDynamicSqlParser prsr)
            {
                prsr.SetParser(parser);
            }

            if (rule is IScriptAnalysisServiceConsumer svc)
            {
                svc.InjectServiceProvider(svcProvider);
            }

            return rule;
        }
    }
}
