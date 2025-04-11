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
            InitEquals();
            InitVars();
        }

        static void InitVars()
        {
            Shell.static_domain.AddAction(
                "get-var",
                manual: new("<variable name>"),
                min_args: 1,
                opts: null,
                args: static exe =>
                {
                    if (exe.line.TryReadArgument(out string var_name, variables.Keys, lint: false))
                        if (variables.TryGetValue(var_name, out object var_value))
                        {
                            exe.line.LintToThisPosition(exe.line.linter.variable);
                            exe.args.Add(var_value);
                        }
                        else
                            exe.error = $"variable '{var_name}' not found";
                },
                action: exe => exe.Stdout(exe.args[0])
                );

            Shell.static_domain.AddAction(
                "set-var",
                manual: new("<variable name> <value>"),
                min_args: 2,
                opts: null,
                args: static exe =>
                {
                    if (exe.line.TryReadArgument(out string var_name, variables.Keys, lint: false))
                    {
                        exe.line.LintToThisPosition(exe.line.linter.variable);
                        exe.args.Add(var_name);

                        if (exe.line.TryReadArgument(out string var_value, lint: false))
                        {
                            exe.line.LintToThisPosition(exe.line.linter.value);
                            exe.args.Add(var_value);
                        }
                    }
                },
                action: static exe =>
                {
                    string var_name = (string)exe.args[0];
                    object value = exe.args[1];
                    variables[var_name] = value;
                }
                );
        }
    }
}