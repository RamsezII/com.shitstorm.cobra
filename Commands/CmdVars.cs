using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _COBRA_
{
    internal static class CmdVars
    {
        public static readonly Dictionary<string, object> variables = new(StringComparer.Ordinal);
        public static readonly IOrderedEnumerable<string> EVariables = variables.Keys.OrderBy(k => k, StringComparer.Ordinal);

        //--------------------------------------------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnBeforeSceneLoad()
        {
            variables.Clear();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoad()
        {
            Command.cmd_root_shell.AddCommand("var", new(
                manual: new("save and read variables"),
                args: exe =>
                {
                    if (exe.line.TryReadArgument(out string var_name, EVariables))
                    {
                        exe.args.Add(var_name);
                        if (exe.line.TryReadArgument(out string value))
                            exe.args.Add(value);
                    }
                },
                action: exe =>
                {
                    string var_name = (string)exe.args[0];
                    if (exe.args.Count < 2)
                    {
                        if (variables.TryGetValue(var_name, out object value))
                            exe.Stdout(value);
                        else
                            exe.Stderr($"Variable '{var_name}' not found");
                    }
                    else
                    {
                        object value = exe.args[1];
                        if (variables.ContainsKey(var_name))
                            variables[var_name] = value;
                        else
                            variables.Add(var_name, value);
                    }
                },
                on_data: (exe, data) =>
                {
                    string var_name = (string)exe.args[0];
                    variables[var_name] = data;
                }
                ));
        }
    }
}