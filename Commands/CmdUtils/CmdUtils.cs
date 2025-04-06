using _ARK_;
using UnityEngine;

namespace _COBRA_
{
    static internal partial class CmdUtils
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoad()
        {
            InitEcho();
            InitManual();
            InitGrep();

            Command.cmd_root_shell.AddCommand(new(
                "shutdown",
                manual: new("quits the game... :("),
                action: exe => Application.Quit()
                ));

            Command.cmd_root_shell.AddCommand(new(
                "clear",
                manual: new("clear all previous logs"),
                action: exe => Application.Quit()
                ));

            Command.cmd_root_shell.AddCommand(new(
                "clear-history",
                manual: new("clear all previous entries"),
                action: exe => NUCLEOR.delegates.onStartOfFrame_once += Command.Line.ClearHistory
                ));
        }
    }
}