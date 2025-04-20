namespace _COBRA_
{
    partial class CmdVars
    {
        static void Init_For()
        {
            Command.static_domain.AddPipe(
                "as",
                manual: new("put the piped data in a <var_name>"),
                min_args: 1,
                args: static exe =>
                {
                    if (exe.line.TryReadArgument(out string var_name, out _))
                        exe.args.Add(var_name);
                },
                on_pipe: static (exe, data) =>
                {
                    string var_name = (string)exe.args[0];
                    exe.shell.shell_vars[var_name] = data;
                    exe.Stdout(data);
                    exe.shell.shell_vars.Remove(var_name);
                }
            );

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

            Command.static_domain.AddPipe(
                "as-do",
                manual: new("as <var_name> <command>"),
                min_args: 2,
                args: static exe =>
                {
                    if (exe.line.TryReadArgument(out string var_name, out _))
                    {
                        exe.args.Add(var_name);

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
                    }
                },
                on_pipe: static (exe, data) =>
                {
                    string var_name = (string)exe.args[0];
                    string cmd_line = (string)exe.args[1];
                    Command.Line subline = new(cmd_line, exe.line.signal, exe.line.shell);

                    exe.shell.shell_vars[var_name] = data;

                    if (Command.static_domain.TryReadCommand_path(subline, out var path))
                    {
                        Command.Executor do_exe = new(exe.shell, exe, subline, path);
                        if (do_exe.error != null)
                            exe.error = do_exe.error;
                        else
                            exe.janitor.AddExecutor(exe.line, do_exe);
                    }

                    exe.shell.shell_vars.Remove(var_name);
                }
            );
        }
    }
}