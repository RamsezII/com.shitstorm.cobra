using System.Collections.Generic;
using UnityEngine;

namespace _COBRA_.Boa.contracts
{
    static class Cmd_tests
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoad()
        {
            DevContract.AddContract(new(
                name: "echo",
                output_type: typeof(object),
                arguments: new() { typeof(object), },
                action_SIG_EXE: static (janitor, args) =>
                {
                    var cell = args.arguments[0];
                    janitor.vstack.Add(cell);
                }
            ));

            DevContract.AddContract(new(
                name: "wait_scaled",
                arguments: new() { typeof(float), },
                routine_SIG_EXE: static (janitor, args) =>
                {
                    var cell = args.arguments[0];
                    float delay = cell.value;

                    return ERoutine(delay);

                    static IEnumerator<ExecutionOutput> ERoutine(float delay)
                    {
                        float start = Time.time;
                        float end = start + delay;
                        while (Time.time < end)
                            yield return new(CMD_STATUS.BLOCKED, progress: Mathf.InverseLerp(start, end, Time.time));
                    }
                }
            ));

            DevContract.AddContract(new(
                name: "wait_unscaled",
                arguments: new() { typeof(float), },
                routine_SIG_EXE: static (janitor, args) =>
                {
                    var cell = args.arguments[0];
                    float delay = cell.value;

                    return ERoutine(delay);

                    static IEnumerator<ExecutionOutput> ERoutine(float delay)
                    {
                        float start = Time.unscaledTime;
                        float end = start + delay;
                        while (Time.unscaledTime < end)
                            yield return new(CMD_STATUS.BLOCKED, progress: Mathf.InverseLerp(start, end, Time.unscaledTime));
                    }
                }
            ));
        }
    }
}