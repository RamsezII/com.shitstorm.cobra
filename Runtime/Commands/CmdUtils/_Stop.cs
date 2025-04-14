using System.Collections.Generic;

namespace _COBRA_
{
    partial class CmdUtils
    {
        static void Init_Stop()
        {
            Command.static_domain.AddPipe(
                "stop",
                max_args: 1,
                opts: static exe => exe.opts.Add("i", 0),
                args: static exe =>
                {
                    if (exe.line.TryReadArgument(out string arg))
                        if (int.TryParse(arg, out int count))
                            exe.args.Add(count);
                        else
                            exe.error = $"could not parse into int value: '{arg}'";
                },
                on_pipe: static (exe, data) =>
                {
                    int skips = (int)exe.args[0];
                    int iterations = (int)exe.opts["i"];

                    bool Check() => iterations++ < skips;

                    switch (data)
                    {
                        case string str:
                            foreach (string line in str.TextToLines(true))
                                if (Check())
                                    exe.Stdout(line);
                            break;

                        case IEnumerable<object> objects:
                            foreach (object obj in objects)
                                if (Check())
                                    exe.Stdout(obj);
                            break;

                        default:
                            if (Check())
                                exe.Stdout(data);
                            break;
                    }

                    exe.opts["i"] = iterations;
                },
                aliases: "head"
                );
        }
    }
}