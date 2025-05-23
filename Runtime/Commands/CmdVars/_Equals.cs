﻿using System;

namespace _COBRA_
{
    partial class CmdVars
    {
        static void Init_Equals()
        {
            Command.static_domain.AddPipe(
                "equals",
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
                });
        }
    }
}