using UnityEngine;

namespace _COBRA_
{
    partial class CmdUtils
    {
        static void InitLinter()
        {
            Command cmd_gui = Command.cmd_root_shell.AddCommand(new("gui"));

            cmd_gui.AddCommand(new(
                "linter-test",
                args: exe =>
                {
                    var dict = exe.line.linter.GetColorProperties();
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