using System.IO;
using System.Linq;

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
                    if (exe.line.TryRead_options(exe, out var opts, opt_search_pattern))
                        if (opts.ContainsKey(opt_search_pattern))
                            exe.opts.Add(opt_search_pattern, opts[opt_search_pattern]);
                },
                args: static exe =>
                {
                    string search_pattern = "*";
                    if (exe.opts.TryGetValue(search_pattern, out object value))
                        search_pattern = (string)value;

                    var dirs =
                        Directory.EnumerateDirectories(exe.shell.work_dir, search_pattern)
                        .Select(path => Path.GetRelativePath(exe.shell.work_dir, path));

                    if (exe.line.TryReadArgument(out string arg, dirs))
                    {
                        string path = Path.Combine(exe.shell.work_dir, arg);
                        path = Path.GetFullPath(path);

                        if (Directory.Exists(path))
                            exe.args.Add(path);
                        else
                            exe.error = $"unexistant directory: '{path}'";
                    }
                },
                action: static exe =>
                {
                    string new_path = Path.Combine(exe.shell.work_dir, (string)exe.args[0]);
                    new_path = Path.GetFullPath(new_path);

                    if (Directory.Exists(new_path))
                        exe.shell.work_dir = new_path;
                    else
                        exe.error = $"unexistant directory: '{new_path}'";
                },
                aliases: "cd");
        }
    }
}