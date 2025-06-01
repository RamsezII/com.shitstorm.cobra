using System.Collections.Generic;

namespace _COBRA_
{
    partial class CmdUtils
    {
        static void Init_StdinRead()
        {
            Command.static_domain.AddRoutine(
                "read-stdin",
                max_args: 1,
                args: static exe =>
                {
                    if (exe.line.TryReadArgument(out string arg, out _))
                        exe.args.Add(arg);
                },
                routine: EStdinTest);

            static IEnumerator<CMD_STATUS> EStdinTest(Command.Executor exe)
            {
                string prefixe = exe.args.Count == 0 ? exe.command.name + ":" : (string)exe.args[0];
                while (true)
                {
                    exe.line.TryReadArgument(out string arg, out _);
                    if (exe.line.flags.HasFlag(SIG_FLAGS.SUBMIT))
                    {
                        exe.Stdout(arg, line: exe.line);
                        yield break;
                    }
                    yield return new CMD_STATUS(CMD_STATES.WAIT_FOR_STDIN, prefixe);
                }
            }
        }
    }
}