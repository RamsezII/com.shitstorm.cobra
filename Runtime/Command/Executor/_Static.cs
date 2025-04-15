using System.Collections.Generic;
using UnityEngine;

namespace _COBRA_
{
    partial class Command
    {
        partial class Executor
        {
            internal static readonly HashSet<Executor> instances = new();

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
            }
        }
    }
}