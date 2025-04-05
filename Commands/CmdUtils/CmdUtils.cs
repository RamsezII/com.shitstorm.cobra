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

            Command.cmd_root_shell.AddCommand("shutdown", new Command(
                manual: new("quits the game... :("),
                action: exe => Application.Quit()
                ));

            Command.cmd_root_shell.AddCommand("clear", new Command(
                manual: new("clear all previous logs"),
                action: exe => Application.Quit()
                ));

            Command.cmd_root_shell.AddCommand("clear-history", new Command(
                manual: new("clear all previous entries"),
                action: exe => NUCLEOR.delegates.onStartOfFrame_once += Command.Line.ClearHistory
                ));
        }
    }
}