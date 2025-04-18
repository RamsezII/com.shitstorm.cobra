namespace _COBRA_
{
    partial class CmdVars
    {
        static void Init_For()
        {
            Command.static_domain.AddPipe(
                "select",
                min_args: 1,
                args: static exe =>
                {
                    if (exe.line.TryReadArgument(out string arg, out _))
                        exe.args.Add(arg);
                },
                on_pipe: static (exe, data) =>
                {
                    string var_name = (string)exe.args[0];
                    exe.janitor.temp_vars[var_name] = data;
                }
            );

            Command.static_domain.AddPipe(
                "do",
                min_args: 1,
                args: static exe =>
                {
                    if (Command.static_domain.TryReadCommand_path(exe.line, out var path))
                    {
                        Command.Executor exe2 = new(exe.shell, exe, exe.line, path);
                        if (exe2.error != null)
                            exe.error = exe2.error;
                        else
                            exe.args.Add(path);
                        exe.Dispose();
                    }
                },
                on_pipe: static (exe, data) =>
                {
                    Command.Executor exe2 = (Command.Executor)exe.args[0];
                    Command.Executor exe3 = new(exe.shell, exe, exe.line, exe2.path);
                    exe.janitor.AddExecutor(exe3);
                }
            );
        }
    }
}