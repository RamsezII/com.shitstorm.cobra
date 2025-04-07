using System.Collections.Generic;

namespace _COBRA_
{
    partial class CmdUtils
    {
        static void InitEdit()
        {
            const string flag_force_file = "force-file";

            Command.cmd_root_shell.AddCommand(new(
                "edit-file",
                action_min_args_required: 1,
                args: static exe =>
                {
                    const string flag_force = "--create-if-not-found";
                    if (exe.line.TryReadFlags(exe, out var opts, flag_force))
                        if (opts.Contains(flag_force))
                            exe.opts.Add(flag_force_file);

                    // read file path
                    // (reparer tab avant)
                },
                routine: ERoutine
                ));

            static IEnumerator<STDIN_INFOS> ERoutine(Command.Executor exe)
            {
                try
                {
                    while (true)
                    {
                        switch (exe.line.signal)
                        {
                            case CMD_SIGNALS.SAVE:
                                exe.Stdout("Saving file...");
                                yield break;
                        }
                        yield return new(CMD_STATES.FULLSCREEN_write);
                    }
                }
                finally
                {
                    exe.Stdout("cleaning");
                }
            }
        }
    }
}