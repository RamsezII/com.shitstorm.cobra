using System.Text;

namespace _COBRA_
{
    partial class CmdPaths
    {
        static void Init_EchoPaths()
        {
            const string
                flag_full = "-F";

            Command.static_domain.AddAction(
                "echo-paths",
                min_args: 1,
                max_args: 100,
                opts: static exe =>
                {
                    if (exe.line.TryRead_one_of_the_flags(exe, out string flag, flag_dir, flag_file))
                        exe.opts.Add(flag, null);

                    if (exe.line.TryRead_one_flag(exe, flag_full))
                        exe.opts.Add(flag_full, null);
                },
                args: static exe =>
                {
                    PATH_FLAGS mode = PATH_FLAGS.BOTH;
                    if (exe.opts.ContainsKey(flag_dir))
                        mode = PATH_FLAGS.DIRECTORY;
                    if (exe.opts.ContainsKey(flag_file))
                        mode = PATH_FLAGS.FILE;

                    bool full = exe.opts.ContainsKey(flag_full);
                    while (exe.line.TryReadArgument(out string path, out _, strict: true, path_mode: mode))
                    {
                        if (full)
                            path = exe.shell.PathCheck(path, PathModes.ForceFull);
                        exe.args.Add(path);
                    }
                },
                action: static exe =>
                {
                    StringBuilder sb = new();
                    for (int i = 0; i < exe.args.Count; ++i)
                        sb.AppendLine(exe.args[i].ToString());
                    exe.Stdout(sb.TroncatedForLog());
                });
        }
    }
}