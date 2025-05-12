using _ARK_;
using _UTIL_;
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
            Init_Remove();
            Init_EchoPaths();
            Init_Replace();

            Command.static_domain.AddAction(
                "working-directory",
                action: static exe => exe.Stdout(exe.shell.working_dir),
                aliases: "wdir");

            Command.static_domain.AddAction(
                "reset-working-dir",
                action: static exe => exe.shell.working_dir = NUCLEOR.home_path
                );

            Command.static_domain.AddAction(
                "file-exists",
                min_args: 1,
                args: static exe =>
                {
                    if (exe.line.TryReadArgument(out string file_path, out bool is_valid, path_mode: FS_TYPES.FILE))
                        exe.args.Add(file_path);
                },
                action: static exe =>
                {
                    string file_path = (string)exe.args[0];
                    file_path = exe.shell.PathCheck(file_path, PathModes.ForceFull);
                    exe.Stdout(File.Exists(file_path));
                },
                aliases: "fex");

            Command.static_domain.AddAction(
                "directory-exists",
                min_args: 1,
                args: static exe =>
                {
                    if (exe.line.TryReadArgument(out string dir_path, out bool is_valid, path_mode: FS_TYPES.DIRECTORY))
                        exe.args.Add(dir_path);
                },
                action: static exe =>
                {
                    string dir_path = (string)exe.args[0];
                    dir_path = exe.shell.PathCheck(dir_path, PathModes.ForceFull);
                    exe.Stdout(Directory.Exists(dir_path));
                },
                aliases: "dex");

            Command.static_domain.AddAction(
                "read",
                min_args: 1,
                args: static exe =>
                {
                    if (exe.line.TryReadArgument(out string path, out bool is_candidate, path_mode: FS_TYPES.FILE))
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
                    if (exe.line.TryReadArgument(out string path, out _, path_mode: FS_TYPES.DIRECTORY))
                        exe.args.Add(path);
                },
                action: static exe =>
                {
                    string path = (string)exe.args[0];
                    path = exe.shell.PathCheck(path, PathModes.ForceFull);
                    Directory.CreateDirectory(path);
                },
                aliases: "mkdir");
        }
    }
}