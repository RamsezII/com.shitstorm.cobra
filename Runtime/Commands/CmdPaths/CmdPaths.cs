using System.IO;
using UnityEngine;

namespace _COBRA_
{
    internal static partial class CmdPaths
    {
        const string
            opt_search_pattern = "--search-pattern";

        //--------------------------------------------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoad()
        {
            Init_cd();

            Command.static_domain.AddAction(
                "working-directory",
                action: static exe => exe.Stdout(exe.shell.work_dir),
                aliases: "wdir");

            Command.static_domain.AddAction(
                "path-test",
                min_args: 1,
                args: static exe =>
                {
                    if (exe.line.TryReadArgument(out string path, is_path: true))
                        ;
                },
                action: static exe =>
                {

                });

            Command.static_domain.AddAction(
                "ls",
                opts: static exe =>
                {
                    if (exe.line.TryRead_options(exe, out var opts, opt_search_pattern))
                        if (opts.ContainsKey(opt_search_pattern))
                            exe.opts.Add(opt_search_pattern, opts[opt_search_pattern]);
                },
                action: static exe =>
                {
                    string search_pattern = "*";
                    if (exe.opts.TryGetValue(opt_search_pattern, out object value))
                        search_pattern = (string)value;

                    foreach (string dir in Directory.EnumerateFileSystemEntries(exe.shell.work_dir, search_pattern))
                        exe.Stdout(Path.GetRelativePath(exe.shell.work_dir, dir));
                });

            Command.static_domain.AddAction(
                "read",
                min_args: 1,
                args: static exe =>
                {

                },
                action: static exe =>
                {

                },
                aliases: "cat");

            Command.static_domain.AddAction(
                "make-directory",
                min_args: 1,
                args: static exe =>
                {
                    if (exe.line.TryReadArgument(out string arg, is_path: true))
                        exe.args.Add(arg);
                },
                action: static exe =>
                {
                    string path = (string)exe.args[0];
                    if (!Path.IsPathRooted(path))
                        path = Path.Combine(exe.shell.work_dir, path);

                    if (Directory.Exists(path))
                        exe.error = $"path already exists: '{path}'";
                    else
                        Directory.CreateDirectory(path);
                },
                aliases: "mkdir");
        }
    }
}