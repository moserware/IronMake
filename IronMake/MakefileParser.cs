using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace IronMake {
    public class MakefileParser {
        private static readonly Regex _Comment = new Regex("#.*$", RegexOptions.Singleline);
        private static readonly Regex _VariableDeclaration = new Regex(@"^\s*            # Variables declarations must be the first thing on a line
                                                                        (?<name>[^=]+)   # The name can be ANYTHING that isn't '=', which should allow lots of flexibility
                                                                        \s*
                                                                        =                # '=' separates declaration from value
                                                                        \s*
                                                                        (?<value>[^:=]*) # variable value
                                                                        \s*$", 
                                                                        RegexOptions.IgnorePatternWhitespace);

        private static readonly Regex _RuleDeclaration = new Regex(@"^(
                                                                        (\s+)                # A single rule can have multiple names since the recipe could generate multiple targets
                                                                        |
                                                                        (?<target>[^:\s]+)   
                                                                      )+
                                                                      :(?!=)                 # rules are separated by a ':' but it can't be := or else it'd look like a variable declaration
                                                                      \s*
                                                                      (
                                                                        (?<dependency>[^ ]+) # dependency names are anything that doesn't have a space (to allow almost anything)
                                                                        |
                                                                        [ ]
                                                                      )*$", RegexOptions.IgnorePatternWhitespace);

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
