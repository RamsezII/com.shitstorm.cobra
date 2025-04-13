using System.IO;

namespace _COBRA_
{
    partial class CmdPaths
    {
        static void Init_cd()
        {
            Command.static_domain.AddAction(
                "change-directory",
                min_args: 1,
                args: static exe =>
                {
                    if (exe.line.TryReadArgument(out string path, path_mode: PATH_FLAGS.DIRECTORY))
                        if (Directory.Exists(path))
                            exe.args.Add(path);
                        else
                            exe.error = $"unexistant directory: '{path}'";
                },
                action: static exe =>
                {
                    string path = ((string)exe.args[0]).SafeRootedPath(exe.shell.work_dir);
                    if (Directory.Exists(path))
                        exe.shell.work_dir = path;
                    else
                        exe.error = $"unexistant directory: '{path}'";
                },
                aliases: "cd");
        }
    }
}