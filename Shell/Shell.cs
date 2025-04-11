﻿using _ARK_;
using System.Collections.Generic;
using UnityEngine;

namespace _COBRA_
{
    public sealed partial class Shell : MonoBehaviour
    {
        public static readonly Command static_domain;

        readonly Queue<Command.Executor> pending_executors = new();
        readonly List<Command.Executor.Janitor> front_janitors = new(), background_janitors = new();

        static byte id_counter = 0;
        public readonly byte shell_ID = ++id_counter;

        internal CMD_STATUS status;
        public bool IsIdle => front_janitors.Count == 0;
        public CMD_STATUS CurrentStatus => status;
        public ITerminal terminal;

        //--------------------------------------------------------------------------------------------------------------

        static Shell()
        {
            static_domain = new Command(nameof(static_domain));
        }

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

            foreach (Command.Executor executor in pending_executors)
                executor.Dispose();
            pending_executors.Clear();

            for (int i = 0; i < front_janitors.Count; i++)
                front_janitors[i].Dispose();
            front_janitors.Clear();

            for (int i1 = 0; i1 < background_janitors.Count; i1++)
                background_janitors[i1].Dispose();
            background_janitors.Clear();
        }
    }
}