using NUnit.Framework;

namespace IronMake.Tests {
    [TestFixture]
    public class VariableRepositoryTests {
        [Test]
        public void ResolveTest() {
            var repo = new VariableRepository();
            repo.Add("GREETING", "Hello");
            repo.Add("Subject", "World");
            Assert.AreEqual("Hello World!", repo.ResolveAnyVariableUsages("$(GREETING) $(SUBJECT)!"));
        }
    }
}
