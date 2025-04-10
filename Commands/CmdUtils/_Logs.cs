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

            Shell.static_domain.AddPipe(
                "log",
                args: exe =>
                {
                    if (exe.line.TryReadOneOfFlags(exe, out string flag))
                        exe.args.Add(flag);
                },
                on_pipe: static (exe, args, data) =>
                {
                    if (exe.args.Count == 0)
                        Debug.Log(data);
                    else if (args.Contains(flag_warning))
                        Debug.LogWarning(data);
                    else if (args.Contains(flag_error))
                        Debug.LogError(data);
                    else if (args.Contains(flag_background))
                        Debug.Log(data.ToString().ToSubLog());
                    else
                        Debug.Log(data);
                });
        }
    }
}