using System.IO;
using System.Text;
using _UTIL_;

namespace _COBRA_
{
    partial class CmdPaths
    {
        const string
            opt_pattern = "--pattern",
            flag_r = "-r",
            flag_file = "-f",
            flag_dir = "-d";

        static readonly Command.Line.OptionParser ls_opts = new(false)
        {
            { flag_file, null },
            { flag_dir, null },
            { flag_r, null },
            {
                Command.Line.opt_workdir,
                Command.Line.OptionParser.parser_workingdir
            },
            {
                opt_pattern,
                line =>
                {
                    if (line.TryReadArgument(out string arg, out _))
                        return arg;
                    return null;
                }
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
                    FS_TYPES flags = FS_TYPES.BOTH;
                    if (exe.opts.ContainsKey(flag_dir))
                        flags = FS_TYPES.DIRECTORY;
                    if (exe.opts.ContainsKey(flag_file))
                        flags |= FS_TYPES.FILE;

                    if (!exe.opts.TryGetValue_str(opt_pattern, out string pattern))
                        pattern = "*";

                    SearchOption search_option = exe.opts.ContainsKey(flag_r) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

                    string workdir = exe.GetWorkdir();
                    StringBuilder sb = new();

                    switch (flags)
                    {
                        case FS_TYPES.FILE:
                            foreach (string file in Directory.EnumerateFiles(workdir, pattern, search_option))
                                sb.AppendLine(exe.shell.PathCheck(file, PathModes.TryLocal));
                            break;

                        case FS_TYPES.DIRECTORY:
                            foreach (string dir in Directory.EnumerateDirectories(workdir, pattern, search_option))
                                sb.AppendLine(exe.shell.PathCheck(dir, PathModes.TryLocal));
                            break;

                        case FS_TYPES.BOTH:
                            foreach (string fse in Directory.EnumerateFileSystemEntries(workdir, pattern, search_option))
                                sb.AppendLine(exe.shell.PathCheck(fse, PathModes.TryLocal));
                            break;
                    }

                    exe.Stdout(sb.TroncatedForLog());
                });
        }
    }
}