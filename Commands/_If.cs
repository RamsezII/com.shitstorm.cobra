using System;
using System.Collections.Generic;

namespace _COBRA_
{
    partial class CmdUtils
    {
        public static class Cmd_IF
        {
            static readonly Dictionary<string, Func<object, object, bool>> comparisons = new(StringComparer.OrdinalIgnoreCase)
            {
                { ">", (a, b) => Convert.ToDouble(a) > Convert.ToDouble(b) },
                { ">=", (a, b) => Convert.ToDouble(a) >= Convert.ToDouble(b) },
                { "<", (a, b) => Convert.ToDouble(a) < Convert.ToDouble(b) },
                { "<=", (a, b) => Convert.ToDouble(a) <= Convert.ToDouble(b) },
                { "==", (a, b) => a.Equals(b) },
                { "!=", (a, b) => !a.Equals(b) }
            };

            //--------------------------------------------------------------------------------------------------------------

            public static void Init()
            {
                Command.cmd_root_shell.AddCommand("if", new Command(
                    manual: new("[pipe output] if [--not] <condition> [command] [else <command>]"),
                    args: exe =>
                    {
                        if (exe.line.TryReadArgument(out string comp_key, comparisons.Keys))
                        {

                        }
                    },
                    action: exe =>
                    {

                    },
                    on_data: (exe, date) =>
                    {

                    }));
            }
        }
    }
}