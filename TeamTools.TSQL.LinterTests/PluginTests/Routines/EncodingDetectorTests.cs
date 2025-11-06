using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Text;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.LinterTests
{
    [Category("Linter.TSQL.Routines")]
    [TestOf(typeof(EncodingDetector))]
    public sealed class EncodingDetectorTests
    {
        private MockEncodingDetector detector;

        [SetUp]
        public void SetUp()
        {
            detector = new MockEncodingDetector();
        }

        [Test]
        public void TestEncodingDetector()
        {
            byte[] bytes;

            bytes = Encoding.ASCII.GetBytes(@"asdf");
            Assert.That(detector.GetStringEncoding(bytes), Is.EqualTo(Encoding.ASCII), "ascii");

            bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(@"asfd")).ToArray();
            Assert.That(detector.GetStringEncoding(bytes), Is.EqualTo(Encoding.UTF8), "utf8-bom");

            bytes = Encoding.UTF32.GetPreamble().Concat(Encoding.UTF32.GetBytes(@"asfd")).ToArray();
            Assert.That(detector.GetStringEncoding(bytes), Is.EqualTo(Encoding.UTF32), "utf32");

            bytes = Encoding.Unicode.GetPreamble().Concat(Encoding.Unicode.GetBytes(@"asfd")).ToArray();
            Assert.That(detector.GetStringEncoding(bytes), Is.EqualTo(Encoding.Unicode), "utf16");

            Assert.Throws<FileNotFoundException>(() => detector.GetFileEncoding("missing file"), "missing file fails");
        }

        [Test]
        public void TestFileEncodingDetector()
        {
            try
            {
                using StreamWriter file = new StreamWriter("./TestFileEncodingDetector.txt", false, Encoding.Unicode);
                file.Write("test");
                file.Close();

                Assert.That(detector.GetFileEncoding("./TestFileEncodingDetector.txt"), Is.EqualTo(Encoding.Unicode));

                File.Delete("./TestFileEncodingDetector.txt");
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        private class MockEncodingDetector : EncodingDetector
        {
            public Encoding GetStringEncoding(byte[] value)
            {
                var stream = new MemoryStream(value);
                stream.Seek(0, System.IO.SeekOrigin.Begin);

                return GetFileEncoding(stream);
            }
        }
    }
}
