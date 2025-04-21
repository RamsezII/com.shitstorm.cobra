using System.Collections.Generic;
using System.IO;

namespace _COBRA_
{
    partial class CmdBoa
    {
        static void Init_Run()
        {
            cmd_boa.AddRoutine(
                "run",
                manual: new("execute script at path <path>"),
                min_args: 1,
                args: exe =>
                {
                    if (exe.line.TryReadArgument(out string script_path, out _, strict: true, path_mode: PATH_FLAGS.FILE))
                        exe.args.Add(script_path);
                },
                routine: ERun);

            static IEnumerator<CMD_STATUS> ERun(Command.Executor exe)
            {
                string script_path = (string)exe.args[0];
                script_path = exe.shell.PathCheck(script_path, PathModes.ForceFull);

                string script_text = File.ReadAllText(script_path);

                List<int> stack_starts = new();
                foreach (string script_line in script_text.IterateThroughData_str())
                {
                    Command.Line cmd_line = new(script_line, exe.line.signal, exe.shell, cursor_i: int.MaxValue);
                    if (Command.static_domain.TryReadCommand_path(cmd_line, out var path))
                    {
                        Command.Executor exe2 = new(exe.shell, exe, cmd_line, path);
                        if (exe2.error != null)
                        {
                            exe.error = exe2.error;
                            yield break;
                        }

                        exe.janitor.AddExecutor(exe.line, exe2);

                        while (!exe2.disposed)
                            yield return exe2.routine.Current;
                    }
                    else
                    {
                        exe.error = $"could not find command ({nameof(exe.line.arg_last)}: {exe.line.arg_last})";
                        yield break;
                    }
                }
            }
        }
    }
}