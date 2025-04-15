﻿using _ARK_;
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
            InitLinter();
            Init_If_Else();
            Init_Skip();
            Init_Stop();
            Init_Logs();
            Init_Seconds();
            Init_Signal();

            CmdExecutors.Init();

            const string
                flag_remove_empties = "--remove-empties",
                flag_no_white_space = "--no-white-space";

            Command.static_domain.AddAction("echo",
                manual: new("echo!"),
                min_args: 1,
                args: exe =>
                {
                    if (exe.line.TryReadArgument(out string arg, out _))
                        exe.args.Add(arg);
                },
                action: exe => exe.Stdout(exe.args[0])
                );

            Command.static_domain.AddAction(
                "shutdown",
                manual: new("closes the application"),
                action: exe => Application.Quit()
                );

            Command.static_domain.AddAction(
                "clear",
                manual: new("clear all previous logs"),
                action: exe => Application.Quit()
                );

            Command.static_domain.AddAction(
                "clear-history",
                manual: new("clear all previous entries"),
                action: exe => NUCLEOR.delegates.onStartOfFrame_once += Command.Line.ClearHistory
                );

            Command.static_domain.AddPipe(
                "split",
                opts: static exe =>
                {
                    if (exe.line.TryRead_flags(exe, out var flags, flag_remove_empties))
                        foreach (string flag in flags)
                            exe.opts.Add(flag, null);
                },
                on_pipe: static (exe, data) =>
                {
                    if (data is string str)
                    {
                        StringSplitOptions options = 0;
                        if (exe.opts.ContainsKey(flag_remove_empties))
                            options |= StringSplitOptions.RemoveEmptyEntries;

                        foreach (string line in str.Split(new[] { ' ', '\n' }, options))
                            exe.Stdout(line);
                    }
                    else
                        exe.Stdout(data);
                });

            Command.static_domain.AddPipe(
                "prefixe",
                min_args: 1,
                on_pipe: static (exe, data) =>
                {
                    string prefixe = (string)exe.args[0];
                    bool no_space = exe.args.Count > 1 && exe.args[1].ToString().Equals(flag_no_white_space, StringComparison.InvariantCultureIgnoreCase);

                    if (!no_space && !prefixe.EndsWith(' '))
                        exe.Stdout($"{prefixe} {data}");
                    else
                        exe.Stdout($"{prefixe}{data}");
                },
                args: static exe =>
                {
                    if (exe.line.TryReadArgument(out string prefixe, out _))
                        exe.args.Add(prefixe);
                    if (exe.line.TryRead_flags(exe, out var flags, flag_no_white_space))
                        foreach (string flag in flags)
                            exe.args.Add(flag);
                }
                );
        }
    }
}