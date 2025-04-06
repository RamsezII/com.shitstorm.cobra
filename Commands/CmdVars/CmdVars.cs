using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _COBRA_
{
    internal static partial class CmdVars
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

        public static readonly Dictionary<string, object> variables = new(StringComparer.Ordinal);
        public static readonly IOrderedEnumerable<string> EVariables = variables.Keys.OrderBy(k => k, StringComparer.Ordinal);

        //--------------------------------------------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnBeforeSceneLoad()
        {
            variables.Clear();
        }

        //--------------------------------------------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoad()
        {
            InitIf();
            InitEquals();
            InitVars();
        }

        static void InitVars()
        {
            Command.cmd_root_shell.AddCommand(new(
                "get-var",
                manual: new("<variable name>"),
                args: exe =>
                {
                    if (exe.line.TryReadArgument(out string arg1, variables.Keys))
                        if (variables.TryGetValue(arg1, out object value))
                            exe.args.Add(value);
                        else
                            exe.error = $"variable '{arg1}' not found";
                },
                action: exe => exe.Stdout(exe.args[0])
                ));

            Command.cmd_root_shell.AddCommand(new(
                "set-var",
                manual: new("<variable name> <value>"),
                args: exe =>
                {
                    if (exe.line.TryReadArgument(out string var_name))
                    {
                        exe.args.Add(var_name);
                        if (exe.line.TryReadArgument(out string var_value))
                            exe.args.Add(var_value);
                    }
                },
                action: exe =>
                {
                    string var_name = (string)exe.args[0];
                    string value = (string)exe.args[1];
                    variables[var_name] = value;
                }
                ));
        }
    }
}