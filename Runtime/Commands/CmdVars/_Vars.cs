namespace _COBRA_
{
    partial class CmdVars
    {
        static Command cmd_setvar;

        //--------------------------------------------------------------------------------------------------------------

        static void Init_Vars()
        {
            cmd_setvar = Command.static_domain.AddAction(
                "set-shell-var",
                min_args: 2,
                args: static exe =>
                {
                    if (exe.signal.TryReadArgument(out string name, out _, exe.shell.shell_vars.Keys))
                        exe.args.Add(name);
                    if (exe.signal.TryReadArgument(out string value, out _))
                        exe.args.Add(value);
                },
                action: static exe =>
                {
                    string name = (string)exe.args[0];
                    object value = exe.args[1];
                    exe.shell.shell_vars[name] = value;
                });

            cmd_setvar = Command.static_domain.AddAction(
                "set-global-var",
                min_args: 2,
                args: static exe =>
                {
                    if (exe.signal.TryReadArgument(out string name, out _, Shell.global_vars.Keys))
                        exe.args.Add(name);
                    if (exe.signal.TryReadArgument(out string value, out _))
                        exe.args.Add(value);
                },
                action: static exe =>
                {
                    string name = (string)exe.args[0];
                    object value = exe.args[1];
                    Shell.global_vars[name] = value;
                });
        }
    }
}