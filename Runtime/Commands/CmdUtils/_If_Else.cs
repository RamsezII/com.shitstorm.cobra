using System;
using System.Collections.Generic;
using UnityEngine;

namespace _COBRA_
{
    partial class CmdUtils
    {
        static void Init_If_Else()
        {
            const string
                flag_else = "--else";

            Command.static_domain.AddPipe(
                "not",
                on_pipe: static (exe, data) =>
                {
                    switch (data)
                    {
                        case bool bool_:
                            exe.Stdout(!bool_);
                            break;

                        case string str:
                            if (str.Equals("true", StringComparison.OrdinalIgnoreCase))
                                exe.Stdout(false);
                            else if (str.Equals("false", StringComparison.OrdinalIgnoreCase))
                                exe.Stdout(true);
                            else
                                exe.error = $"wrong argument '{str}'";
                            break;

                        default:
                            exe.error = $"wrong argument '{data}'";
                            break;
                    }
                },
                args: null);

            Command.static_domain.AddPipe(
                "if",
                manual: new("<command> {--else <command>}"),
                min_args: 1,
                max_args: 2,
                args: static exe =>
                {
                    int read_i = exe.signal.read_i;
                    if (Command.static_domain.TryReadCommand_path(exe.signal, out var path1))
                    {
                        Command.Executor exe1 = new(exe.shell, exe, exe.signal, path1);
                        if (exe1.error != null)
                            exe.error = exe1.error;
                        else
                            exe.args.Add(exe.signal.text[read_i..exe.signal.read_i]);
                    }
                    else
                        exe.error = $"command '{exe.command.name}' could not find command '{exe.signal.arg_last}'";

                    if (exe.error != null)
                        return;

                    if (exe.signal.TryRead_flags(exe, out var flags, flag_else))
                        if (flags.Contains(flag_else))
                        {
                            read_i = exe.signal.read_i;
                            if (Command.static_domain.TryReadCommand_path(exe.signal, out var path2))
                            {
                                Command.Executor exe2 = new(exe.shell, exe, exe.signal, path2);
                                if (exe2.error != null)
                                    exe.error = exe2.error;
                                else
                                    exe.args.Add(exe.signal.text[read_i..exe.signal.read_i]);
                            }
                        }
                },
                on_pipe: static (exe, data) =>
                {
                    bool isTrue;
                    switch (data)
                    {
                        case bool _bool:
                            isTrue = _bool;
                            break;

                        case string _string:
                            isTrue = _string.Equals("true", StringComparison.OrdinalIgnoreCase);
                            break;

                        case int _int:
                            isTrue = _int != 0;
                            break;

                        default:
                            isTrue = false;
                            exe.error = $"received wrong argument '{data}'.";
                            break;
                    }

                    if (isTrue)
                    {
                        string cmd_line = (string)exe.args[0];
                        Command.Signal signal = new(cmd_line, exe.signal.flags, exe.signal.shell);

                        if (Command.static_domain.TryReadCommand_path(signal, out var path))
                        {
                            Command.Executor exe1 = new(exe.shell, exe, signal, path);
                            if (exe1.error != null)
                                exe.error = exe1.error;
                            else
                                exe.janitor.AddExecutor(exe.signal, exe1);
                        }
                    }
                    else if (exe.args.Count > 1)
                    {
                        string cmd_line = (string)exe.args[1];
                        Command.Signal signal = new(cmd_line, exe.signal.flags, exe.signal.shell);

                        if (Command.static_domain.TryReadCommand_path(signal, out var path))
                        {
                            Command.Executor exe2 = new(exe.shell, exe, signal, path);
                            if (exe2.error != null)
                                exe.error = exe2.error;
                            else
                                exe.janitor.AddExecutor(exe.signal, exe2);
                        }
                    }
                });
        }
    }
}