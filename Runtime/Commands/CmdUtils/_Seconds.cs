using System.Collections.Generic;
using UnityEngine;

namespace _COBRA_
{
    partial class CmdUtils
    {
        static void Init_Seconds()
        {
            const string
                flag_count_down = "--count-down";

            Command.static_domain.AddRoutine(
                "count-seconds",
                min_args: 1,
                opts: static exe =>
                {
                    if (exe.line.TryRead_one_of_the_flags(exe, out string flag, flag_count_down))
                        exe.opts.Add(flag_count_down, true);
                },
                args: static exe =>
                {
                    if (exe.line.TryReadArgument(out string arg))
                        exe.args.Add(int.Parse(arg));
                },
                routine: ERoutineTest);

            static IEnumerator<CMD_STATUS> ERoutineTest(Command.Executor exe)
            {
                bool count_down = exe.opts.ContainsKey(flag_count_down);
                int seconds = (int)exe.args[0];
                for (int i = 0; i <= seconds; ++i)
                {
                    if (count_down)
                        exe.Stdout(seconds - i);
                    else
                        exe.Stdout(i);

                    if (i < seconds)
                    {
                        float timer = 0;
                        while (timer < 1)
                        {
                            if (exe.line.signal.HasFlag(SIGNALS.TICK))
                                timer += 3 * Time.deltaTime;

                            yield return new CMD_STATUS()
                            {
                                state = CMD_STATES.BLOCKING,
                                progress = (i + timer) / seconds,
                            };
                        }
                    }
                }
            }
        }
    }
}