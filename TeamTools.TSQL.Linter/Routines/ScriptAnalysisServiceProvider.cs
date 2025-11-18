using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace TeamTools.TSQL.Linter.Routines
{
    internal class ScriptAnalysisServiceProvider : IScriptAnalysisServiceProvider
    {
        private readonly ThreadLocal<Dictionary<TSqlFragment, List<object>>> services = new ThreadLocal<Dictionary<TSqlFragment, List<object>>>(() => new Dictionary<TSqlFragment, List<object>>());
        private readonly ConcurrentDictionary<Type, Func<TSqlFragment, object>> serviceFactories = new ConcurrentDictionary<Type, Func<TSqlFragment, object>>();

        public ScriptAnalysisServiceProvider()
        { }

        public T GetService<T>(TSqlFragment script)
        where T : class
        {
            // lock (services)
            {
                if (services.Value.TryGetValue(script, out var ss))
                {
                    int n = ss.Count;
                    for (int i = 0; i < n; i++)
                    {
                        if (ss[i] is T typedSvc)
                        {
                            return typedSvc;
                        }
                    }
                }

                if (serviceFactories.TryGetValue(typeof(T), out var factoryMethod))
                {
                    var svc = factoryMethod(script);
                    if (svc != null && svc is T typedSvc)
                    {
                        PutService(script, typedSvc);
                        return typedSvc;
                    }
                }
            }

            Debug.Assert(false, "service not found. we should never get here");

            return default;
        }

        public void PutService<T>(TSqlFragment script, T svc)
        where T : class
        {
            if (!services.Value.TryGetValue(script, out var ss))
            {
                ss = new List<object>();
                services.Value.Add(script, ss);
            }

            ss.Add(svc);
        }

        public void RegisterServiceFactory<TS, T>(Func<TS, T> factoryMethod)
        where TS : TSqlFragment
        where T : class
        {
            serviceFactories.TryAdd(typeof(T), sql => factoryMethod(sql as TS));
        }

        public void DisposeScriptServices(TSqlFragment script)
        {
            services.Value.Clear();
        }
    }
}
