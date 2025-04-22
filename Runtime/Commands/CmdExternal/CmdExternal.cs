using UnityEngine;

namespace _COBRA_
{
    internal static class CmdExternal
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoad()
        {
            Command.static_domain.AddAction(
                "run-external-command",
                min_args: 1,
                opts: static exe => exe.line.TryReadOption_workdir(exe),
                args: static exe =>
                {
                    if (exe.line.TryReadArgument(out string command_line, out _, lint: false))
                    {
                        exe.line.LintToThisPosition(exe.line.linter.external);
                        exe.args.Add(command_line);
                    }
                },
                action: static exe =>
                {
                    string command_line = (string)exe.args[0];
                    string workdir = exe.GetWorkdir();
                    Util.RunExternalCommand(workdir, command_line, on_stdout: stdout => exe.Stdout(stdout));
                },
                aliases: "."
            );
        }
    }
}