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
                targs: new() { typeof(bool), },
                action_SIG_EXE: static (janitor, prms) =>
                {
                    bool val = (bool)janitor.mem_stack.Pop().value;
                    janitor.shell.on_output(val, null);
                }
            ));
        }
    }
}