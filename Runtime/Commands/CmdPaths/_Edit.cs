﻿using System.IO;
using _UTIL_;
using UnityEngine;

namespace _COBRA_
{
    partial class CmdPaths
    {
        static void Init_edit()
        {
            const string flag_force_file = "--create-if-absent";

            Command.static_domain.AddAction(
                "open",
                manual: new("create and edit a file"),
                min_args: 1,
                no_background: true,
                opts: static exe =>
                {
                    if (exe.line.TryRead_one_flag(exe, flag_force_file))
                        exe.opts.Add(flag_force_file, null);
                },
                args: static exe =>
                {
                    bool force = exe.opts.ContainsKey(flag_force_file);
                    if (exe.line.TryReadArgument(out string path, out bool is_candidate, path_mode: FS_TYPES.FILE))
                        exe.args.Add(path);
                },
                action: static exe =>
                {
                    bool force = exe.opts.ContainsKey(flag_force_file);
                    string path = (string)exe.args[0];
                    path = exe.shell.PathCheck(path, PathModes.ForceFull);

                    string text = string.Empty;
                    if (!force && !File.Exists(path))
                        Debug.LogWarning($"[ERROR] {exe} trying to edit none existing file at: '{path}'\nuse {flag_force_file} if this was intended.");

                    Application.OpenURL(path);
                });
        }
    }
}