using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;

namespace TeamTools.TSQL.Linter.Routines
{
    [Obsolete("It does not look like a great solution")]
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

        protected void MarkDetected(TSqlFragment node)
        {
            Detected = true;
            LastDetectedNode = node;
            FirstDetectedNode = FirstDetectedNode ?? node;
        }
    }
}
