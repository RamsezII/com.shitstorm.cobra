using System.Collections.Generic;
using System.IO;
using _UTIL_;
using UnityEngine;
using UnityEngine.Networking;

namespace _COBRA_
{
    partial class CmdMedia
    {
        static void Init_Music()
        {
            const string
                flag_url = "--is-web-url";

            Command.static_domain.AddRoutine(
                "play-music-at-path",
                min_args: 1,
                opts: static exe =>
                {
                    if (exe.line.TryRead_one_flag(exe, flag_url))
                        exe.opts.Add(flag_url, null);
                },
                args: static exe =>
                {
                    bool is_url = exe.opts.ContainsKey(flag_url);
                    if (exe.line.TryReadArgument(out string path, out bool is_candidate, path_mode: FS_TYPES.FILE))
                        if (is_url)
                            exe.args.Add(path);
                        else
                        {
                            string full_path = exe.shell.PathCheck(path, PathModes.ForceFull);
                            if (File.Exists(full_path))
                                exe.args.Add(full_path);
                        }
                },
                routine: ERoutine);

            static IEnumerator<CMD_STATUS> ERoutine(Command.Executor exe)
            {
                bool is_url = exe.opts.ContainsKey(flag_url);
                string url = (string)exe.args[0];
                using UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.UNKNOWN);
                UnityWebRequestAsyncOperation operation = request.SendWebRequest();

                while (!operation.isDone || !exe.line.HasFlags_any(SIG_FLAGS.EXEC | SIG_FLAGS.TICK))
                    yield return new CMD_STATUS(CMD_STATES.BLOCKING, progress: operation.progress);

                if (request.result != UnityWebRequest.Result.Success)
                    Debug.LogError($"erreur chargement audio ({url}): " + request.error);
                else
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(request);

                    AudioSource source = null;
                    try
                    {
                        source = Util.InstantiateOrCreate<AudioSource>();
                        Object.DontDestroyOnLoad(source.gameObject);

                        source.playOnAwake = false;
                        source.loop = false;
                        source.spatialBlend = 0;
                        source.clip = clip;
                        source.Play();

                        while (source.time < clip.length)
                            yield return new CMD_STATUS(CMD_STATES.BLOCKING, progress: source.time / clip.length);
                    }
                    finally
                    {
                        Object.Destroy(clip);
                        if (source != null)
                            Object.Destroy(source.gameObject);
                    }
                }
            }
        }
    }
}