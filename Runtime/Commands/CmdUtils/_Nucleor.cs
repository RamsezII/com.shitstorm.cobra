﻿using System.Collections.Generic;

namespace _COBRA_
{
    partial class CmdUtils
    {
        static void Init_Nucleor()
        {
            Command domain_nucleor = Command.static_domain.AddDomain("nucleor");

            domain_nucleor.AddRoutine("monitor", routine: ERoutine);

            static IEnumerator<CMD_STATUS> ERoutine(Command.Executor exe)
            {
                while (true)
                    yield return new CMD_STATUS(CMD_STATES.FULLSCREEN_readonly);
            }
        }
    }
}