using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace IronMake {
    public class MakefileParser {
        private static readonly Regex _Comment = new Regex("#.+$", RegexOptions.Singleline);
        private static readonly Regex _VariableDeclaration = new Regex(@"^\s*(?<name>[^=]+)\s*=\s*(?<value>[^:=]*)\s*$");
        private static readonly Regex _RuleDeclaration = new Regex(@"^((\s+)|(?<target>[^:\s]+))+:(?!=)\s*((?<dependency>[^ ]+)|[ ])*$");

        public static Makefile Parse() {
            return Parse("Makefile");
        }

        public static Makefile Parse(string path) {
            return Parse(File.ReadLines(path));
        }
        
        public static Makefile Parse(IEnumerable<string> lines) {
            Rule currentRule = null;
            var result = new Makefile();
            var currentLineNumber = 0;

            foreach (var currentLine in lines) {
                currentLineNumber++;
                var lineWithoutComments = StripComments(currentLine).TrimEnd();

                if (String.IsNullOrWhiteSpace(lineWithoutComments)) {
                    continue;
                }

                if (currentRule != null) {
                    // Recipes start with a tab
                    if (lineWithoutComments.StartsWith("\t")) {
                        currentRule.Recipe.Add(lineWithoutComments.Substring(1));
                        continue;
                    }

                    currentRule = null;
                }

                var variableDeclarationMatch = _VariableDeclaration.Match(lineWithoutComments);
                if (variableDeclarationMatch.Success) {
                    result.Variables.Add(variableDeclarationMatch.Groups["name"].Value.Trim(), variableDeclarationMatch.Groups["value"].Value.Trim());
                    continue;
                }

                var ruleDeclarationMatch = _RuleDeclaration.Match(lineWithoutComments);
                if (ruleDeclarationMatch.Success) {
                    currentRule = new Rule(ruleDeclarationMatch.Groups["target"].Captures.Cast<Capture>().Select(c => c.Value),
                                           ruleDeclarationMatch.Groups["dependency"].Captures.Cast<Capture>().Select(c => c.Value));
                    result.Rules.Add(currentRule);
                    continue;
                }

                throw new Exception(String.Format("Unrecognized command on line {0}", currentLineNumber));
            }

            return result;
        }

        private static string StripComments(string line) {
            return _Comment.Replace(line, "");
        }
    }
}
