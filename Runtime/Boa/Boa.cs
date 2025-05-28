using UnityEngine;

namespace _COBRA_
{
    public static partial class Boa
    {
        static Command cmd_boa;

        //--------------------------------------------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoad()
        {
            cmd_boa = Command.static_domain.AddDomain(
                "boa",
                manual: new("create and run your own Boa scripts :)")
                );

            Init_Run();
        }
    }
}