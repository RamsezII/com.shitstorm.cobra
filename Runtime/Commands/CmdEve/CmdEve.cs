using System.Collections.Generic;
using UnityEngine;

namespace _COBRA_
{
    static internal partial class CmdEve
    {
        static Command domain_eve, cmd_ls, cmd_cat, cmd_cd;
        const string eve_url = "https://shitstorm.ovh/eve";

        static string EvePathToUrl(in string eve_path) => eve_url + "/" + eve_path;

        //--------------------------------------------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoad()
        {
            domain_eve = Command.static_domain.AddRoutine(
                "satellite",
                routine: EMain,
                aliases: "eve"
                );

            cmd_ls = domain_eve.AddAction(
                "ls",
                action: null);

            cmd_cat = domain_eve.AddAction(
                "cat",
                min_args: 1,
                args: static exe =>
                {
                    if (exe.line.TryReadArgument(out string arg, out _))
                        exe.args.Add(arg);
                },
                action: null);

            cmd_cd = domain_eve.AddAction(
                "cd",
                min_args: 1,
                args: static exe =>
                {
                    if (exe.line.TryReadArgument(out string arg, out _))
                        exe.args.Add(arg);
                },
                action: null);

            static IEnumerator<CMD_STATUS> EMain(Command.Executor eve_exe)
            {
                List<string> eve_path_list = new();

                string EvePathStr() => eve_path_list.Join("/").TrimEnd('/');
                string EvePrefixe() => eve_exe.shell.GetPrefixe(cmd_path: "[EVE]" + EvePathToUrl(EvePathStr()));

                yield return new(CMD_STATES.WAIT_FOR_STDIN, prefixe: EvePrefixe());

                while (true)
                {
                    if (domain_eve.TryReadCommand_path(eve_exe.line, out var path))
                    {
                        Command.Executor cmd_exe = new(eve_exe.shell, eve_exe, eve_exe.line, path);
                        if (cmd_exe.error != null)
                            eve_exe.error = cmd_exe.error;
                        else if (eve_exe.line.flags.HasFlag(SIG_FLAGS.TICK))
                        {
                            IEnumerator<float> routine = null;

                            if (cmd_exe.command == cmd_ls)
                                routine = E_ls(eve_exe, cmd_exe, EvePathStr());

                            if (cmd_exe.command == cmd_cd)
                                ChangeDirectory(eve_exe, cmd_exe, EvePathStr(), eve_path_list);

                            if (cmd_exe.command == cmd_cat)
                                routine = EReadFile(eve_exe, cmd_exe, EvePathStr());

                            cmd_exe.line = eve_exe.line;
                            if (routine != null)
                                while (routine.MoveNext())
                                {
                                    yield return new CMD_STATUS(CMD_STATES.BLOCKING, progress: routine.Current);
                                    cmd_exe.line = eve_exe.line;
                                }
                        }

                        if (cmd_exe.error != null)
                            if (eve_exe.line.flags.HasFlag(SIG_FLAGS.CHECK))
                                Debug.LogWarning($"[EVE_WARN] {cmd_exe.error}");
                            else if (eve_exe.line.flags.HasFlag(SIG_FLAGS.TICK))
                                Debug.LogError($"[EVE_ERROR] {cmd_exe.error}");

                        cmd_exe.Dispose();
                    }
                    yield return new(CMD_STATES.WAIT_FOR_STDIN, prefixe: EvePrefixe());
                }
            }
        }
    }
}