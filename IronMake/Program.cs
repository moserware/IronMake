using System;
using System.Linq;

namespace IronMake {
    public static class Program {
        public static void Main(string[] args) {
            try {
                var makefile = MakefileParser.Parse();
                makefile.Make(args.Any() ? args.First() : "all");
            } catch(Exception exception) {
                Console.Error.WriteLine("{0}. Stop.", exception.Message);
            }
        }
    }
}
