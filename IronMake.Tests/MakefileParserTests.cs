using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace IronMake.Tests {
    [TestClass]
    public class MakefileParserTests {
        [TestMethod]
        public void TestMakefilesDirectory() {
            foreach (var currentMakefile in Directory.GetFiles(MakefilesPath, "*.makefile")) {
                TestMakefile(currentMakefile);
            }
        }
        private void TestMakefile(string path) {
            Environment.CurrentDirectory = Path.GetDirectoryName(path);
            var makefile = MakefileParser.Parse(path);
            makefile.Make("all");

            // TODO: Get results and compare with the corresponding .Expected file
            throw new NotImplementedException();
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
