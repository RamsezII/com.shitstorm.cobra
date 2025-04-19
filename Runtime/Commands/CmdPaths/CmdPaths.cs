using _ARK_;
using System.IO;
using UnityEngine;

namespace _COBRA_
{
    internal static partial class CmdPaths
    {
        const string
            opt_search_pattern = "--search-pattern",
            flag_recursive = "--recursive",
            flag_create_if_empty = "--create-if-empty",
            flag_overwrite = "--overwrite";

        //--------------------------------------------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoad()
        {
            Init_cd();
            Init_edit();
            Init_ToFile();
            Init_Enumerates();
            Init_CombinePaths();

            Command.static_domain.AddAction(
                "working-directory",
                action: static exe => exe.Stdout(exe.shell.working_dir),
                aliases: "wdir");

            Command.static_domain.AddAction(
                "reset-working-dir",
                action: static exe => exe.shell.working_dir = NUCLEOR.home_path
                );

            Command.static_domain.AddAction(
                "read",
                min_args: 1,
                args: static exe =>
                {
                    if (exe.line.TryReadArgument(out string path, out bool is_candidate, path_mode: PATH_FLAGS.FILE))
                        exe.args.Add(path);
                },
                action: static exe =>
                {
                    string path = (string)exe.args[0];
                    path = exe.shell.PathCheck(path, PathModes.ForceFull);

                    string text = File.ReadAllText(path);
                    exe.Stdout(text);
                },
                aliases: "cat");

            Command.static_domain.AddAction(
                "make-directory",
                min_args: 1,
                args: static exe =>
                {
                    if (exe.line.TryReadArgument(out string path, out _, path_mode: PATH_FLAGS.DIRECTORY))
                        exe.args.Add(path);
                },
                action: static exe =>
                {
                    string path = (string)exe.args[0];
                    path = exe.shell.PathCheck(path, PathModes.ForceFull);
                    Directory.CreateDirectory(path);
                },
                aliases: "mkdir");

            Command.static_domain.AddAction(
                "rm",
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
                });

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