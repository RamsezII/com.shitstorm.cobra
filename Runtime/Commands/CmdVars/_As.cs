namespace _COBRA_
{
    partial class CmdVars
    {
        static void Init_For()
        {
            const string
                flag_grouped = "--grouped";

            Command.static_domain.AddPipe(
                "as",
                manual: new($"named pipe: <var_name> {{{flag_grouped}}}"),
                min_args: 1,
                opts: static exe =>
                {
                    if (exe.signal.TryRead_one_flag(exe, flag_grouped))
                        exe.opts.Add(flag_grouped, null);
                },
                args: static exe =>
                {
                    if (exe.signal.TryReadArgument(out string var_name, out _))
                        exe.args.Add(var_name);
                },
                on_pipe: static (exe, data) =>
                {
                    bool grouped = exe.opts.ContainsKey(flag_grouped);
                    string var_name = (string)exe.args[0];

                    if (grouped)
                    {
                        exe.shell.shell_vars[var_name] = data;
                        exe.Stdout(data);
                    }
                    else
                        foreach (object o in data.IterateThroughData())
                        {
                            exe.shell.shell_vars[var_name] = o;
                            exe.Stdout(o);
                        }

                    //exe.shell.shell_vars.Remove(var_name);
                });

            Command.static_domain.AddPipe(
                "do",
                manual: new("as <var_name> <command>"),
                min_args: 1,
                args: static exe =>
                {
                    int read_i = exe.signal.read_i;
                    if (Command.static_domain.TryReadCommand_path(exe.signal, out var path))
                    {
                        Command.Executor exe2 = new(exe.shell, exe, exe.signal, path);
                        if (exe2.error != null)
                            exe.error = exe2.error;
                        else
                            exe.args.Add(exe.signal.text[read_i..exe.signal.read_i]);
                        exe2.Dispose();
                    }
                },
                on_pipe: static (exe, data) =>
                {
                    string cmd_line = (string)exe.args[0];
                    Command.Signal signal = new(cmd_line, exe.signal?.flags ?? SIG_FLAGS.EXEC, exe.signal?.shell ?? exe.shell);

                    if (Command.static_domain.TryReadCommand_path(signal, out var path))
                    {
                        Command.Executor do_exe = new(exe.shell, exe, signal, path);
                        if (do_exe.error != null)
                            exe.error = do_exe.error;
                        else
                            exe.janitor.AddExecutor(exe.signal, do_exe);
                    }
                }
            );
        }
    }
}