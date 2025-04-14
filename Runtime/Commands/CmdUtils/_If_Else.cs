using System;

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
                    if (Command.static_domain.TryReadCommand_path(exe.line, out var cmd1_path))
                    {
                        Command cmd = cmd1_path[^1];
                        Command.Executor exe1 = new(exe.shell, exe, exe.line, cmd1_path);

                        if (exe1.error != null)
                            exe.error = exe1.error;
                        else
                            exe.args.Add(exe1);
                    }
                    else
                        exe.error = $"command '{exe.command.name}' could not find command '{exe.line.arg_last}'";

                    if (exe.error != null)
                        return;

                    if (exe.line.TryRead_flags(exe, out var flags, flag_else))
                        if (flags.Contains(flag_else))
                            if (Command.static_domain.TryReadCommand_path(exe.line, out var cmd2_path))
                            {
                                Command cmd = cmd2_path[^1];
                                Command.Executor exe2 = new(exe.shell, exe, exe.line, cmd2_path);

                                if (exe2.error != null)
                                    exe.error = exe2.error;
                                else
                                    exe.args.Add(exe2);
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

                    Command.Executor exe1 = (Command.Executor)exe.args[0];

                    if (isTrue)
                        exe.janitor.AddExecutor(exe1);
                    else if (exe.args.Count > 1)
                    {
                        Command.Executor exe2 = (Command.Executor)exe.args[1];
                        exe.janitor.AddExecutor(exe2);
                    }
                }
                );
        }
    }
}