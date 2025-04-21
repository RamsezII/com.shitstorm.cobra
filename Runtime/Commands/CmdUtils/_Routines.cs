using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _COBRA_
{
    partial class CmdUtils
    {
        static readonly Command cmd_routinize = new(
            "_routinize",
            min_args: 1,
            args: static exe => exe.args.Add(null),
            routine: ERoutinize
            );

        //--------------------------------------------------------------------------------------------------------------

        static IEnumerator<CMD_STATUS> ERoutinize(Command.Executor exe)
        {
            object[] datas = exe.args[0].ExtractDataArray();
            for (int i = 0; i < datas.Length; i++)
            {
                CMD_STATUS status = new(CMD_STATES.BLOCKING, progress: (float)i / datas.Length);

                do yield return status;
                while (!exe.line.HasFlags_any(SIGNALS.EXEC | SIGNALS.TICK));

                exe.Stdout(datas[i]);
            }
        }

        static void Init_Routines()
        {
            Command.static_domain.AddPipe(
                "routinize",
                on_pipe: static (exe, data) =>
                {
                    Command.Executor exe2 = new(exe.shell, exe, exe.line, new() { cmd_routinize, })
                    {
                        stdout_exe = exe.stdout_exe,
                        next_exe = exe.next_exe,
                    };

                    if (exe.background)
                        exe2.PropagateBackground();

                    exe2.args[0] = data;

                    exe.janitor.AddExecutor(exe.line, exe2);
                });

            Command.static_domain.AddRoutine(
                "tick",
                routine: ETick);

            Command.static_domain.AddRoutine(
                "wait-frames",
                min_args: 1,
                args: static exe =>
                {
                    if (exe.line.TryReadInt(out int value))
                        exe.args.Add(value);
                },
                routine: EWaitFrames);

            Command.static_domain.AddRoutine(
                "wait-seconds",
                min_args: 1,
                args: static exe =>
                {
                    if (exe.line.TryReadFloat(out float value))
                        exe.args.Add(value);
                },
                routine: EWaitSeconds);

            static IEnumerator<CMD_STATUS> ETick(Command.Executor exe)
            {
                do yield return new CMD_STATUS(CMD_STATES.BLOCKING);
                while (!exe.line.signal.HasFlag(SIGNALS.TICK));

                exe.Stdout(null);
            }

            static IEnumerator<CMD_STATUS> EWaitFrames(Command.Executor exe)
            {
                int wait_amount = (int)exe.args[0];
                int frames = 0;

                do
                {
                    if (exe.line.signal.HasFlag(SIGNALS.TICK))
                        ++frames;
                    yield return new CMD_STATUS(CMD_STATES.BLOCKING, progress: Mathf.InverseLerp(0, wait_amount, frames));
                }
                while (frames < wait_amount || !exe.line.HasFlags_any(SIGNALS.EXEC | SIGNALS.TICK));

                exe.Stdout(null);
            }

            static IEnumerator<CMD_STATUS> EWaitSeconds(Command.Executor exe)
            {
                float wait_seconds = (float)exe.args[0];
                float timer = 0;

                do
                {
                    if (exe.line.signal.HasFlag(SIGNALS.TICK))
                        timer += Time.unscaledDeltaTime;
                    yield return new CMD_STATUS(CMD_STATES.BLOCKING, progress: Mathf.InverseLerp(0, wait_seconds, timer));
                }
                while (timer < wait_seconds || !exe.line.signal.HasFlag(SIGNALS.TICK));

                exe.Stdout(null);
            }
        }
    }
}