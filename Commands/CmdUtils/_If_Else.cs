using System;

namespace _COBRA_
{
    partial class CmdUtils
    {
        static void Init_If_Else()
        {
            const string
                flag_else = "--else";

            Command.cmd_root_shell.AddCommand(new(
                "not",
                on_pipe: static (exe, args, data) =>
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
                }));

            Command.cmd_root_shell.AddCommand(new(
                "if",
                manual: new("<command> {--else <command>}"),
                pipe_min_args_required: 1,
                args: static exe =>
                {
                    if (Command.cmd_root_shell.TryReadCommand_path(exe.line, out var cmd1_path))
                    {
                        Command cmd = cmd1_path[^1].Value;
                        string cmd_name = cmd1_path[^1].Key;

                        Command.Executor exe1 = new(exe.root, cmd1_path, exe.line);
                        if (exe1.error != null)
                            exe.error = exe1.error;
                        else
                            exe.args.Add(exe1);
                    }
                    else
                        exe.error = $"command '{exe.cmd_name}' could not find command '{exe.line.arg_last}'";

                    if (exe.error != null)
                        return;

                    if (exe.line.TryReadFlags(exe, out var flags, flag_else))
                        if (flags.Contains(flag_else))
                            if (Command.cmd_root_shell.TryReadCommand_path(exe.line, out var cmd2_path))
                            {
                                Command cmd = cmd2_path[^1].Value;
                                string cmd_name = cmd2_path[^1].Key;

                                Command.Executor exe2 = new(exe.root, cmd2_path, exe.line);
                                if (exe2.error != null)
                                    exe.error = exe2.error;
                                else
                                    exe.args.Add(exe2);
                            }
                },
                on_pipe: static (exe, args, data) =>
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
                        exe1.Executate(exe.line);
                    else if (exe.args.Count > 1)
                    {
                        Command.Executor exe2 = (Command.Executor)exe.args[1];
                        exe2.Executate(exe.line);
                    }
                }
                ));
        }
    }
}