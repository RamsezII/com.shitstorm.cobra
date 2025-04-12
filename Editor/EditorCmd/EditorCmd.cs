using _COBRA_;
using _UTIL_e;
using System.Collections.Generic;
using UnityEngine;

namespace _COBRA_e
{
    internal static class EditorCmd
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoad()
        {
            const string
                flag_blocking = "--blocking";

            Shell.static_domain.AddRoutine(
                "push-all-git-repos",
                min_args: 1,
                opts: static exe =>
                {
                    if (exe.line.TryRead_one_flag(exe, out string flag, flag_blocking))
                        exe.opts.Add(flag_blocking, null);
                },
                args: static exe =>
                {
                    if (exe.line.TryReadArgument(out string arg))
                        exe.args.Add(arg);
                },
                routine: ERoutine);

            static IEnumerator<CMD_STATUS> ERoutine(Command.Executor exe)
            {
                string arg = (string)exe.args[0];
                bool blocking = exe.opts.ContainsKey(flag_blocking);

                if (blocking)
                    GitBatchPusher.PushAllGitRepos(arg);
                else
                {
                    var routine = GitBatchPusher.EPushAllGitRepos(arg);
                    while (routine.MoveNext())
                        yield return new CMD_STATUS(CMD_STATES.BLOCKING, progress: routine.Current);
                }
            }
        }
    }
}