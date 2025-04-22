using System.IO;
using UnityEngine;

namespace _COBRA_
{
    partial class CmdPaths
    {
        static void Init_Remove()
        {
            Command.static_domain.AddAction(
                "remove-file-or-directory",
                min_args: 1,
                opts: static exe =>
                {
                    if (exe.line.TryRead_one_flag(exe, "-r", flag_recursive))
                        exe.opts.Add(flag_recursive, null);
                },
                args: static exe =>
                {
                    if (exe.line.TryReadArgument(out string path, out bool is_candidate, path_mode: PATH_FLAGS.BOTH))
                        if (is_candidate)
                            exe.args.Add(path);
                },
                action: static exe =>
                {
                    bool recursive = exe.opts.ContainsKey(flag_recursive);
                    string path = (string)exe.args[0];
                    path = exe.shell.PathCheck(path, PathModes.ForceFull);

                    try
                    {
                        if (Directory.Exists(path))
                            Directory.Delete(path, recursive);
                        if (File.Exists(path))
                            File.Delete(path);
                    }
                    catch (IOException ioe)
                    {
                        Debug.LogException(ioe);
                        Debug.LogWarning($"use the '{flag_recursive}' flag if you want to remove recursively (including inside content)");
                    }
                },
                aliases: "rm");

            Command.static_domain.AddAction(
                "remove-directory",
                min_args: 1,
                opts: static exe =>
                {
                    if (exe.line.TryRead_one_flag(exe, "-r", flag_recursive))
                        exe.opts.Add(flag_recursive, null);
                },
                args: static exe =>
                {
                    if (exe.line.TryReadArgument(out string path, out bool is_candidate, path_mode: PATH_FLAGS.DIRECTORY))
                        if (is_candidate)
                            exe.args.Add(path);
                        else
                            exe.error = $"invalid directory: '{path}'";
                },
                action: static exe =>
                {
                    bool recursive = exe.opts.ContainsKey(flag_recursive);
                    string path = (string)exe.args[0];
                    path = exe.shell.PathCheck(path, PathModes.ForceFull);

                    try
                    {
                        Directory.Delete(path, recursive);
                    }
                    catch (IOException ioe)
                    {
                        Debug.LogException(ioe);
                        Debug.LogWarning($"use the '{flag_recursive}' flag if you want to remove recursively (including inside content)");
                    }
                });
        }
    }
}