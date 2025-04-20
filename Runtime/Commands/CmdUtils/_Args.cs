using System.Collections.Generic;
using System.Linq;

namespace _COBRA_
{
    partial class CmdUtils
    {
        static void Init_Args()
        {
            Command.static_domain.AddPipe(
                "args",
                manual: new("pipes data as arguments to a command"),
                min_args: 1,
                max_args: 1,
                args: static exe =>
                {
                    if (Command.static_domain.TryReadCommand_path(exe.line, out var cmd_path))
                    {
                        Command.Executor exe2 = new(exe.shell, exe, exe.line, cmd_path, parse_arguments: false);
                        exe.args.Add(exe2);
                    }
                    else
                        exe.error = $"command '{exe.command.name}' could not find command ({nameof(Command.Line.arg_last)}: '{exe.line.arg_last}')";
                },
                on_pipe: static (exe, data) =>
                {
                    string cmd_line = data switch
                    {
                        string str => str,
                        IEnumerable<object> lines => lines.Select(o => o.ToString()).Join(" "),
                        _ => data.ToString(),
                    };

                    Command.Line line = new(cmd_line, exe.line.signal, exe.shell);
                    Command.Executor exe2 = (Command.Executor)exe.args[0];
                    exe2.ParseArguments(line);

                    if (exe2.error != null)
                        exe.error = exe2.error;
                    else
                        exe.janitor.AddExecutor(exe.line, exe2);
                },
                aliases: "xargs");
        }
    }
}