using UnityEngine;

namespace _COBRA_
{
    internal static partial class CmdMedia
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoad()
        {
            Init_Music();
        }
    }
}