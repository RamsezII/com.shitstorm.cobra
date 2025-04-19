using System.IO;

namespace _COBRA_
{
    partial class CmdPaths
    {
        static void Init_CombinePaths()
        {
            Command.static_domain.AddAction(
                "combine-paths",
                min_args: 2,
                args: static exe =>
                {
                    if (exe.line.TryReadArgument(out string path1, out _, path_mode: PATH_FLAGS.BOTH))
                    {
                        exe.args.Add(path1);
                        if (exe.line.TryReadArgument(out string path2, out _, path_mode: PATH_FLAGS.BOTH))
                            exe.args.Add(path2);
                    }
                },
                action: static exe =>
                {
                    string path1 = (string)exe.args[0];
                    string path2 = (string)exe.args[1];

                    string combine = Path.Combine(path1, path2).Replace("\\","/");
                    exe.Stdout(combine);
                });
        }
    }
}