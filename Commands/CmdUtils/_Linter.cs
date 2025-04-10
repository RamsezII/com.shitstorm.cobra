using System;
using UnityEngine;

namespace _COBRA_
{
    partial class CmdUtils
    {
        static void InitLinter()
        {
            Command.cmd_root_shell.AddCommand(new(
                "linter-test",
                args: exe =>
                {
                    var dict = exe.line.linter.GetColorProperties();
                    if (exe.line.TryReadOptions(exe, new(StringComparer.OrdinalIgnoreCase) { { "--option", new[] { "yes", "no", "maybe", } } }, out var output))
                        ;

                    while (exe.line.TryReadArgument(out string arg, dict.Keys, lint: false))
                        if (dict.TryGetValue(arg, out Color color))
                            exe.line.LintToThisPosition(color);
                        else
                            exe.line.LintToThisPosition(exe.line.linter.error);
                }
                ));
        }
    }
}