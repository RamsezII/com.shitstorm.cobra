using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace _COBRA_
{
    partial class CmdUtils
    {
        static void InitManual()
        {
            Command.cmd_root_shell.AddCommand("manual", new Command(
                manual: new("Of the whats to and the hows to... nowamsayn [burp]"),
                args: exe =>
                {
                    if (Command.cmd_root_shell.TryReadCommand(exe.line, out var path))
                        exe.args.Add(path);
                },
                action: exe =>
                {
                    if (exe.args.Count > 0)
                        Debug.Log(((List<KeyValuePair<string, Command>>)exe.args[0])[^1].Value.manual);
                    else
                    {
                        var groupedByValue = Command.cmd_root_shell._commands.GroupBy(pair => pair.Value);
                        foreach (var group in groupedByValue)
                        {
                            StringBuilder sb = new();
                            foreach (var pair in group)
                                sb.Append($"{pair.Key}, ");

                            sb.Remove(sb.Length - 2, 2);
                            Debug.Log($"{sb}: {group.Key.manual}");
                        }
                    }
                }
            ), 
            "help");
        }
    }
}