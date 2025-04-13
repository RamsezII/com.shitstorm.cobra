using System;

namespace _COBRA_
{
    partial class CmdVars
    {
        static void InitEquals()
        {
            Command.static_domain.AddPipe(
                "equals-literal",
                manual: new("[pipe output] <value>"),
                max_args: 1,
                args: static exe =>
                {
                    if (exe.line.TryReadArgument(out string literal))
                        exe.args.Add(literal);
                },
                on_pipe: static (exe, args, data) =>
                {
                    string literal = (string)exe.args[0];
                    string data_str = data.ToString();
                    exe.Stdout(data_str.Equals(literal, StringComparison.Ordinal));
                }
                );

            Command.static_domain.AddPipe(
                "equals-var",
                manual: new("[pipe output] <variable>"),
                max_args: 1,
                args: static exe =>
                {
                    if (exe.line.TryReadArgument(out string var_name, variables.Keys))
                        exe.args.Add(var_name);
                },
                on_pipe: static (exe, args, data) =>
                {
                    string var_name = (string)exe.args[0];
                    object value = variables[var_name];
                    exe.Stdout(data.ToString().Equals(value.ToString(), StringComparison.Ordinal));
                }
                );
        }
    }
}