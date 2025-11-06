using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;

namespace TeamTools.TSQL.Linter.Routines
{
    internal abstract class TSqlViolationDetector : TSqlFragmentVisitor
    {
        public bool Detected { get; protected set; }

        public TSqlFragment FirstDetectedNode { get; private set; }

        public TSqlFragment LastDetectedNode { get; private set; }

        public static void DetectFirst(TSqlViolationDetector detector, TSqlFragment src, Action<TSqlFragment> callback)
        {
            if (src is null)
            {
                return;
            }

            src.Accept(detector);
            if (detector.Detected)
            {
                callback(detector.FirstDetectedNode);
            }
        }

        public static void DetectFirst<T>(TSqlFragment src, Action<TSqlFragment> callback)
        where T : TSqlViolationDetector, new()
        => DetectFirst((T)Activator.CreateInstance(typeof(T)), src, callback);

        protected void MarkDetected(TSqlFragment node)
        {
            Detected = true;
            LastDetectedNode = node;
            FirstDetectedNode = FirstDetectedNode ?? node;
        }
    }
}
