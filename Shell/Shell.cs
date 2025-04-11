using _ARK_;
using System.Collections.Generic;
using UnityEngine;

namespace _COBRA_
{
    public sealed partial class Shell : MonoBehaviour
    {
        public static readonly Command static_domain = new("shell_root");

        readonly List<ExecutorPipeline> active_executor_pipelines_stack = new();
        readonly Queue<Command.Executor> pending_executors_queue = new();
        readonly List<ExecutorPipeline> background_executors_pipelines = new();

        static byte id_counter = 0;
        public readonly byte id = ++id_counter;

        internal CMD_STATUS status;
        public bool IsIdle => active_executor_pipelines_stack.Count == 0;
        public CMD_STATUS CurrentStatus => status;
        public ITerminal terminal;

        //--------------------------------------------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnBeforeSceneLoad()
        {
            id_counter = 0;
            static_domain.PropagateOblivion();
        }

        //--------------------------------------------------------------------------------------------------------------

        private void Awake()
        {
            terminal = GetComponentInParent<ITerminal>();
            NUCLEOR.delegates.update_shells += TickExecutors;
        }

        //--------------------------------------------------------------------------------------------------------------

        private void OnDestroy()
        {
            NUCLEOR.delegates.update_shells -= TickExecutors;

            foreach (Command.Executor executor in pending_executors_queue)
                executor.Dispose();
            pending_executors_queue.Clear();

            for (int i = 0; i < active_executor_pipelines_stack.Count; i++)
                active_executor_pipelines_stack[i].Dispose();
            active_executor_pipelines_stack.Clear();

            for (int i1 = 0; i1 < background_executors_pipelines.Count; i1++)
                background_executors_pipelines[i1].Dispose();

            background_executors_pipelines.Clear();
        }
    }
}