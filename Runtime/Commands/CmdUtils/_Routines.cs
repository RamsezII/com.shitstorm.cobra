using System.Collections.Generic;
using UnityEngine;

namespace _COBRA_
{
    partial class CmdUtils
    {
        static void Init_Routines()
        {
            Command.static_domain.AddRoutine(
                "wait-seconds",
                min_args: 1,
                args: static exe =>
                {

                },
                routine: EWaitSeconds);

            Command.static_domain.AddRoutine(
                "wait-frames",
                min_args: 1,
                args: static exe =>
                {
                    if (exe.line.TryReadInt(out int value))
                        exe.args.Add(value);
                },
                routine: EWaitFrames);

            static IEnumerator<CMD_STATUS> ERoutinize(Command.Executor exe)
            {
                string arg = (string)exe.args[0];
                yield break;
            }

            static IEnumerator<CMD_STATUS> EWaitSeconds(Command.Executor exe)
            {
                yield break;
            }

            static IEnumerator<CMD_STATUS> EWaitFrames(Command.Executor exe)
            {
                int value = (int)exe.args[0];
                int start_frame = Time.frameCount;

                while (Time.frameCount < start_frame + value || !exe.line.HasFlags_any(SIGNALS.EXEC | SIGNALS.TICK))
                    yield return new CMD_STATUS(CMD_STATES.BLOCKING, progress: Mathf.InverseLerp(start_frame, start_frame + value, Time.frameCount));
            }
        }
    }
}