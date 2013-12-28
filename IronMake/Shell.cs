using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace IronMake {
    public class Shell {
        private static readonly string DefaultShell = Environment.GetEnvironmentVariable("COMSPEC") ?? "cmd.exe";

        public Shell(string shellPath = null) {
            ShellPath = !String.IsNullOrWhiteSpace(shellPath) ? shellPath : DefaultShell;
        }

        public bool RedirectStandardOutput { get; set; }

        private string ShellPath { get; set; }

        public string Execute(string command) {
            return Execute(new[] {command});
        }

        public string Execute(IEnumerable<string> commands) {
            var startInfo = new ProcessStartInfo(ShellPath.Replace("/", "\\")) {
                UseShellExecute = false,
                RedirectStandardOutput = RedirectStandardOutput,
                ErrorDialog = false,
                CreateNoWindow = true
            };

            var executionOutput = new StringBuilder();

            foreach (var currentCommand in commands) {
                var actualCommand = currentCommand;

                if (actualCommand.StartsWith("@")) {
                    // indicates that the command should not be echoed
                    actualCommand = actualCommand.Substring(1);
                }
                else {
                    if (RedirectStandardOutput) {
                        executionOutput.AppendLine(actualCommand);
                    }
                    else {
                        Console.WriteLine(actualCommand);
                    }
                }

                // Make gives each command its own shell process

                // HACK: just support cmd.exe style for now
                //       the /S allows us to now care about inner quotes

                startInfo.Arguments = "/S /C \"" + actualCommand + "\"";

                using (var process = Process.Start(startInfo)) {
                    if (process != null) {
                        process.WaitForExit();

                        if (RedirectStandardOutput) {
                            executionOutput.Append(process.StandardOutput.ReadToEnd());
                        }

                        if (process.ExitCode != 0) {
                            throw new Exception(String.Format("Unsuccessful: {0}", process.ExitCode));
                        }
                    }
                }
            }

            return executionOutput.ToString();
        }
    }
}
