using _UTIL_;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.Networking;

namespace _COBRA_
{
    partial class CmdEve
    {
        static IEnumerator<CMD_STATUS> ELS(Command.Executor eve_exe, Command.Executor cmd_exe, string request_url)
        {
            using UnityWebRequest request = UnityWebRequest.Get(request_url);
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();

            while (!operation.isDone)
                yield return new(CMD_STATES.BLOCKING, progress: operation.progress);

            if (request.result != UnityWebRequest.Result.Success)
                cmd_exe.error = $"[EVE] failed to get index: \"{request.result}\"";
            else if (request.downloadHandler.text.TryExtractIndex_FromNGinxText(out var index, out string nginx_error))
                if (index.entries.Length == 0)
                    cmd_exe.Stdout("No entries found");
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

                    cmd_exe.Stdout($"{directories.Count} {nameof(directories).SetColor(eve_exe.line.linter.directory)} ; {files.Count} {nameof(files).SetColor(eve_exe.line.linter.file)}");

                    for (int i = 0; i < directories.Count; ++i)
                        cmd_exe.Stdout(directories[i].SetColor(eve_exe.line.linter.directory));

                    for (int i = 0; i < files.Count; ++i)
                        cmd_exe.Stdout(files[i].SetColor(eve_exe.line.linter.file));
                }
            else
                cmd_exe.error = $"[EVE] {nginx_error}";
        }
    }
}