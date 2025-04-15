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
                    if (exe.line.TryReadArgument(out string literal, out _))
                        exe.args.Add(literal);
                },
                on_pipe: static (exe, data) =>
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
                    if (exe.line.TryReadArgument(out string var_name, out bool is_candidate, variables.Keys))
                        if (is_candidate)
                            exe.args.Add(var_name);
                        else
                            exe.error = $"unknown var '{var_name}'";
                },
                on_pipe: static (exe, data) =>
                {
                    string var_name = (string)exe.args[0];
                    object value = variables[var_name];
                    exe.Stdout(data.ToString().Equals(value.ToString(), StringComparison.Ordinal));
                }
                );
        }
    }
}