using NUnit.Framework;
using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace IronMake.Tests {
    [TestFixture]
    public class MakefileParserTests {
        // Rather than spend time making a bunch of mocks, lets just use the file system directly with our own isolated subdirs

        [Test]
        public void TestMakefilesDirectory() {
            var tempPathRoot = Path.Combine(Path.GetTempPath(), "IronMake");
            var unitTestsRoot = Path.Combine(tempPathRoot, "UnitTests");

            if (Directory.Exists(tempPathRoot)) {
                Directory.Delete(tempPathRoot, recursive: true);
            }

            foreach (var currentTestCaseGrouping in Directory.GetFiles(MakefilesPath, "*.makefile").GroupBy(GetTestCaseCoreName)) {
                // Run each test case group in its own private directory
                var tempFolderName = String.Format(CultureInfo.InvariantCulture, "{0} - {1:yyyy-MM-dd} {1:HH-mm-ss}", currentTestCaseGrouping.Key, DateTime.Now);

                var fullPath = Path.Combine(unitTestsRoot, tempFolderName);
                
                Directory.CreateDirectory(fullPath);
                Environment.CurrentDirectory = fullPath;

                foreach (var currentTestCase in currentTestCaseGrouping.OrderBy(p => p)) {
                    TestMakefile(currentTestCase);
                }
            }
        }

        private static string GetTestCaseCoreName(string path) {
            var filePart = Path.GetFileNameWithoutExtension(path);

            // Split things up with underscores so we can have
            // TestName_2_Comment
            var subParts = filePart.Split('_');
            return subParts[0];
        }

        private void TestMakefile(string path) {
            var expectedFile = Path.ChangeExtension(path, ".Expected");
            var makefile = MakefileParser.Parse(path);
            makefile.Shell.RedirectStandardOutput = true;
            var result = (makefile.Make("all") ?? "").Trim();

            if (File.Exists(expectedFile)) {
                var expectedContents = File.ReadAllText(expectedFile).Trim();
                Assert.AreEqual(expectedContents, result, "Output of '{0}' test case did not match", Path.GetFileNameWithoutExtension(path));
            }
        }

        private string MakefilesPath {
            get {
                var type = typeof(MakefileParserTests);
                var assembly = type.Assembly;
                var dir = new DirectoryInfo(Path.GetDirectoryName(new Uri(assembly.CodeBase).AbsolutePath));
                while (dir.Name != type.Namespace) {
                    dir = dir.Parent;
                }

                return Path.Combine(dir.FullName, "Makefiles");
            }
        }
    }
}
