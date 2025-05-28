namespace _COBRA_
{
    partial class CmdUtils
    {
        static void Init_Take()
        {
            Command.static_domain.AddPipe(
                "take",
                max_args: 1,
                opts: static exe => exe.opts.Add("i", 0),
                args: static exe =>
                {
                    if (exe.signal.TryReadArgument(out string arg, out _))
                        if (int.TryParse(arg, out int count))
                            exe.args.Add(count);
                        else
                            exe.error = $"could not parse into int value: '{arg}'";
                },
                on_pipe: static (exe, data) =>
                {
                    int skips = (int)exe.args[0];
                    int iterations = (int)exe.opts["i"];

                    foreach (object o in data.IterateThroughData())
                        if (iterations++ < skips)
                            exe.Stdout(o);

                    exe.opts["i"] = iterations;
                },
                aliases: "head"
                );
        }
    }
}