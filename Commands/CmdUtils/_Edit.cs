using System.Collections.Generic;

namespace _COBRA_
{
    partial class CmdUtils
    {
        static void InitEdit()
        {
            Command.cmd_root_shell.AddCommand(new(
                "edit-file",
                action_min_args_required: 1,
                args: static exe =>
                {
                    // option: force file
                    if (exe.line.TryReadFlags(exe, out var opts, "--create-if-not-found"))
                        exe.args.Add(true);
                    else
                        exe.args.Add(false);
                },
                routine: ERoutine
                ));

            static IEnumerator<CMD_STATUS> ERoutine(Command.Executor exe)
            {
                while (true)
                {
                    yield return new()
                    {
                        state = CMD_STATES.WAIT_FOR_STDIN,
                        prefixe = $"{exe.cmd_name} ({exe.cmd_path})",
                    };
                }
            }
        }
    }
}