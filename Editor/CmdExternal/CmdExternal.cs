using UnityEngine;

namespace _COBRA_e
{
    internal static partial class CmdExternal
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoad()
        {
            Init_PushAll();
            Init_Scan();
            Init_Git();
        }
    }
}