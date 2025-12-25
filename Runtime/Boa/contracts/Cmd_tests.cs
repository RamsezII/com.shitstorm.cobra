using System.Collections.Generic;
using UnityEngine;

namespace _COBRA_.Boa.contracts
{
    static class Cmd_tests
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            DevContract.AddContract(new(
                name: "path",
                arguments: new() { typeof(BoaPath), },
                output_type: typeof(string),
                action: static (memstack, memscope, prms) =>
                {
                    MemCell cell = prms.arguments[0];
                    memstack.Add(cell);
                }
            ));

            DevContract.AddContract(new(
                name: "wait_scaled",
                arguments: new() { typeof(float), },
                routine: static (memstack, memscope, args) =>
                {
                    MemCell cell = args.arguments[0];
                    float delay = cell;

                    return ERoutine(delay);

                    static IEnumerator<ExecutionStatus> ERoutine(float delay)
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
                routine: static (memstack, memscope, args) =>
                {
                    MemCell cell = args.arguments[0];
                    float delay = cell;

                    return ERoutine(delay);

                    static IEnumerator<ExecutionStatus> ERoutine(float delay)
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