using System;
using System.Collections.Generic;
using System.IO;
using _UTIL_;
using UnityEngine;

namespace _COBRA_
{
    partial class Boa
    {
        enum BoaSymbols
        {
            label,
            jump_to,
            jump_if,
            jump_else,
        }

        //--------------------------------------------------------------------------------------------------------------

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

                // catch symbols
                for (int line_i = 0; line_i < script_lines.Length; line_i++)
                {
                    string script_line = script_lines[line_i];
                    if (!string.IsNullOrWhiteSpace(script_line))
                    {
                        int read_i = 0;

                        if (Util_cobra.TryReadArgument(script_line, out int start_i, ref read_i, out string arg0, true))
                            if (arg0.StartsWith(':'))
                            {
                                string symbol = arg0[1..];
                                if (Enum.TryParse(symbol, true, out BoaSymbols code))
                                    switch (code)
                                    {
                                        case BoaSymbols.label:
                                            if (Util_cobra.TryReadArgument(script_line, out start_i, ref read_i, out string label, true))
                                                labels[label] = line_i;
                                            break;

                                        case BoaSymbols.jump_if:
                                            if (Util_cobra.TryReadArgument(script_line, out start_i, ref read_i, out label, true))
                                                //if(Util_cobra)
                                                ;
                                            break;

                                        case BoaSymbols.jump_else:
                                            break;

                                        case BoaSymbols.jump_to:
                                            break;
                                    }
                            }
                            else
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
                                    exe.error = $"could not find command ({nameof(line)}.{nameof(line.arg_last)}: '{line.arg_last}')";
                                    yield break;
                                }
                            }
                    }
                }
            }
        }
    }
}