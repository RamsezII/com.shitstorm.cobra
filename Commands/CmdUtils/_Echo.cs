using System.Collections.Generic;

namespace _COBRA_
{
    partial class CmdUtils
    {
        static void InitEcho()
        {
            Command.cmd_root_shell.AddCommand(new(
                "echo",
                manual: new("echo!"),
                action_min_args_required: 1,
                args: exe =>
                {
                    if (exe.line.TryReadArgument(out string arg))
                        exe.args.Add(arg);
                },
                action: exe => exe.Stdout((string)exe.args[0]),
                on_pipe: (exe, args, data) =>
                {
                    switch (data)
                    {
                        case string str:
                            exe.Stdout(str);
                            break;

                        case IEnumerable<object> lines:
                            exe.Stdout(lines.LinesToText());
                            break;

                        default:
                            exe.Stdout(data);
                            break;
                    }
                }));
        }
    }
}