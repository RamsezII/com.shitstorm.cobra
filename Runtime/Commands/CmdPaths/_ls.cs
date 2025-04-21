using System.Collections.Generic;
using System.IO;
using System.Text;

namespace _COBRA_
{
    partial class CmdPaths
    {
        const string
            opt_pattern = "--pattern",
            flag_file = "-f",
            flag_dir = "-d";

        static readonly Command.Line.OptionParser ls_opts = new(false)
        {
            { flag_file, null },
            { flag_dir, null },
            {
                opt_pattern,
                line =>
                {
                    if (line.TryReadArgument(out string arg, out _))
                        return arg;
                    return null;
                }
            },
            {
                Command.Line.opt_workdir,
                Command.Line.OptionParser.parser_workingdir
            }
        };

        static void Init_Enumerates()
        {
            Command.static_domain.AddAction(
                "ls",
                opts: static exe =>
                {
                    if (exe.line.TryRead_options_parsed(exe, out var outputs, ls_opts))
                        foreach (var pair in outputs)
                            exe.opts.Add(pair.Key, pair.Value);
                },
                action: static exe =>
                {
                    PATH_FLAGS flags = PATH_FLAGS.BOTH;
                    if (exe.opts.ContainsKey(flag_dir))
                        flags = PATH_FLAGS.DIRECTORY;
                    if (exe.opts.ContainsKey(flag_file))
                        flags |= PATH_FLAGS.FILE;

                    if (!exe.opts.TryGetValue_str(opt_pattern, out string pattern))
                        pattern = "*";

                    string workdir = exe.GetWorkdir();
                    StringBuilder sb = new();

                    switch (flags)
                    {
                        case PATH_FLAGS.FILE:
                            foreach (string file in Directory.EnumerateFiles(workdir, pattern))
                                sb.AppendLine(exe.shell.PathCheck(file, PathModes.ForceFull));
                            break;

                        case PATH_FLAGS.DIRECTORY:
                            foreach (string dir in Directory.EnumerateDirectories(workdir, pattern))
                                sb.AppendLine(exe.shell.PathCheck(dir, PathModes.ForceFull));
                            break;

                        case PATH_FLAGS.BOTH:
                            foreach (string fse in Directory.EnumerateFileSystemEntries(workdir, pattern))
                                sb.AppendLine(exe.shell.PathCheck(fse, PathModes.ForceFull));
                            break;
                    }

                    exe.Stdout(sb.TroncatedForLog());
                });
        }
    }
}