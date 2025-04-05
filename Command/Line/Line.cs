using _ARK_;
using UnityEngine;

namespace _COBRA_
{
    partial class Command
    {
        public sealed partial class Line
        {
            public static readonly Line EMPTY_EXE = new(default, CMD_SIGNALS.EXEC);

            public string text;
            public bool notEmpty;
            public int cpl_index;
            public CMD_SIGNALS signal;
            public int cursor_i, read_i, start_i, arg_i = -1, cpl_start_i;
            public string arg_last;
            public bool IsCplThis => signal >= CMD_SIGNALS.TAB && cursor_i >= start_i && (cursor_i < read_i || cursor_i == read_i && cursor_i == text.Length);

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

            public Line(in string text, in CMD_SIGNALS signal, in int cursor_i = default, in int cpl_index = default)
            {
                notEmpty = !string.IsNullOrWhiteSpace(text);
                this.text = notEmpty ? text : string.Empty;
                this.signal = signal;
                this.cursor_i = cursor_i;
                this.cpl_index = cpl_index;
            }
        }
    }
}