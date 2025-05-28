using System;
using UnityEngine;

namespace _COBRA_
{
    partial class CmdUtils
    {
        static void InitLinter()
        {
            Command.static_domain.AddAction(
                "linter-test",
                action: null,
                max_args: 20,
                args: exe =>
                {
                    var dict = exe.signal.linter.GetColorProperties();
                    if (exe.signal.TryRead_options_with_allowed_values(exe, new(StringComparer.OrdinalIgnoreCase) { { "--option", new[] { "yes", "no", "maybe", } } }, out var output))
                        ;

                    while (exe.signal.TryReadArgument(out string arg, out bool is_candidate, dict.Keys, lint: false))
                        if (dict.TryGetValue(arg, out Color color))
                            exe.signal.LintToThisPosition(color);
                        else
                            exe.signal.LintToThisPosition(exe.signal.linter.error);
                });
        }
    }
}