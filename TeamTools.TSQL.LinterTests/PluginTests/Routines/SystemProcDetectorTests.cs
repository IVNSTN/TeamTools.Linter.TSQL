using NUnit.Framework;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.LinterTests
{
    [Category("Linter.TSQL.Routines")]
    [TestOf(typeof(SystemProcDetector))]
    public sealed class SystemProcDetectorTests
    {
        private SystemProcDetector detector;

        [SetUp]
        public void SetUp()
        {
            detector = new SystemProcDetector();
        }

        [Test]
        public void TestSystemProcDetectorCatchesSysProcs()
        {
            Assert.That(detector.IsSystemProc(@"myproc"), Is.False, "myproc");
            Assert.That(detector.IsSystemProc(@"spBadName"), Is.False, "spNotTotallyBadName");

            Assert.That(detector.IsSystemProc(@"sp_executesql"), Is.True, "sp_executesql");
            Assert.That(detector.IsSystemProc(@"sys_dm_exec"), Is.True, "sys_dm_exec");
            Assert.That(detector.IsSystemProc(@"sysmail_"), Is.True, "sysmail_stopmail");
            Assert.That(detector.IsSystemProc(@"xp_startmail"), Is.True, "xp_startmail");
        }

        [Test]
        public void TestSystemProcDetectorReturnsFalseOnEmptyName()
        {
            Assert.That(detector.IsSystemProc(""), Is.False, "empty string");
            Assert.That(detector.IsSystemProc(null), Is.False, "null");
        }
    }
}
