using NUnit.Framework;

namespace IronMake.Tests {
    [TestFixture]
    public class VariableRepositoryTests {
        [Test]
        public void ResolveTest() {
            var repo = new VariableRepository();
            repo.Add("GREETING", "Hello");
            repo.Add("Subject", "World");
            repo.Add("FIRSTVAR", "GREETING");
            Assert.AreEqual("Hello", repo.ResolveVariable("GREETING"));
            Assert.AreEqual("Hello", repo.ResolveAnyVariableUsages("$(GREETING)"));
            Assert.AreEqual("Hello World!", repo.ResolveAnyVariableUsages("$(GREETING) $(SUBJECT)!"));
            Assert.AreEqual("Hello World!", repo.ResolveAnyVariableUsages("$($(FIRSTVAR)) $(SUBJECT)!"));
        }
    }
}
