using _COBRA_;
using UnityEngine;

namespace _COBRA_et
{
    internal static partial class CmdTests
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoad()
        {
            Command.static_domain.AddAction(
                "test-completion",
                min_args: 1,
                args: static exe =>
                {
                    if (exe.line.TryReadArgument(out string arg, out bool is_candidate, new string[] { "tiz", "taz", "toz", }) && is_candidate)
                        exe.args.Add(arg);
                    else
                        exe.error = $"invalid arg ('{exe.line.arg_last}')";
                },
                action: static exe =>
                {
                    exe.Stdout(exe.args[0]);
                });
        }
    }
}
