using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace _COBRA_
{
    partial class CmdPaths
    {
        static void Init_edit()
        {
            const string flag_force_file = "--create-if-absent";

            Command.static_domain.AddRoutine(
                "edit-file",
                manual: new("create and edit a file"),
                min_args: 1,
                no_background: true,
                opts: static exe =>
                {
                    if (exe.line.TryRead_one_flag(exe, flag_force_file))
                        exe.opts.Add(flag_force_file, null);
                },
                args: static exe =>
                {
                    bool force = exe.opts.ContainsKey(flag_force_file);
                    if (exe.line.TryReadArgument(out string path, strict: !force, path_mode: PATH_FLAGS.FILE))
                        exe.args.Add(path);
                },
                routine: ERoutine
                );

            static IEnumerator<CMD_STATUS> ERoutine(Command.Executor exe)
            {
                bool force = exe.opts.ContainsKey(flag_force_file);
                string path = (string)exe.args[0];
                string parent_dir = Path.GetDirectoryName(path);
                Debug.Log(parent_dir);
                path = exe.shell.PathCheck(path, PathModes.ForceFull);

                string text = string.Empty;
                if (!force && !File.Exists(path))
                {
                    Debug.LogWarning($"[ERROR] {exe} trying to edit none existing file at: '{path}'\nuse {flag_force_file} if this was intended.");
                    yield break;
                }

                yield return new(CMD_STATES.FULLSCREEN_write);
                text = File.ReadAllText(path);
                exe.shell.terminal.ForceStdin(text);

                while (true)
                {
                    if (exe.line.signal.HasFlag(SIGNALS.SAVE))
                        exe.Stdout("Saving file...");
                    yield return new(CMD_STATES.FULLSCREEN_write);
                }
            }
        }
    }
}