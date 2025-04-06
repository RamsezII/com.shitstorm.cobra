using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace _COBRA_
{
    partial class CmdUtils
    {
        static void InitGrep()
        {
            Command.cmd_root_shell.AddCommand(new(
                "grep",
                manual: new("regex filter"),
                pipe_min_args_required: 1,
                args: exe =>
                {
                    if (exe.line.TryReadArgument(out string arg))
                        exe.args.Add(arg);
                },
                on_pipe: (exe, data) =>
                {
                    Regex regex = new((string)exe.args[0], RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                    switch (data)
                    {
                        case string str:
                            if (regex.IsMatch(str))
                                exe.Stdout(str);
                            break;

                        case IEnumerable<object> objects:
                            {
                                List<object> filtered = new();
                                foreach (object obj in objects)
                                {
                                    string str = obj switch
                                    {
                                        string s => s,
                                        _ => obj.ToString()
                                    };
                                    if (regex.IsMatch(str))
                                        filtered.Add(obj);
                                }
                                if (filtered.Count > 0)
                                    exe.Stdout(filtered.LinesToText());
                            }
                            break;

                        default:
                            {
                                string str = data.ToString();
                                if (regex.IsMatch(str))
                                    exe.Stdout(str);
                            }
                            break;
                    }
                }
            ), "regex");
        }
    }
}