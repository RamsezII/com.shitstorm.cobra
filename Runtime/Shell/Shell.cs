using _ARK_;
using System.Collections.Generic;
using UnityEngine;

namespace _COBRA_
{
    public sealed partial class Shell : MonoBehaviour
    {
        public static readonly Command static_domain;

        readonly Queue<Command.Executor> pending_executors = new();
        readonly List<Command.Executor.Janitor> front_janitors = new();
        internal static readonly List<Command.Executor.Janitor> background_janitors = new();

        static byte id_counter = 0;
        public readonly byte shell_ID = ++id_counter;

        public CMD_STATUS current_status;
        public CMD_STATES previous_state;
        public bool state_changed;
        public bool IsIdle => front_janitors.Count == 0;
        public bool IsBusy => front_janitors.Count > 0;
        public ITerminal terminal;
        public override string ToString() => $"{GetType().FullName}[{shell_ID}]";

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
            OnNucleorQuit();
        }

        //--------------------------------------------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoad()
        {
            NUCLEOR.delegates.onApplicationQuit -= OnNucleorQuit;
            NUCLEOR.delegates.onApplicationQuit += OnNucleorQuit;
            NUCLEOR.delegates.shell_tick -= UpdateBackgroundJanitors;
            NUCLEOR.delegates.shell_tick += UpdateBackgroundJanitors;
        }

        //--------------------------------------------------------------------------------------------------------------

        private void Awake()
        {
            terminal = GetComponentInParent<ITerminal>();
            NUCLEOR.delegates.shell_tick += UpdateUpdateJanitors;
        }

        //--------------------------------------------------------------------------------------------------------------

        private void OnDestroy()
        {
            NUCLEOR.delegates.shell_tick -= UpdateUpdateJanitors;

            foreach (Command.Executor executor in pending_executors)
                executor.Dispose();
            pending_executors.Clear();

            for (int i = 0; i < front_janitors.Count; i++)
                front_janitors[i].Dispose();
            front_janitors.Clear();
        }

        static void OnNucleorQuit()
        {
            NUCLEOR.delegates.shell_tick -= UpdateBackgroundJanitors;
            for (int i = 0; i < background_janitors.Count; i++)
                background_janitors[i].Dispose();
            background_janitors.Clear();
        }
    }
}