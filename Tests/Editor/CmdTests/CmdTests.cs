using _COBRA_;
using System.Collections.Generic;
using UnityEngine;

namespace _COBRA_et
{
    internal static partial class CmdTests
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoad()
        {
            Command.static_domain.AddAction(
                "test-completion",
                min_args: 1,
                args: static exe =>
                {
                    if (exe.signal.TryReadArgument(out string arg, out bool is_candidate, new string[] { "tiz", "taz", "toz", }) && is_candidate)
                        exe.args.Add(arg);
                    else
                        exe.error = $"invalid arg ('{exe.signal.arg_last}')";
                },
                action: static exe =>
                {
                    exe.Stdout(exe.args[0]);
                });

            Command.static_domain.AddRoutine(
                "read-stdin",
                routine: EStdinTest);

            static IEnumerator<CMD_STATUS> EStdinTest(Command.Executor exe)
            {
                yield return new CMD_STATUS(CMD_STATES.WAIT_FOR_STDIN, "balls:");
                yield return new CMD_STATUS(CMD_STATES.WAIT_FOR_STDIN, "balls:");

                while (true)
                {
                    exe.signal.TryReadArgument(out string arg, out _);
                    if (exe.signal.flags.HasFlag(SIG_FLAGS.EXEC))
                    {
                        exe.Stdout(arg, signal: exe.signal);
                        yield break;
                    }
                    yield return new CMD_STATUS(CMD_STATES.WAIT_FOR_STDIN, "balls:");
                }
            }
        }
    }
}
