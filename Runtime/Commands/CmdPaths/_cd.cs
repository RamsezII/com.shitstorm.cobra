using System.IO;
using _UTIL_;

namespace _COBRA_
{
    partial class CmdPaths
    {
        static void Init_cd()
        {
            Command.static_domain.AddAction(
                "change-directory",
                min_args: 1,
                opts: static exe =>
                {
                    if (exe.signal.TryRead_one_flag(exe, flag_create_if_empty))
                        exe.opts.Add(flag_create_if_empty, null);
                },
                args: static exe =>
                {
                    bool flag_create = exe.opts.ContainsKey(flag_create_if_empty);
                    if (exe.signal.TryReadArgument(out string path, out bool is_candidate, path_mode: FS_TYPES.DIRECTORY))
                    {
                        path = exe.shell.PathCheck(path, PathModes.ForceFull);
                        if (flag_create || Directory.Exists(path))
                            exe.args.Add(path);
                        else
                            exe.error = $"unexistant directory: '{path}'";
                    }
                },
                action: static exe =>
                {
                    bool flag_create = exe.opts.ContainsKey(flag_create_if_empty);
                    string path = (string)exe.args[0];
                    path = exe.shell.PathCheck(path, PathModes.ForceFull);

                    if (Directory.Exists(path))
                        exe.shell.working_dir = path;
                    else if (flag_create)
                    {
                        Directory.CreateDirectory(path);
                        exe.shell.working_dir = path;
                    }
                    else
                        exe.error = $"unexistant directory: '{path}'";
                },
                aliases: "cd");
        }
    }
}