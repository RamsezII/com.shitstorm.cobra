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
                    if (exe.line.TryRead_one_flag(exe, flag_grouped))
                        exe.opts.Add(flag_grouped, null);
                },
                args: static exe =>
                {
                    if (exe.line.TryReadArgument(out string var_name, out _))
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
                    int read_i = exe.line.read_i;
                    if (Command.static_domain.TryReadCommand_path(exe.line, out var path))
                    {
                        Command.Executor exe2 = new(exe.shell, exe, exe.line, path);
                        if (exe2.error != null)
                            exe.error = exe2.error;
                        else
                            exe.args.Add(exe.line.text[read_i..exe.line.read_i]);
                        exe2.Dispose();
                    }
                },
                on_pipe: static (exe, data) =>
                {
                    string cmd_line = (string)exe.args[0];
                    Command.Line subline = new(cmd_line, exe.line.signal, exe.line.shell);

                    if (Command.static_domain.TryReadCommand_path(subline, out var path))
                    {
                        Command.Executor do_exe = new(exe.shell, exe, subline, path);
                        if (do_exe.error != null)
                            exe.error = do_exe.error;
                        else
                            exe.janitor.AddExecutor(exe.line, do_exe);
                    }
                }
            );
        }
    }
}