using UnityEngine;

namespace _COBRA_
{
    static internal partial class CmdBoa
    {
        static Command cmd_boa;

        //--------------------------------------------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoad()
        {
            cmd_boa = Command.static_domain.AddDomain(
                "boa",
                manual: new("create and run your own shitstorm scripts :)")
                );

            Init_Run();
        }
    }
}