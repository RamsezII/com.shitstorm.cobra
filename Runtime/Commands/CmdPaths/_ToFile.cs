using System;
using System.IO;

namespace _COBRA_
{
    partial class CmdPaths
    {
        internal static Command cmd_tofile;

        //--------------------------------------------------------------------------------------------------------------

        static void Init_ToFile()
        {
            cmd_tofile = Command.static_domain.AddPipe(
                "to-file",
                min_args: 1,
                opts: static exe =>
                {
                    if (exe.line.TryRead_flags(exe, out var flags, flag_create_if_empty, flag_overwrite))
                        foreach (string flag in flags)
                            exe.opts.Add(flag, null);
                },
                args: static exe =>
                {
                    bool create_if_empty = exe.opts.ContainsKey(flag_create_if_empty);
                    if (exe.line.TryReadArgument(out string path, out bool is_candidate, path_mode: PATH_FLAGS.FILE))
                        exe.args.Add(path);
                },
                on_pipe: static (exe, data) =>
                {
                    bool create_if_empty = exe.opts.ContainsKey(flag_create_if_empty);
                    bool overwrite = exe.opts.ContainsKey(flag_overwrite);

                    string path = (string)exe.args[0];
                    path = exe.shell.PathCheck(path, PathModes.ForceFull);
                    bool exists = File.Exists(path);

                    if (!exists)
                        if (create_if_empty)
                        {
                            File.OpenWrite(path).Close();
                            exists = true;
                        }
                        else
                            exe.error = $"could not find file at: '{path}'";

                    if (exists)
                        try
                        {
                            using StreamWriter writer = new(path, !overwrite);
                            writer.Write(data);
                        }
                        catch (Exception ex)
                        {
                            exe.error = $"failed to write to file: '{path}', error: {ex.TrimMessage()}";
                        }
                });
        }
    }
}