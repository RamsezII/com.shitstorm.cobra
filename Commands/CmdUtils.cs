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

            Command.cmd_root_shell.AddCommand(new Command(
                manual: new("quits the game... :("),
                action: exe => Application.Quit()
                ),
                "shutdown");

            Command.cmd_root_shell.AddCommand(new Command(
                manual: new("clear all previous logs"),
                action: exe => Application.Quit()
                ),
                "clear");
        }
    }
}