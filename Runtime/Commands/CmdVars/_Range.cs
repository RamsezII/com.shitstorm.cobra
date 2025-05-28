namespace _COBRA_
{
    partial class CmdVars
    {
        static void Init_Range()
        {
            Command.static_domain.AddAction(
                "range",
                manual: new("<min><pad><max>"),
                min_args: 1,
                max_args: 3,
                args: static exe =>
                {
                    for (int i = 0; i < 3; ++i)
                        if (exe.signal.TryReadArgument(out string arg, out _))
                            exe.args.Add(arg);
                        else
                            break;
                },
                action: static exe =>
                {
                    int count = exe.args.Count;
                    int min = 0, increment = 1, max = 0;

                    switch (count)
                    {
                        case 3:
                            min = int.Parse((string)exe.args[0]);
                            increment = int.Parse((string)exe.args[1]);
                            max = int.Parse((string)exe.args[2]);
                            break;

                        case 2:
                            min = int.Parse((string)exe.args[0]);
                            max = int.Parse((string)exe.args[1]);
                            break;
                        case 1:
                            max = int.Parse((string)exe.args[0]);
                            break;
                    }

                    for (int i = min; i < max; i += increment)
                        exe.Stdout(i);
                });
        }
    }
}