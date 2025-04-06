using System;

namespace _COBRA_
{
    partial class CmdVars
    {
        static void InitEquals()
        {
            Command.cmd_root_shell.AddCommand(new(
                "equals-literal",
                manual: new("[pipe output] <value>"),
                init_min_args_required: 1,
                args: static exe =>
                {
                    if (exe.line.TryReadArgument(out string literal))
                        exe.args.Add(literal);
                },
                on_data: (exe, data) =>
                {
                    string literal = (string)exe.args[0];
                    string data_str = data.ToString();
                    exe.Stdout(data_str.Equals(literal, StringComparison.Ordinal));
                }
                ));

            Command.cmd_root_shell.AddCommand(new(
                "equals-var",
                manual: new("[pipe output] <variable>"),
                init_min_args_required: 1,
                args: static exe =>
                {
                    if (exe.line.TryReadArgument(out string var_name, variables.Keys))
                        exe.args.Add(var_name);
                },
                on_data: (exe, data) =>
                {
                    string var_name = (string)exe.args[0];
                    object value = variables[var_name];
                    exe.Stdout(data.ToString().Equals(value.ToString(), StringComparison.Ordinal));
                }
                ));
        }
    }
}