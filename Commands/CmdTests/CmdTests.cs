using System.Collections.Generic;
using UnityEngine;

namespace _COBRA_
{
    internal static class CmdTests
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoad()
        {
            Command.cmd_root_shell.AddCommand(new(
                "test-options",
                args: exe =>
                {
                    if (exe.line.TryReadFlags(exe, out var flags, "-m", "--meaning", "--enhance"))
                        foreach (var flag in flags)
                            exe.args.Add(flag);
                    if (exe.line.TryReadArgument(out string arg, new[] { "bush", "fire", "word", }))
                        exe.args.Add(arg);
                },
                action: exe => exe.Stdout(exe.args.LinesToText())
                ));

            Command.cmd_root_shell.AddCommand(new(
                "routine-test",
                args: exe =>
                {
                    if (exe.line.TryReadArgument(out string arg))
                        exe.args.Add(int.Parse(arg));
                },
                routine: ERoutineTest));

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
                    Debug.Log($"'{exe.cmd_name}' ({exe.cmd_path}) disposed".ToSubLog());
                }
            }
        }
    }
}