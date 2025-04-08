using _ARK_;
using System.Collections.Generic;
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
            InitEdit();

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

            Command.cmd_root_shell.AddCommand(new(
                "skip",
                manual: new("skip <int> first entries from pipe"),
                pipe_min_args_required: 1,
                args: exe =>
                {
                    if (exe.root.line.TryReadArgument(out string arg))
                        if (int.TryParse(arg, out int count))
                            exe.args.Add(count);
                        else
                            exe.root.error = $"could not parse into int value: '{arg}'";
                    exe.args.Add(0);
                },
                on_pipe: (exe, data) =>
                {
                    int skips = (int)exe.args[0];
                    int iterations = (int)exe.args[1];

                    switch (data)
                    {
                        case IEnumerable<string> strings:
                            foreach (string str in strings)
                                if (iterations++ >= skips)
                                    exe.Stdout(str);
                            break;

                        default:
                            if (iterations++ >= skips)
                                exe.Stdout(data);
                            break;
                    }

                    exe.args[1] = iterations;
                }));
        }
    }
}