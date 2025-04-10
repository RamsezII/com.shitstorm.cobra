using System.Collections.Generic;
using UnityEngine;

namespace _COBRA_
{
    public sealed partial class Shell : MonoBehaviour
    {
        public static readonly Command static_domain = new("shell_root");

        internal readonly List<Command.Executor> active_executors_stack = new();
        internal readonly Queue<Command.Executor> pending_executors_queue = new();
        internal readonly List<Command.Executor> background_executors = new();

        static byte id_counter = 0;
        public readonly byte id = ++id_counter;

        internal CMD_STATUS status;
        public bool IsIdle => active_executors_stack.Count == 0;
        public float Progress => status.progress;
        public CMD_STATUS CurrentStatus => status;

        //--------------------------------------------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnBeforeSceneLoad()
        {
            id_counter = 0;
            static_domain.PropagateOblivion();
        }

        //--------------------------------------------------------------------------------------------------------------

        private void OnDestroy()
        {
            foreach (var executor in active_executors_stack)
                executor.Dispose();
            active_executors_stack.Clear();

            foreach (var executor in pending_executors_queue)
                executor.Dispose();
            pending_executors_queue.Clear();

            foreach (var executor in background_executors)
                executor.Dispose();
            background_executors.Clear();
        }
    }
}