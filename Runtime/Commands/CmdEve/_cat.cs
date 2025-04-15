using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace _COBRA_
{
    partial class CmdEve
    {
        static IEnumerator<float> EReadFile(Command.Executor eve_exe, Command.Executor cmd_exe, string eve_path)
        {
            string file_name = (string)cmd_exe.args[0];

            string request_url = EvePathToUrl(eve_path + "/" + file_name);
            using UnityWebRequest request = UnityWebRequest.Get(request_url);
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();

            while (!operation.isDone)
                yield return operation.progress;

            if (request.result == UnityWebRequest.Result.Success)
                cmd_exe.Stdout(request.downloadHandler.text[..^1]);
            else
                Debug.Log($"[EVE] failed to get index: \"{request.result}\" ({nameof(eve_url)}: '{eve_url}')");
        }
    }
}