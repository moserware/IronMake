using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace IronMake {
    public class Makefile {
        public readonly IList<Rule> Rules = new List<Rule>();
        public readonly VariableRepository Variables = new VariableRepository();
        private readonly Lazy<Shell> _Shell; 

        public Makefile() {
            _Shell = new Lazy<Shell>(() => new Shell(Variables.ResolveVariable("SHELL")));
        }

        public string Make(string target) {
            var targetToRule = new Dictionary<string, Rule>(StringComparer.OrdinalIgnoreCase);
            var executedRules = new HashSet<Rule>();

            foreach(var currentRule in Rules) {
                currentRule.ResolveVariables(Variables);

                foreach (var currentTargetName in currentRule.Targets) {
                    targetToRule[currentTargetName] = currentRule;
                }
            }

            var topLevelOutput = Make(target, GetTargetLastWriteTime(target, targetToRule), targetToRule, executedRules, Shell);
            if (topLevelOutput == null) {
                var nothingToBeDone = String.Format("Nothing to be done for `{0}'", target);

                if (Shell.RedirectStandardOutput) {
                    return nothingToBeDone;
                }
                else {
                    Console.WriteLine(nothingToBeDone);
                }
            }

            return topLevelOutput;
        }

        public Shell Shell {
            get { return _Shell.Value; }
        }

        private string Make(string target, DateTime? parentLastWriteTime, Dictionary<string, Rule> targetToRule, HashSet<Rule> executedRules, Shell shell) {
            Rule currentRule;
            if (!targetToRule.TryGetValue(target, out currentRule)) {
                throw new Exception(String.Format("Don't know how to make '{0}'", target));
            }

            if (executedRules.Contains(currentRule)) {
                // Don't do it again!
                return null;
            }

            if (!executedRules.Contains(currentRule)) {
                executedRules.Add(currentRule);
            }

            // Check the file system to see if the target already exists if it's not phony
            var targetLastWrite = GetTargetLastWriteTime(target, targetToRule);

            var dependencyWasMade = false;

            var output = new StringBuilder();

            foreach (var currentDependency in currentRule.Dependencies) {
                var currentDependencyOutput = Make(currentDependency, targetLastWrite, targetToRule, executedRules, shell);
                var currentDependencyWasMade = currentDependencyOutput != null;
                dependencyWasMade |= currentDependencyWasMade;
                if (currentDependencyWasMade) {
                    output.Append(currentDependencyOutput);
                }
            }

            if (!dependencyWasMade) {
                if (targetLastWrite != null) {
                    if (parentLastWriteTime != null) {
                        if (targetLastWrite.Value <= parentLastWriteTime.Value) {
                            // dependency is older than target, which means we shouldn't have to bother redoing it
                            return null;
                        }
                    }
                    else {
                        // If the parent doesn't exist, but we exist (as a dependency), then the existing version is fine
                        // and we don't have to recreate it.
                        return null;
                    }
                }
            }

            if (currentRule.Recipe.Any()) {
                var currentRecipeOutput = shell.Execute(currentRule.Recipe);
                if (currentRecipeOutput != null) {
                    output.Append(currentRecipeOutput);
                }

                return output.ToString();
            }

            return dependencyWasMade ? output.ToString() : null;
        }

        // "Phony" means to ignore an identically named file (e.g. to still make "all" even if there is a file called "all")
        private static bool IsPhony(string target, Dictionary<string, Rule> targetToRule) {
            Rule phonyRule;
            return targetToRule.TryGetValue(".PHONY", out phonyRule) && phonyRule.Dependencies.Contains(target, StringComparer.OrdinalIgnoreCase);
        }

        private static DateTime? GetTargetLastWriteTime(string target, Dictionary<string, Rule> targetToRule) {
            if (IsPhony(target, targetToRule)) {
                return null;
            }

            var info = new FileInfo(GetCanonicalizedPath(target));
            if (!info.Exists) {
                return null;
            }

            return info.LastWriteTime;
        }

        private static string GetCanonicalizedPath(string target) {
            return Path.GetFullPath(target);
        }
    }

    public class VariableRepository {
        private static readonly Regex _VariableUsage = new Regex(@"\$                       # Starts ref as in $(VARNAME)
                                                                    \(
                                                                        \s*
                                                                        (?<name>[^$\(\)]*)  # We don't allow inner ('s because of how we resolve things from most inside to out. This allows nesting
                                                                    \)", 
                                                                    RegexOptions.IgnorePatternWhitespace);
        private readonly Dictionary<string, string> _NameToValue = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase); 

        public void Add(string name, string value) {
            _NameToValue[name] = value;
        }

        public string ResolveAnyVariableUsages(string variableUsage) {
            // To allow nesting, we run until convergence
            var resolvedUsage = variableUsage;
            while (true) {
                var replaced = _VariableUsage.Replace(resolvedUsage, m => ResolveVariable(m.Groups["name"].Value.Trim()));
                if (replaced == resolvedUsage) {
                    break;
                }
                resolvedUsage = replaced;
            }

            return resolvedUsage;
        }

        public string ResolveVariable(string name) {
            string resolvedVariable;
            if (_NameToValue.TryGetValue(name, out resolvedVariable)) {
                return ResolveAnyVariableUsages(resolvedVariable);
            }

            return Environment.GetEnvironmentVariable(name) ?? "";
        }
    }

    public class Rule {
        public Rule(IEnumerable<string> targets, IEnumerable<string> dependencies) {
            Recipe = new List<string>();
            Targets = targets.ToList();
            Dependencies = dependencies.ToList();
        }

        public IList<string> Recipe { get; private set; }
        public IList<string> Targets { get; private set; }
        public IList<string> Dependencies { get; private set; }

        public void ResolveVariables(VariableRepository variables) {
            foreach (var currentList in new[] {Targets, Dependencies, Recipe}) {
                for (int i = 0; i < currentList.Count; i++) {
                    currentList[i] = variables.ResolveAnyVariableUsages(currentList[i]);
                }
            }
        }
    }
}
