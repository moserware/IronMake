using System.Linq;

namespace IronMake {
    public static class Program {
        public static void Main(string[] args) {
            var makefile = MakefileParser.Parse();
            makefile.Make(args.Any() ? args.First() : "all");
        }
    }
}
