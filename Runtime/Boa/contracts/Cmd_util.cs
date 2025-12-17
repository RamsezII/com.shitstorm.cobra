using System;
using UnityEngine;

namespace _COBRA_.Boa.contracts
{
    static class Cmd_util
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init()
        {
            DevContract.AddContract(new(
                name: "typeof",
                output_type: typeof(Type),
                arguments: new() { typeof(object), },
                action: static (janitor, prms) =>
                {
                    MemCell cell = prms.arguments[0];
                    Type type = cell._type;
                    janitor.vstack.Add(new(type));
                }
            ));
        }
    }
}