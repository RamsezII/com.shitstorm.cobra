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
                name: "echo",
                output_type: typeof(object),
                arguments: new() { typeof(object), },
                action: static (janitor, args) =>
                {
                    var cell = args.arguments[0];
                    janitor.vstack.Add(cell);
                }
            ));

            DevContract.AddContract(new(
                name: "read_entry",
                output_type: typeof(string),
                arguments: new() { typeof(string), },
                routine_READER: static (janitor, prms) =>
                {
                    string entry = prms.arguments[0]._value.ToString();
                    return ERoutine(janitor, entry);
                    static IEnumerator<ExecutionStatus> ERoutine(Janitor janitor, string entry)
                    {
                        while (janitor.reader == null || !janitor.reader.sig_flags.HasFlag(SIG_FLAGS.SUBMIT))
                            yield return new(CMD_STATUS.WAIT_FOR_STDIN, prefixe: new(entry));

                        string read = janitor.reader.ReadAll();
                        janitor.vstack.Add(new MemCell(read));
                    }
                }
            ));

            DevContract.AddContract(new(
                name: "path",
                arguments: new() { typeof(BoaPath), },
                output_type: typeof(string),
                action: static (janitor, prms) =>
                {
                    MemCell cell = prms.arguments[0];
                    janitor.vstack.Add(cell);
                }
            ));

            DevContract.AddContract(new(
                name: "wait_scaled",
                arguments: new() { typeof(float), },
                routine: static (janitor, args) =>
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
                routine: static (janitor, args) =>
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