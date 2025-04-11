using System.Collections.Generic;

namespace _COBRA_
{
    partial class CmdUtils
    {
        static void InitEdit()
        {
            const string flag_force_file = "--create-if-not-found";

            Shell.static_domain.AddRoutine(
                "edit-file",
                manual: new("create and edit a file"),
                min_args: 1,
                args: static exe =>
                {
                    if (exe.line.TryReadFlags(exe, out var opts, flag_force_file))
                        if (opts.Contains(flag_force_file))
                            exe.opts.Add(flag_force_file, null);

                    // read file path
                    // (reparer tab avant)
                },
                routine: ERoutine
                );

            static IEnumerator<CMD_STATUS> ERoutine(Command.Executor exe)
            {
                try
                {
                    while (true)
                    {
                        if (exe.line.signal.HasFlag(SIGNAL_FLAGS.SAVE))
                            exe.Stdout("Saving file...");
                        yield return new(CMD_STATES.FULLSCREEN_write);
                    }
                }
                finally
                {
                    exe.Stdout("cleaning after closing file");
                }
            }
        }
    }
}