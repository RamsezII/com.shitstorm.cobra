using UnityEngine;

namespace _COBRA_
{
    internal static class CmdExternal
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoad()
        {
            const string
                flag_read_all = "--read-all",
                flag_r = "-r";

            Command.static_domain.AddAction(
                "run-external-command",
                min_args: 1,
                opts: static exe =>
                {
                    if (exe.line.TryRead_one_of_the_flags(exe, out string flag, flag_read_all, flag_r))
                        exe.opts.Add(flag_r, null);
                    exe.line.TryReadOption_workdir(exe);
                },
                args: static exe =>
                {
                    if (exe.opts.ContainsKey(flag_r))
                    {
                        if (exe.line.TryReadArguments(out string command_line))
                            exe.args.Add(command_line);
                    }
                    else
                    {
                        if (exe.line.TryReadArgument(out string command_line, out _, lint: false))
                        {
                            exe.line.LintToThisPosition(exe.line.linter.external);
                            exe.args.Add(command_line);
                        }
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