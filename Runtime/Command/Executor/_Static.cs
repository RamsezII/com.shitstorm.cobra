using System.Collections.Generic;
using UnityEngine;

namespace _COBRA_
{
    partial class Command
    {
        partial class Executor
        {
            internal static readonly HashSet<Executor> instances = new();
            internal static readonly Executor exe_log;

            //--------------------------------------------------------------------------------------------------------------

            static Executor()
            {
                if (instances != null)
                {
                    foreach (var instance in instances)
                        instance.Dispose();
                    instances.Clear();
                }
                instances = new();
                PID_counter = 0;

                exe_log?.Dispose();

                exe_log = new(
                    shell: null,
                    parent: null,
                    line: new Line(string.Empty, SIGNALS._none_, null),
                    path: new()
                    {
                        new Command("_log", on_pipe: (exe, args, data) =>
                        {
                            switch (data)
                            {
                                case IEnumerable<string> lines:
                                    foreach (string line in lines)
                                        Debug.Log(line);
                                    break;

                                case string str:
                                    Debug.Log(str);
                                    break;

                                default:
                                    Debug.Log(data);
                                    break;
                            }
                        }),
                    }
                );
            }

            //--------------------------------------------------------------------------------------------------------------

            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
            static void OnAfterSceneLoad()
            {
                Init_Cmd();
            }
        }
    }
}