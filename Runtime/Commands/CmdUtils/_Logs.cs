using UnityEngine;

namespace _COBRA_
{
    partial class CmdUtils
    {
        static void Init_Logs()
        {
            const string
                flag_warning = "--warning",
                flag_error = "--error",
                flag_background = "--background";

            Command.static_domain.AddAction(
                "log",
                min_args: 1,
                args: exe =>
                {
                    if (exe.line.TryRead_one_of_the_flags(exe, out string flag, new[] { flag_warning, flag_error, flag_background, }))
                        exe.opts[flag] = null;
                    if (exe.line.TryReadArgument(out string message, out _))
                        exe.args.Add(message);
                },
                action: static exe =>
                {
                    string message = (string)exe.args[0];
                    if (exe.opts.ContainsKey(flag_warning))
                        Debug.LogWarning(message);
                    else if (exe.opts.ContainsKey(flag_error))
                        Debug.LogError(message);
                    else if (exe.opts.ContainsKey(flag_background))
                        Debug.Log(message.ToString().ToSubLog());
                    else
                        Debug.Log(message);
                });
        }
    }
}