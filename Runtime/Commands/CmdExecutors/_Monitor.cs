using System.Collections.Generic;

namespace _COBRA_
{
    partial class CmdExecutors
    {
        static void Init_Monitor()
        {
            domain_exe.AddRoutine(
                "monitor",
                routine: ERoutine
                );

            static IEnumerator<CMD_STATUS> ERoutine(Command.Executor exe)
            {
                yield break;
            }
        }
    }
}