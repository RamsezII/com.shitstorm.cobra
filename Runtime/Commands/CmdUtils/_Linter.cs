using System;
using UnityEngine;

namespace _COBRA_
{
    partial class CmdUtils
    {
        static void InitLinter()
        {
            Shell.static_domain.AddAction(
                "linter-test",
                action: null,
                max_args: 20,
                args: exe =>
                {
                    var dict = exe.line.linter.GetColorProperties();
                    if (exe.line.TryRead_options_with_allowed_values(exe, new(StringComparer.OrdinalIgnoreCase) { { "--option", new[] { "yes", "no", "maybe", } } }, out var output))
                        ;

                    while (exe.line.TryReadArgument(out string arg, dict.Keys, lint: false))
                        if (dict.TryGetValue(arg, out Color color))
                            exe.line.LintToThisPosition(color);
                        else
                            exe.line.LintToThisPosition(exe.line.linter.error);
                });
        }
    }
}