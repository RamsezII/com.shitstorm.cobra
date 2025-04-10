using _ARK_;
using UnityEngine;

namespace _COBRA_
{
    partial class Command
    {
        public sealed partial class Line
        {
            public readonly ITerminal terminal;
            public readonly Linter linter = new();
            public string text;
            public bool notEmpty;
            public int cpl_index;
            public CMD_SIGNALS signal;
            public int cursor_i, read_i, start_i, end_i, arg_i = -1, cpl_start_i;
            public string arg_last;
            public bool HasFlags_any(in CMD_SIGNALS flags) => (signal & flags) != 0;

            //--------------------------------------------------------------------------------------------------------------

            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
            static void OnBeforeSceneLoad()
            {
                LoadHistory();
            }

            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
            static void OnAfterSceneLoad()
            {
                NUCLEOR.delegates.onApplicationQuit += SaveHistory;
            }

            //--------------------------------------------------------------------------------------------------------------

            public Line(in string text, in CMD_SIGNALS signal, in ITerminal terminal, in int cursor_i = default, in int cpl_index = default)
            {
                notEmpty = !string.IsNullOrWhiteSpace(text);
                this.text = notEmpty ? text : string.Empty;
                this.signal = signal;
                this.terminal = terminal;
                this.cursor_i = cursor_i;
                this.cpl_index = cpl_index;

                if (terminal == null)
                    Debug.LogWarning("null terminal");

                linter = terminal?.Linter;
                linter?.Clear();
            }
        }
    }
}