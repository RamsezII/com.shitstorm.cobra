using System.Collections.Generic;
using System.Linq;

namespace _COBRA_
{
    partial class CmdUtils
    {
        static void Init_Args()
        {
            Command.cmd_root_shell.AddCommand(new Command(
                "args",
                pipe_min_args_required: 1,
                args: static exe =>
                {
                    if (exe.root.command.TryReadCommand_path(exe.line, out var cmd_path))
                        exe.args.Add(cmd_path);
                    else
                        exe.error = $"command '{exe.cmd_name}' could not find command '{exe.line.arg_last}'";
                },
                on_pipe: static (exe, args, data) =>
                {
                    string cmd_line = data switch
                    {
                        string str => str,
                        IEnumerable<object> lines => lines.Select(o => o.ToString()).Join(" "),
                        _ => data.ToString(),
                    };

                    Command.Line line = new(cmd_line, exe.line.signal, exe.line.terminal);
                    Command.Executor exe2 = new(exe.root, (List<KeyValuePair<string, Command>>)exe.args[0], line);

                    if (exe2.error != null)
                        exe.error = exe2.error;
                    else
                        exe2.Executate(line);
                }),
                "xargs");
        }
    }
}