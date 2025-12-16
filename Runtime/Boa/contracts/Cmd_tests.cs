using UnityEngine;

namespace _COBRA_.Boa.contracts
{
    static class Cmd_tests
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoad()
        {
            DevContract.AddContract(new(
                name: "test",
                output_type: typeof(bool),
                arguments: new() { typeof(bool), },
                action_SIG_EXE: static (janitor, prms) =>
                {
                    bool val = (bool)prms.arguments[0].value;
                    janitor.vstack.Add(new(val));
                    janitor.shell.on_output(val, null);
                }
            ));
        }
    }
}