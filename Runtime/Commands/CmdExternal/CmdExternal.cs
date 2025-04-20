using UnityEngine;

namespace _COBRA_
{
    internal static class CmdExternal
    {
        static Command cmd_ext;

        //--------------------------------------------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoad()
        {
            cmd_ext = Command.static_domain.AddAction(
                "run-external-command",
                min_args: 1,
                opts: static exe => exe.line.TryReadOption_workdir(exe),
                args: static exe =>
                {
                    if (exe.line.TryReadArguments(out string command_line))
                        exe.args.Add(command_line);
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