using NUnit.Framework;

namespace IronMake.Tests {
    [TestFixture]
    public class ShellTests {
        [Test]
        public void ExecuteTest() {
            var shell = new Shell {
                RedirectStandardOutput = true
            };
            // Do a simple cmd.exe "ver" command:
            var verOutput = shell.Execute("ver");

            // Make sure we see our command and the version reply:
            Assert.IsTrue(verOutput.Contains("ver") && verOutput.Contains("Microsoft Windows [Version "));
        }
    }
}
