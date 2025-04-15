using _UTIL_;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace _COBRA_
{
    static internal class CmdEve
    {
        enum EveCmds : byte
        {
            ls,
            cd,
            cat,
        }

        static readonly HashSet<string> commands_names = new(Enum.GetNames(typeof(EveCmds)), StringComparer.OrdinalIgnoreCase);

        static readonly Dictionary<string, NGinxIndex.Entry> eve_tree = new();

        static Command domain_eve;

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

            domain_eve.AddAction(
                "ls",
                action: static exe =>
                {
                    exe.Stdout(null);
                });

            domain_eve.AddAction(
                "cat",
                action: static exe =>
                {
                    exe.Stdout(null);
                });

            domain_eve.AddAction(
                "cd",
                action: static exe =>
                {
                    exe.Stdout(null);
                });

            static IEnumerator<CMD_STATUS> EMain(Command.Executor exe)
            {
                string request_url = $"https://shitstorm.ovh/texts";
                string EvePrefixe() => exe.shell.GetPrefixe(cmd_path: "[EVE]" + request_url);

                yield return new(CMD_STATES.WAIT_FOR_STDIN, prefixe: EvePrefixe());

                while (true)
                {
                    if (domain_eve.TryReadCommand_path(exe.line, out var path))
                    {
                        Command.Executor exe2 = new(exe.shell, exe, exe.line, path);
                        if (exe2.error != null)
                            exe.error = exe2.error;
                        else if (exe.line.signal.HasFlag(SIGNALS.EXEC))
                            exe2.Stdout(exe2.cmd_path);
                        continue;
                    }

                    if (false)
                        if (exe.line.TryReadArgument(out string cmd_name, out bool is_candidate, commands_names, false))
                            if (!Enum.TryParse(cmd_name, true, out EveCmds code))
                                exe.error = $"[EVE] Invalid command '{exe.line.arg_last}'";
                            else
                                switch (code)
                                {
                                    case EveCmds.ls:
                                        if (exe.line.signal.HasFlag(SIGNALS.EXEC))
                                        {
                                            using UnityWebRequest request = UnityWebRequest.Get(request_url);
                                            UnityWebRequestAsyncOperation operation = request.SendWebRequest();

                                            while (!operation.isDone)
                                                yield return new(CMD_STATES.BLOCKING, progress: operation.progress, prefixe: EvePrefixe());

                                            if (request.result != UnityWebRequest.Result.Success)
                                                exe.error = $"[EVE] failed to get index: \"{request.result}\"";
                                            else if (request.downloadHandler.text.TryExtractIndex_FromNGinxText(out var index, out string nginx_error))
                                                if (index.entries.Length == 0)
                                                    exe.Stdout("No entries found");
                                                else
                                                {
                                                    List<string> directories = new();
                                                    List<string> files = new();

                                                    foreach (var entry in index.entries.OrderBy(entry => entry.name, StringComparer.OrdinalIgnoreCase))
                                                        switch (entry.GetEntryType)
                                                        {
                                                            case NGinxIndex.TYPES.DIRECTORY:
                                                                directories.Add(entry.name);
                                                                break;

                                                            case NGinxIndex.TYPES.FILE:
                                                                files.Add(entry.name);
                                                                break;
                                                        }

                                                    exe.Stdout($"{directories.Count} {nameof(directories).SetColor(exe.line.linter.directory)} ; {files.Count} {nameof(files).SetColor(exe.line.linter.file)}");

                                                    for (int i = 0; i < directories.Count; ++i)
                                                        exe.Stdout(directories[i].SetColor(exe.line.linter.directory));

                                                    for (int i = 0; i < files.Count; ++i)
                                                        exe.Stdout(files[i].SetColor(exe.line.linter.file));
                                                }
                                            else
                                                exe.error = $"[EVE] {nginx_error}";
                                        }
                                        break;

                                    case EveCmds.cd:
                                        break;

                                    case EveCmds.cat:
                                        break;
                                }
                    yield return new(CMD_STATES.WAIT_FOR_STDIN, prefixe: EvePrefixe());
                }
            }
        }
    }
}