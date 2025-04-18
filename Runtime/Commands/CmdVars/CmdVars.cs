using UnityEngine;

namespace _COBRA_
{
    internal static partial class CmdVars
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoad()
        {
            Init_Vars();
            Init_Equals();
            Init_For();
            Init_Range();
        }
    }
}