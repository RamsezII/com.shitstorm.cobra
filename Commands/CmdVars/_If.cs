using System;
using System.Collections.Generic;

namespace _COBRA_
{
    partial class CmdVars
    {
        static void InitIf()
        {
            Command.cmd_root_shell.AddCommand("if", new Command(
                manual: new("<if: {pipe|var}> <then: command> [else: command]"),
                init_min_args_required: 1,
                args: static exe =>
                {
                    if (Command.cmd_root_shell.TryReadCommand_path(exe.line, out var cmd1_path))
                        exe.args.Add(cmd1_path[^1]);

                    if (exe.line.TryReadArgument(out string _else, new[] { "else", }))
                    {
                        _else = _else.Trim();
                        if (!_else.Equals("else", StringComparison.OrdinalIgnoreCase))
                            exe.error = $"wrong argument '{_else}'";
                        else if (Command.cmd_root_shell.TryReadCommand_path(exe.line, out var cmd2_path))
                            exe.args.Add(cmd2_path[^1]);
                        else
                            exe.error = $"missing command after '{_else}'";
                    }
                },
                on_data: static (exe, data) =>
                {
                    bool isTrue = (bool)data;
                    var cmd1 = (KeyValuePair<string, Command>)exe.args[0];

                    if (isTrue)
                        if (cmd1.Value.on_data == null)
                            exe.error = $"command '{cmd1.Key}' has no {nameof(cmd1.Value.on_data)} callback";
                        else
                            cmd1.Value.on_data(exe, data);
                    else if (exe.args.Count > 1)
                    {
                        var cmd2 = (KeyValuePair<string, Command>)exe.args[1];
                        if (cmd2.Value.on_data == null)
                            exe.error = $"command '{cmd2.Key}' has no {nameof(cmd2.Value.on_data)} callback";
                        else
                            cmd2.Value.on_data(exe, data);
                    }
                }
                ));
        }
    }
}