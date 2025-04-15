using _UTIL_;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace _COBRA_
{
    static internal partial class CmdEve
    {
        static readonly Dictionary<string, NGinxIndex.Entry> eve_tree = new();

        static Command domain_eve, cmd_ls, cmd_cat, cmd_cd;
        const string request_url = "https://shitstorm.ovh/texts";
        static string CombinedPath(in string eve_path) => Path.Combine(request_url, eve_path).Replace("\\", "/");

        //--------------------------------------------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnBeforeSceneLoad()
        {
            eve_tree.Clear();
        }

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
                action: null);

            cmd_cd = domain_eve.AddAction(
                "cd",
                action: null);

            static IEnumerator<CMD_STATUS> EMain(Command.Executor eve_exe)
            {
                string EvePrefixe() => eve_exe.shell.GetPrefixe(cmd_path: "[EVE]" + request_url);

                string eve_path = string.Empty;

                yield return new(CMD_STATES.WAIT_FOR_STDIN, prefixe: EvePrefixe());

                while (true)
                {
                    if (domain_eve.TryReadCommand_path(eve_exe.line, out var path))
                    {
                        Command.Executor cmd_exe = new(eve_exe.shell, eve_exe, eve_exe.line, path);
                        if (cmd_exe.error != null)
                            eve_exe.error = cmd_exe.error;
                        else if (eve_exe.line.signal.HasFlag(SIGNALS.EXEC))
                        {
                            IEnumerator<CMD_STATUS> routine = null;

                            if (cmd_exe.command == cmd_ls)
                                routine = ELS(eve_exe, cmd_exe, request_url);

                            do
                                cmd_exe.line = eve_exe.line;
                            while (routine.MoveNext());
                            cmd_exe.line = null;
                        }
                    }
                    yield return new(CMD_STATES.WAIT_FOR_STDIN, prefixe: EvePrefixe());
                }
            }
        }
    }
}