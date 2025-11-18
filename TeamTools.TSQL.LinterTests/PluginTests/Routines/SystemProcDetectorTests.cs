using NUnit.Framework;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.LinterTests
{
    [Category("Linter.TSQL.Routines")]
    [TestOf(typeof(SystemProcDetector))]
    public sealed class SystemProcDetectorTests
    {
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void TestSystemProcDetectorCatchesSysProcs()
        {
            Assert.That(IsSystemProc("myproc"), Is.False, "myproc");
            Assert.That(IsSystemProc("spBadName"), Is.False, "spNotTotallyBadName");

            Assert.That(IsSystemProc("sp_executesql"), Is.True, "sp_executesql");
            Assert.That(IsSystemProc("sys_dm_exec"), Is.True, "sys_dm_exec");
            Assert.That(IsSystemProc("sysmail_"), Is.True, "sysmail_stopmail");
            Assert.That(IsSystemProc("xp_startmail"), Is.True, "xp_startmail");
        }

        [Test]
        public void TestSystemProcDetectorReturnsFalseOnEmptyName()
        {
            Assert.That(IsSystemProc(""), Is.False, "empty string");
            Assert.That(IsSystemProc(null), Is.False, "null");
        }

        private static bool IsSystemProc(string name) => SystemProcDetector.IsSystemProc(name);
    }
}
