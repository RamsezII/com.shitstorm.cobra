using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace _COBRA_
{
    partial class CmdEve
    {
        static IEnumerator<float> EInitRoot()
        {
            using UnityWebRequest request = UnityWebRequest.Get(eve_url);
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();

            while (!operation.isDone)
                yield return operation.progress;

            if (request.result != UnityWebRequest.Result.Success)
                Debug.Log($"[EVE] failed to get index: \"{request.result}\" ({nameof(eve_url)}: '{eve_url}')");
            else if (!request.downloadHandler.text.TryExtractIndex_FromNGinxText(out var index, out string nginx_error))
                Debug.LogError($"[EVE] {nginx_error}");
            else
                eve_tree.Add("eve", index);
        }
    }
}