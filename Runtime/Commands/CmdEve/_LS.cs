using _UTIL_;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace _COBRA_
{
    partial class CmdEve
    {
        static IEnumerator<float> E_ls(Command.Executor eve_exe, Command.Executor cmd_exe, string eve_path)
        {
            static void Output(in Command.Executor eve_exe, in Command.Executor cmd_exe, in NGinxIndex index)
            {
                StringBuilder sb = new(), sb_lint = new();

                for (int i = 0; i < index.entries.Length; ++i)
                {
                    NGinxIndex.Entry entry = index.entries[i];
                    sb.Append(entry.name);
                    sb_lint.Append($"{entry.name} ".SetColor(entry.GetEntryType switch
                    {
                        NGinxIndex.TYPES.DIRECTORY => eve_exe.line.linter.directory,
                        NGinxIndex.TYPES.FILE => eve_exe.line.linter.file,
                        _ => eve_exe.line.linter.path,
                    }));
                }

                cmd_exe.Stdout(sb.ToString(), sb_lint.ToString());
            }

            string request_url = EvePathToUrl(eve_path);
            using UnityWebRequest request = UnityWebRequest.Get(request_url);
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();

            while (!operation.isDone)
                yield return operation.progress;

            if (request.result != UnityWebRequest.Result.Success)
                Debug.Log($"[EVE] failed to get index: \"{request.result}\" ({nameof(eve_url)}: '{eve_url}')");
            else if (!request.downloadHandler.text.TryExtractIndex_FromNGinxText(out NGinxIndex index, out string nginx_error))
                Debug.LogError($"[EVE] {nginx_error}");
            else
                Output(eve_exe, cmd_exe, index);
        }
    }
}