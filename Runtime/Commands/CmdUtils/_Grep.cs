using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace _COBRA_
{
    partial class CmdUtils
    {
        static void InitGrep()
        {
            Command.static_domain.AddPipe(
                "grep",
                manual: new("regex filter"),
                min_args: 1,
                args: static exe =>
                {
                    if (exe.signal.TryReadArgument(out string arg, out _))
                        exe.args.Add(arg);
                },
                on_pipe: static (exe, data) =>
                {
                    Regex regex = new((string)exe.args[0], RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                    StringBuilder sb = new();
                    foreach (string str in data.IterateThroughData_str())
                        if (regex.IsMatch(str))
                            sb.AppendLine(str);
                    if (sb.Length > 0)
                        exe.Stdout(sb.TroncatedForLog());
                },
                aliases: "regex");
        }
    }
}