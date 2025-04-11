using System.Collections.Generic;
using UnityEngine;

namespace _COBRA_
{
    internal static class CmdTests
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoad()
        {
            Shell.static_domain.AddAction(
                "test-options",
                action: exe => exe.Stdout(exe.args.LinesToText())
,
                opts: exe =>
                {
                    if (exe.line.TryRead_flags(exe, out var flags, "-m", "--meaning", "--enhance"))
                        foreach (var flag in flags)
                            exe.opts.Add(flag, null);
                },
                args: exe =>
                {
                    if (exe.line.TryReadArgument(out string arg, new[] { "bush", "fire", "word", }))
                        exe.args.Add(arg);
                });

            Shell.static_domain.AddRoutine(
                "routine-test",
                min_args: 1,
                opts: null,
                args: exe =>
                {
                    if (exe.line.TryReadArgument(out string arg))
                        exe.args.Add(int.Parse(arg));
                },
                routine: ERoutineTest);

            static IEnumerator<CMD_STATUS> ERoutineTest(Command.Executor exe)
            {
                try
                {
                    int loops = (int)exe.args[0];
                    for (int i = 0; i < loops; ++i)
                    {
                        exe.Stdout(i);
                        float timer = 0;
                        while (timer < 1)
                        {
                            if (exe.line.signal.HasFlag(SIGNAL_FLAGS.TICK))
                                timer += 3 * Time.deltaTime;

                            yield return new CMD_STATUS()
                            {
                                state = CMD_STATES.BLOCKING,
                                progress = (i + timer) / loops,
                            };
                        }
                    }
                }
                finally
                {
                    Debug.Log($"{exe} disposed".ToSubLog());
                }
            }
        }
    }
}