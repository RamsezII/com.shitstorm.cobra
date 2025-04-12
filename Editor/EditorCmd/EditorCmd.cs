using _COBRA_;
using _UTIL_e;
using UnityEngine;

namespace _COBRA_e
{
    internal static class EditorCmd
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoad()
        {
            Shell.static_domain.AddAction(
                "push-all-git-repos",
                min_args: 1,
                args: static exe =>
                {
                    if (exe.line.TryReadArgument(out string arg))
                        exe.args.Add(arg);
                },
                action: static exe =>
                {
                    string arg = (string)exe.args[0];
                    GitBatchPusher.PushAllGitRepos(arg);
                });
        }
    }
}