using _ARK_;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace _COBRA_
{
    public sealed partial class Shell : MonoBehaviour
    {
        public static readonly Command static_domain = new("shell_root");

        internal readonly List<ExecutorPipeline> active_exe_pipelines_stack = new();
        internal readonly Queue<Command.Executor> pending_executors_queue = new();
        internal readonly List<Command.Executor> background_executors = new();

        static byte id_counter = 0;
        public readonly byte id = ++id_counter;

        internal CMD_STATUS status;
        public bool IsIdle => active_exe_pipelines_stack.Count == 0;
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

            for (int i = 0; i < active_exe_pipelines_stack.Count; i++)
            {
                ExecutorPipeline pipeline = active_exe_pipelines_stack[i];
                for (int j = 0; j < pipeline.executors.Count; j++)
                    pipeline.executors[j].Dispose();
            }

            active_exe_pipelines_stack.Clear();

            foreach (var executor in pending_executors_queue)
                executor.Dispose();
            pending_executors_queue.Clear();

            for (int i1 = 0; i1 < background_executors.Count; i1++)
                background_executors[i1].Dispose();

            background_executors.Clear();
        }
    }
}