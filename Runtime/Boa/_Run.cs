using System;
using System.Collections.Generic;
using System.IO;
using _UTIL_;

namespace _COBRA_
{
    partial class Boa
    {
        static void Init_Run()
        {
            cmd_boa.AddRoutine(
                "run",
                manual: new("execute script at path <path>"),
                min_args: 1,
                args: exe =>
                {
                    if (exe.line.TryReadArgument(out string script_path, out _, strict: true, path_mode: FS_TYPES.FILE))
                        exe.args.Add(script_path);
                },
                routine: ERun);

            static IEnumerator<CMD_STATUS> ERun(Command.Executor exe)
            {
                string script_path = (string)exe.args[0];
                script_path = exe.shell.PathCheck(script_path, PathModes.ForceFull);

                Dictionary<string, int> labels = new(StringComparer.Ordinal);
                string[] script_lines = File.ReadAllLines(script_path);

                for (int i = 0; i < script_lines.Length; i++)
                {
                    string script_line = script_lines[i];
                    if (!string.IsNullOrWhiteSpace(script_line))
                    {
                        Command.Line line = new(script_line, exe.line.flags, exe.shell, cursor_i: int.MaxValue);
                        if (Command.static_domain.TryReadCommand_path(line, out var path))
                        {
                            Command.Executor exe2 = new(exe.shell, exe, line, path);
                            if (exe2.error != null)
                            {
                                exe.error = exe2.error;
                                yield break;
                            }

                            exe.janitor.AddExecutor(exe2);

                            while (!exe2.disposed)
                                yield return exe.janitor.exe_status;
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
}