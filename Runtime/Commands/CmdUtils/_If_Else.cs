using System;
using System.Collections.Generic;

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
                    int read_i = exe.line.read_i;
                    if (Command.static_domain.TryReadCommand_path(exe.line, out var path1))
                    {
                        Command.Executor exe1 = new(exe.shell, exe, exe.line, path1);
                        if (exe1.error != null)
                            exe.error = exe1.error;
                        else
                            exe.args.Add(exe.line.text[read_i..exe.line.read_i]);
                    }
                    else
                        exe.error = $"command '{exe.command.name}' could not find command '{exe.line.arg_last}'";

                    if (exe.error != null)
                        return;

                    if (exe.line.TryRead_flags(exe, out var flags, flag_else))
                        if (flags.Contains(flag_else))
                        {
                            read_i = exe.line.read_i;
                            if (Command.static_domain.TryReadCommand_path(exe.line, out var path2))
                            {
                                Command.Executor exe2 = new(exe.shell, exe, exe.line, path2);
                                if (exe2.error != null)
                                    exe.error = exe2.error;
                                else
                                    exe.args.Add(exe.line.text[read_i..exe.line.read_i]);
                            }
                        }
                },
                on_pipe: static (exe, data) =>
                {
                    bool isTrue = data switch
                    {
                        bool b => b,
                        string str => str.Equals("true", StringComparison.OrdinalIgnoreCase),
                        int i => i != 0,
                        _ => false,
                    };

                    if (isTrue)
                    {
                        string cmd_line = (string)exe.args[0];
                        Command.Line line = new(cmd_line, exe.line.signal, exe.line.shell);

                        if (Command.static_domain.TryReadCommand_path(line, out var path))
                        {
                            Command.Executor exe1 = new(exe.shell, exe, line, path);
                            if (exe1.error != null)
                                exe.error = exe1.error;
                            else
                                exe.janitor.AddExecutor(exe.line, exe1);
                        }
                    }
                    else if (exe.args.Count > 1)
                    {
                        string cmd_line = (string)exe.args[1];
                        Command.Line line = new(cmd_line, exe.line.signal, exe.line.shell);

                        if (Command.static_domain.TryReadCommand_path(line, out var path))
                        {
                            Command.Executor exe1 = new(exe.shell, exe, line, path);
                            if (exe1.error != null)
                                exe.error = exe1.error;
                            else
                                exe.janitor.AddExecutor(exe.line, exe1);
                        }
                    }
                });
        }
    }
}