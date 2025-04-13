using _COBRA_;
using UnityEngine;

namespace _COBRA_e
{
    internal static partial class GitCmd
    {
        static Command domain_git;

        //--------------------------------------------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoad()
        {
            domain_git = Command.static_domain.AddDomain("git");

            Init_PushAll();
        }
    }
}