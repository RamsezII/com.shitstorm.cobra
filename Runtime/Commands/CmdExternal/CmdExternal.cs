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
                opts: static exe => exe.line.TryReadWorkingDir(exe),
                args: static exe =>
                {
                    if (exe.line.TryReadArgument(out string arg0, out _, lint: false))
                    {
                        exe.args.Add(arg0);
                        exe.line.LintToThisPosition(exe.line.linter._readall_);
                    }
                },
                action: static exe =>
                {
                    string command_line = (string)exe.args[0];
                    string workdir = exe.shell.working_dir;
                    if (exe.opts.TryGetValue(Command.Line.opt_workdir, out object _val))
                        workdir = (string)_val;
                    Util.RunExternalCommand(workdir, command_line, on_stdout: stdout => exe.Stdout(stdout));
                },
                aliases: "."
            );
        }
    }
}