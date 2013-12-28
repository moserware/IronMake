using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IronMake.Tests {
    [TestClass]
    public class ShellTests {
        [TestMethod]
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
