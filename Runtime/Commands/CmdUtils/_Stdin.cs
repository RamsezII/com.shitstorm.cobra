using System.Collections.Generic;

namespace _COBRA_
{
    partial class CmdUtils
    {
        static void Init_StdinRead()
        {
            Command.static_domain.AddRoutine(
                "read-stdin",
                routine: EStdinTest);

            static IEnumerator<CMD_STATUS> EStdinTest(Command.Executor exe)
            {
                while (true)
                {
                    exe.line.TryReadArgument(out string arg, out _);
                    if (exe.line.flags.HasFlag(SIG_FLAGS.SUBMIT))
                    {
                        exe.Stdout(arg, line: exe.line);
                        yield break;
                    }
                    yield return new CMD_STATUS(CMD_STATES.WAIT_FOR_STDIN, "balls:");
                }
            }
        }
    }
}