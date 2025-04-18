using System.Linq;
using System.Text;
using UnityEngine;

namespace _COBRA_
{
    partial class CmdUtils
    {
        static void InitManual()
        {
            Command.static_domain.AddAction(
                "manual",
                manual: new("Of the whats to and the hows to... nowamsayn [burp]"),
                max_args: 2,
                args: exe =>
                {
                    if (Command.static_domain.TryReadCommand_path(exe.line, out var path))
                    {
                        exe.args.Add(path[^1]);
                        exe.args.Add(path.Select(cmd => cmd.name).Join("/"));
                    }
                },
                action: exe =>
                {
                    if (exe.args.Count > 0)
                        Debug.Log(((Command)exe.args[0]).manual);
                    else
                    {
                        StringBuilder sb = new(
@"Of the whats to and the hows to... nowamsayn [burp]

command arguments work as this :
  <...> = required
  [...] = optional
  {a|b} = choice
  ... = repeatable\n\n"
                        );

                        sb.AppendLine("Commands :");

                        foreach (var group in Command.static_domain._commands.GroupBy(pair => pair.Value))
                        {
                            foreach (var pair in group)
                                sb.Append($"{pair.Key}, ");

                            sb.Remove(sb.Length - 2, 2);
                            sb.AppendLine(": " + group.Key.manual);
                        }

                        exe.Stdout(sb.TroncatedForLog());
                    }
                },
                aliases: "help");
        }
    }
}