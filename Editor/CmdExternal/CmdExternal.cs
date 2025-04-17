using System;
using System.Collections.Generic;
using System.IO;
using _COBRA_;
using UnityEngine;

namespace _COBRA_e
{
    internal static partial class CmdExternal
    {
        static Command domain_git;

        const string
            opt_workdir = "--working-directory";

        //--------------------------------------------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoad()
        {
            domain_git = Command.static_domain.AddDomain("git");

            Init_PushAll();
            Init_Scan();
            Init_CmdExt();
        }
    }
}