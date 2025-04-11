using _ARK_;
using System;
using UnityEngine;

namespace _COBRA_
{
    static internal partial class CmdUtils
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoad()
        {
            Init_Args();
            InitManual();
            InitGrep();
            InitEdit();
            InitLinter();
            Init_If_Else();
            Init_Skip();
            Init_Stop();
            Init_Logs();

            const string
                flag_remove_empties = "--remove-empties",
                flag_no_white_space = "--no-white-space";

            Shell.static_domain.AddAction("echo",
                manual: new("echo!"),
                min_args: 1,
                args: exe =>
                {
                    if (exe.line.TryReadArgument(out string arg))
                        exe.args.Add(arg);
                },
                action: exe => exe.Stdout(exe.args[0])
                );

            Shell.static_domain.AddAction(
                "shutdown",
                manual: new("closes the application"),
                action: exe => Application.Quit()
                );

            Shell.static_domain.AddAction(
                "clear",
                manual: new("clear all previous logs"),
                action: exe => Application.Quit()
                );

            Shell.static_domain.AddAction(
                "clear-history",
                manual: new("clear all previous entries"),
                action: exe => NUCLEOR.delegates.onStartOfFrame_once += Command.Line.ClearHistory
                );

            Shell.static_domain.AddPipe(
                "split",
                on_pipe: static (exe, args, data) =>
                {
                    if (data is string str)
                    {
                        StringSplitOptions options = 0;
                        if (exe.args.Contains(flag_remove_empties))
                            options |= StringSplitOptions.RemoveEmptyEntries;

                        foreach (string line in str.Split(new[] { '\n' }, options))
                            exe.Stdout(line);
                    }
                    else
                        exe.Stdout(data);
                }
,
                args: static exe =>
                {
                    if (exe.line.TryReadFlags(exe, out var flags, flag_remove_empties))
                        foreach (string flag in flags)
                            exe.args.Add(flag);
                });

            Shell.static_domain.AddPipe(
                "prefixe",
                min_args: 1,
                args: static exe =>
                {
                    if (exe.line.TryReadArgument(out string prefixe))
                        exe.args.Add(prefixe);
                    if (exe.line.TryReadFlags(exe, out var flags, flag_no_white_space))
                        foreach (string flag in flags)
                            exe.args.Add(flag);
                },
                on_pipe: static (exe, args, data) =>
                {
                    string prefixe = (string)args[0];
                    bool no_space = args.Count > 1 && args[1].ToString().Equals(flag_no_white_space, StringComparison.InvariantCultureIgnoreCase);

                    if (!no_space && !prefixe.EndsWith(' '))
                        exe.Stdout($"{prefixe} {data}");
                    else
                        exe.Stdout($"{prefixe}{data}");
                }
                );
        }
    }
}