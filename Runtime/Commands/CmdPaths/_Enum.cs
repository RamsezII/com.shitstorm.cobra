using System;
using System.IO;

namespace _COBRA_
{
    partial class CmdPaths
    {
        const string
            opt_pattern = "--pattern";

        static void Init_Enumerates()
        {
            Command cmd_enum = Command.static_domain.AddAction(
                "enumerate",
                min_args: 2,
                opts: static exe =>
                {
                    if (exe.line.TryRead_options(exe, out var outputs, opt_pattern))
                        foreach (var pair in outputs)
                            exe.opts.Add(pair.Key, pair.Value);
                },
                args: static exe =>
                {
                    if (exe.line.TryReadArgument(out string arg, out bool is_candidate, Enum.GetNames(typeof(PATH_FLAGS))))
                        if (is_candidate)
                            if (Enum.TryParse(arg, true, out PATH_FLAGS flags) && flags > 0)
                            {
                                exe.args.Add(flags);
                                if (exe.line.TryReadArgument(out string path, out bool seems_valid, path_mode: PATH_FLAGS.DIRECTORY))
                                    if (seems_valid)
                                        exe.args.Add(path);
                            }
                },
                action: static exe =>
                {
                    PATH_FLAGS flags = (PATH_FLAGS)exe.args[0];
                    string full_path = (string)exe.args[1];
                    full_path = exe.shell.PathCheck(full_path, PathModes.ForceFull);

                    string pattern;
                    if (exe.opts.TryGetValue(opt_pattern, out object _pattern))
                        pattern = (string)_pattern;
                    else
                        pattern = "*";

                    switch (flags)
                    {
                        case PATH_FLAGS.FILE:
                            foreach (string file in Directory.EnumerateFiles(full_path, pattern))
                                exe.Stdout(exe.shell.PathCheck(file, PathModes.ForceFull));
                            break;

                        case PATH_FLAGS.DIRECTORY:
                            foreach (string dir in Directory.EnumerateDirectories(full_path, pattern))
                                exe.Stdout(exe.shell.PathCheck(dir, PathModes.ForceFull));
                            break;

                        case PATH_FLAGS.BOTH:
                            foreach (string fse in Directory.EnumerateFileSystemEntries(full_path, pattern))
                                exe.Stdout(exe.shell.PathCheck(fse, PathModes.ForceFull));
                            break;
                    }
                });
        }
    }
}