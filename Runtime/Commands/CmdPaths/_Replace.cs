using _UTIL_;
using System.IO;
using UnityEngine;

namespace _COBRA_
{
    partial class CmdPaths
    {
        static void Init_Replace()
        {
            Command.static_domain.AddAction(
                "replace-extension",
                min_args: 3,
                args: static exe =>
                {
                    if (exe.line.TryReadArgument(out string path, out bool seems_valid, strict: true, path_mode: FS_TYPES.BOTH))
                    {
                        exe.args.Add(path);
                        if (exe.line.TryReadArgument(out string ext1, out _))
                        {
                            exe.args.Add(ext1);
                            if (exe.line.TryReadArgument(out string ext2, out _))
                                exe.args.Add(ext2);
                        }
                    }
                },
                action: static exe =>
                {
                    string path = (string)exe.args[0];
                    path = exe.shell.PathCheck(path, PathModes.ForceFull);
                    string ext1 = (string)exe.args[1];
                    string ext2 = (string)exe.args[2];

                    if (File.Exists(path))
                        if (path.EndsWith(ext1, System.StringComparison.OrdinalIgnoreCase))
                        {
                            string new_path = path[..^ext1.Length] + ext2;
                            File.Move(path, new_path);
                            Debug.Log($"{Path.GetFileName(path)} -> {Path.GetFileName(new_path)}".ToSubLog());
                        }
                        else
                            exe.error = $"File '{path}' does not end with '{ext1}'";

                    if (Directory.Exists(path))
                        foreach (string file in Directory.GetFiles(path, "*" + ext1, SearchOption.AllDirectories))
                        {
                            string new_path = file[..^ext1.Length] + ext2;
                            File.Move(file, new_path);
                            Debug.Log($"{Path.GetFileName(file)} -> {Path.GetFileName(new_path)}".ToSubLog());
                        }
                });
        }
    }
}